using System.Threading;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using Tatehama_tetudou_denwa_PCclient.Models;
using Tatehama_tetudou_denwa_PCclient.Views;
using System.Diagnostics;

namespace Tatehama_tetudou_denwa_PCclient
{
    public partial class MainWindow : Window
    {
        // --- 旧実装の全機能を復元 ---
        // 変数宣言
        private string? incomingCallerNumber;
        private CancellationTokenSource? aitenashiCancelToken;
        private enum PhoneState { Idle, ReadyToDial, Dialing, InCall, Busy, Ringing }
        private PhoneState currentState = PhoneState.Idle;
        private CallListItem? myLocation;
        private WaveOutEvent? loopingDevice;
        private WaveInEvent? waveIn;
        private BufferedWaveProvider? bufferedWaveProvider;
        private Dictionary<string, WaveFileReader> soundCache = new Dictionary<string, WaveFileReader>();
        private ImageBrush? brushJyuwa, brushJyuwaAka, brushJyuwaKiro, brushJyuwaAo, brushSyuwa, brushSyuwaAka, brushSyuwaAo, brushHashin, brushHashinAo;
        private DispatcherTimer? inCallDurationTimer, flashingTimer, ringingTimer;
        private TimeSpan inCallDuration;
        private System.Windows.Forms.NotifyIcon? notifyIcon;
        private bool isOutgoingCall = false;
        private bool isHashinFlashing, isSyuwaFlashing, isJyuwaFlashing;
        private bool flashToggle = false;
        private Models.ServerConnection? serverConnection;

        public MainWindow()
        {
            InitializeComponent();
            InitializeMedia();
            InitializeNotifyIcon();
            while (myLocation == null)
            {
                if (!SelectLocation(true))
                {
                    System.Windows.Application.Current.Shutdown();
                    return;
                }
            }
            this.Title = $"館浜鉄道電話 - {myLocation.DisplayName}";
            inCallDurationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            inCallDurationTimer.Tick += InCallDurationTimer_Tick;
            flashingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            flashingTimer.Tick += FlashingTimer_Tick;
            ringingTimer = new DispatcherTimer();
            ringingTimer.Tick += RingingTimer_Tick;
            SetState(PhoneState.Idle);
            this.Closing += (s, e) => {
                StopAllSounds();
                loopingDevice?.Dispose();
                waveIn?.Dispose();
                notifyIcon?.Dispose();
                foreach (var sound in soundCache.Values) sound.Dispose();
            };
            serverConnection = new Models.ServerConnection();
            string serverUrl = "http://localhost:5148/connectionHub";
            string myPhoneNumber = myLocation != null ? myLocation.PhoneNumber : "";
            _ = serverConnection.ConnectAsync(serverUrl, myPhoneNumber).ContinueWith(async t =>
            {
                if (serverConnection != null)
                    await serverConnection.RegisterPhoneNumber(myPhoneNumber);

                // サーバーから「不在」通知
                serverConnection.NoAnswerReceived += async () =>
                {
                    await Dispatcher.InvokeAsync(async () => await PlayAitenashiSequence());
                    Dispatcher.Invoke(() => {
                        SetState(PhoneState.Idle);
                        incomingCallerNumber = null;
                    });
                };

                // サーバーから「着信」通知
                serverConnection.RingingReceived += (callerNumber) =>
                {
                    Dispatcher.Invoke(() => {
                        // 既に通話中・着信中なら無視（多重着信防止）
                        if (currentState == PhoneState.InCall || currentState == PhoneState.Ringing)
                            return;
                        incomingCallerNumber = callerNumber;
                        SetState(PhoneState.Ringing, callerNumber);
                    });
                };

                // サーバーから「通話確立」通知
                serverConnection.CallOkReceived += (calleeNumber) =>
                {
                    Dispatcher.Invoke(() => {
                        // 通話確立時は着信番号をクリア
                        incomingCallerNumber = null;
                        SetState(PhoneState.InCall, calleeNumber);
                    });
                };

                // サーバーから「終話」通知
                serverConnection.EndCallReceived += () =>
                {
                    Dispatcher.Invoke(() => {
                        PlaySfx("kire.wav");
                        SetState(PhoneState.Idle);
                        incomingCallerNumber = null;
                    });
                };
            });
        }

        private async Task PlayAitenashiSequence()
        {
            StopAllSounds();
            aitenashiCancelToken?.Cancel();
            aitenashiCancelToken = new CancellationTokenSource();
            var token = aitenashiCancelToken.Token;
            await Task.Delay(100); // 呼出中音停止後の待機
            PlaySfx("tori.wav");
            await Task.Delay(500);
            var rand = new Random();
            string aitenashi = rand.NextDouble() < 0.9 ? "aitenashi_1.wav" : "aitenashi_2.wav";
            PlaySfx(aitenashi);
            await Task.Delay(1500);
            Dispatcher.Invoke(() => {
                NumberDisplay.Text = "不在";
                NumberDisplay.FontSize = 24;
                NumberDisplay.Visibility = Visibility.Visible;
            });
            if (!token.IsCancellationRequested)
                PlaySfx("kire.wav");
            if (!token.IsCancellationRequested)
                await Task.Delay(300);
            if (!token.IsCancellationRequested)
                PlaySfx("kire2.wav");
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(350);
                if (token.IsCancellationRequested) break;
                PlaySfx("kire2.wav");
            }
        }

        private void InitializeNotifyIcon()
        {
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "image/tetudoudenwa.png");
                using (var bitmap = new Bitmap(iconPath))
                {
                    IntPtr hIcon = bitmap.GetHicon();
                    notifyIcon = new System.Windows.Forms.NotifyIcon
                    {
                        Icon = System.Drawing.Icon.FromHandle(hIcon),
                        Visible = true,
                        Text = "館浜鉄道電話"
                    };
                }
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show($"通知アイコンの初期化に失敗しました。");
            }
        }

        private bool SelectLocation(bool isInitial)
        {
            LocationSelector selector = new LocationSelector();
            if (selector.ShowDialog() == true && selector.SelectedLocation != null)
            {
                myLocation = selector.SelectedLocation;
                this.Title = $"館浜鉄道電話 - {myLocation.DisplayName}";
                PopulateCallList();
                return true;
            }
            else
            {
                if (isInitial) return false;
                return true;
            }
        }

        private ImageBrush? LoadImageBrushFromFile(string path)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                using (FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                return new ImageBrush(bitmap) { Stretch = Stretch.Fill };
            }
            catch (Exception) { System.Windows.MessageBox.Show($"画像読込エラー: " + path); return null; }
        }

        private void InitializeMedia()
        {
            try
            {
                StopAllSounds();
                loopingDevice?.Dispose();
                waveIn?.Dispose();
                if (WaveOut.DeviceCount > 0)
                {
                    loopingDevice = new WaveOutEvent { DeviceNumber = AudioSettings.OutputDeviceNumber };
                }
                if (WaveIn.DeviceCount > 0)
                {
                    waveIn = new WaveInEvent { DeviceNumber = AudioSettings.InputDeviceNumber };
                    waveIn.WaveFormat = new WaveFormat(8000, 16, 1);
                    bufferedWaveProvider = new BufferedWaveProvider(waveIn.WaveFormat);
                    waveIn.DataAvailable += WaveIn_DataAvailable;
                    if (loopingDevice != null)
                    {
                        loopingDevice.Init(bufferedWaveProvider);
                    }
                }
                if(soundCache.Count == 0)
                {
                    string soundDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sound");
                    string[] soundFiles = { "push.wav", "yobidashityuu.wav", "watyu.wav", "beru.wav", "denshiberu.wav", "tori.wav", "oki.wav", "kire.wav", "kire2.wav", "aitenashi_1.wav", "aitenashi_2.wav" };
                    foreach (var file in soundFiles)
                    {
                        soundCache[file] = new WaveFileReader(Path.Combine(soundDir, file));
                    }
                    brushJyuwa = LoadImageBrushFromFile("image/jyuwa.png");
                    brushJyuwaAka = LoadImageBrushFromFile("image/jyuwa-aka.png");
                    brushJyuwaKiro = LoadImageBrushFromFile("image/jyuwa-kiro.png");
                    brushJyuwaAo = LoadImageBrushFromFile("image/jyuwa-ao.png");
                    brushSyuwa = LoadImageBrushFromFile("image/syuwa.png");
                    brushSyuwaAka = LoadImageBrushFromFile("image/syuwa-aka.png");
                    brushSyuwaAo = LoadImageBrushFromFile("image/syuwa-ao.png");
                    brushHashin = LoadImageBrushFromFile("image/hashin.png");
                    brushHashinAo = LoadImageBrushFromFile("image/hashin-ao.png");
                }
            }
            catch (Exception) { System.Windows.MessageBox.Show($"メディア初期化エラー"); }
        }

        private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
        {
            bufferedWaveProvider?.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }

        private void PopulateCallList()
        {
            CallList?.Items.Clear();
            var allLocations = LocationData.GetLocations();
            foreach (var location in allLocations)
            {
                CallList?.Items.Add(location);
            }
        }

        private void PlaySfx(string key)
        {
            try
            {
                if (WaveOut.DeviceCount == 0)
                {
                    System.Windows.MessageBox.Show("サウンドデバイスが見つかりません。設定画面で出力デバイスを確認してください。", "サウンドエラー");
                    return;
                }
                if (!soundCache.ContainsKey(key))
                {
                    System.Windows.MessageBox.Show($"サウンドファイル '{key}' が見つかりません。", "サウンドエラー");
                    return;
                }
                var sound = soundCache[key];
                sound.Position = 0;
                var tempDevice = new WaveOutEvent { DesiredLatency = 100, DeviceNumber = AudioSettings.OutputDeviceNumber };
                tempDevice.Init(sound);
                tempDevice.PlaybackStopped += (s, e) => tempDevice.Dispose();
                tempDevice.Play();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"サウンド再生エラー: {ex.Message}", "サウンドエラー");
            }
        }

        private void PlayLoopingSfx(string key)
        {
            if (loopingDevice == null || WaveOut.DeviceCount == 0 || !soundCache.ContainsKey(key)) return;
            if (loopingDevice.PlaybackState == PlaybackState.Playing) loopingDevice.Stop();
            var sound = soundCache[key];
            sound.Position = 0;
            var loopStream = new LoopStream(sound);
            loopingDevice.Init(loopStream);
            loopingDevice.Play();
        }

        private void StopAllSounds()
        {
            loopingDevice?.Stop();
            waveIn?.StopRecording();
            if (bufferedWaveProvider != null) bufferedWaveProvider.ClearBuffer();
        }

        private void PlayRingingSound()
        {
            string soundToPlay = "denshiberu.wav";
            var allLocations = LocationData.GetLocations();
            if (myLocation != null)
            {
                int myIndex = allLocations.FindIndex(loc => loc.PhoneNumber == myLocation.PhoneNumber);
                if (myIndex != -1 && myIndex < 4)
                {
                    soundToPlay = "beru.wav";
                }
            }
            if (soundCache.TryGetValue(soundToPlay, out var sound))
            {
                PlaySfx(soundToPlay);
                if (ringingTimer == null) ringingTimer = new DispatcherTimer();
                ringingTimer.Interval = sound.TotalTime.Add(TimeSpan.FromSeconds(1));
            }
        }

        private void SetState(PhoneState newState, string? callerName = null)
        {
            ringingTimer?.Stop();
            StopAllSounds();
            inCallDurationTimer?.Stop();
            flashingTimer?.Stop();
            isHashinFlashing = isSyuwaFlashing = isJyuwaFlashing = false;
            NumberDisplay.Visibility = Visibility.Visible;
            CallTimerDisplay.Visibility = Visibility.Collapsed;
            NumberDisplay.FontSize = 30;
            jyuwaButton.Background = brushJyuwa;
            syuwaButton.Background = brushSyuwa;
            hashinButton.Background = brushHashin;
            currentState = newState;
            switch (currentState)
            {
                case PhoneState.Idle:
                    NumberDisplay.Text = "";
                    break;
                case PhoneState.ReadyToDial:
                    if (NumberDisplay.Text.Length == 4) { isHashinFlashing = true; flashingTimer?.Start(); }
                    break;
                case PhoneState.Dialing:
                    hashinButton.Background = brushHashinAo;
                    isSyuwaFlashing = true;
                    flashingTimer?.Start();
                    PlayLoopingSfx("yobidashityuu.wav");
                    break;
                case PhoneState.InCall:
                    loopingDevice?.Stop();
                    syuwaButton.Background = brushSyuwaAka;
                    if (isOutgoingCall) { hashinButton.Background = brushHashinAo; }
                    else { jyuwaButton.Background = brushJyuwaAo; }
                    CallTimerDisplay.Visibility = Visibility.Visible;
                    inCallDuration = TimeSpan.Zero;
                    CallTimerDisplay.Text = "00:00";
                    inCallDurationTimer?.Start();
                    waveIn?.StartRecording();
                    break;
                case PhoneState.Busy:
                    NumberDisplay.Text = "話し中";
                    NumberDisplay.FontSize = 24;
                    PlaySfx("watyu.wav");
                    break;
                case PhoneState.Ringing:
                    NumberDisplay.Text = "着信中";
                    jyuwaButton.Background = brushJyuwaAo;
                    loopingDevice?.Stop();
                    isJyuwaFlashing = true;
                    isSyuwaFlashing = true;
                    flashingTimer?.Start();
                    PlayRingingSound();
                    ringingTimer?.Start();
                    ShowNotification(callerName ?? "不明な発信元");
                    incomingCallerNumber = callerName;
                    break;
            }
        }

        private void ShowNotification(string callerName)
        {
            if (notifyIcon != null)
            {
                notifyIcon.ShowBalloonTip(3000, "着信", $"{callerName}から着信", System.Windows.Forms.ToolTipIcon.Info);
            }
        }

        private void FlashingTimer_Tick(object? sender, EventArgs e)
        {
            flashToggle = !flashToggle;
            if (isHashinFlashing)
            {
                hashinButton.Background = flashToggle ? brushHashinAo : brushHashin;
            }
            if (isSyuwaFlashing)
            {
                syuwaButton.Background = flashToggle ? brushSyuwaAka : brushSyuwa;
            }
            if (isJyuwaFlashing)
            {
                jyuwaButton.Background = flashToggle ? brushJyuwaAo : brushJyuwa;
            }
        }

        private void RingingTimer_Tick(object? sender, EventArgs e)
        {
            PlayRingingSound();
        }

        private void InCallDurationTimer_Tick(object? sender, EventArgs e)
        {
            inCallDuration = inCallDuration.Add(TimeSpan.FromSeconds(1));
            CallTimerDisplay.Text = inCallDuration.ToString(@"mm\:ss");
        }

        private void ShowSelfCallWarning()
        {
            var messages = new List<string>
            {
                "何なんだね？その発信先は？",
                "発信先良いか？",
                "発信先良いか？",
                "違うよ？",
                "混線させる気かい？",
                "混線させる気かい？"
            };
            System.Windows.MessageBox.Show(messages[new Random().Next(messages.Count)], "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void AppendNumber(string number)
        {
            if (currentState != PhoneState.Idle && currentState != PhoneState.ReadyToDial) return;
            PlaySfx("push.wav");
            if (NumberDisplay.Text.Length < 4)
            {
                if (currentState == PhoneState.Idle) NumberDisplay.Text = "";
                NumberDisplay.Text += number;
                SetState(PhoneState.ReadyToDial);
            }
        }

        private void syoukyoButton_Click(object sender, RoutedEventArgs e)
        {
            PlaySfx("push.wav");
            if (NumberDisplay.Text.Length > 0 && (currentState == PhoneState.Idle || currentState == PhoneState.ReadyToDial))
            {
                NumberDisplay.Text = NumberDisplay.Text.Substring(0, NumberDisplay.Text.Length - 1);
                if (NumberDisplay.Text.Length == 0) SetState(PhoneState.Idle);
                else SetState(PhoneState.ReadyToDial);
            }
        }

        private void CallList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CallList?.SelectedItem is CallListItem selectedItem) { NumberDisplay.Text = selectedItem.PhoneNumber; SetState(PhoneState.ReadyToDial); }
        }

        private void jyuwaButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentState == PhoneState.Ringing)
            {
                PlaySfx("tori.wav");
                isOutgoingCall = false;
                if (serverConnection != null && myLocation != null && !string.IsNullOrEmpty(incomingCallerNumber))
                {
                    // RegisterPhoneNumber後にconnid:xxxが返るのでそれを使う
                    string myConnId = serverConnection.GetConnectionId() ?? "";
                    string peerConnId = incomingCallerNumber; // RingingReceivedでConnectionIdを受け取る
                    if (!string.IsNullOrEmpty(myConnId) && !string.IsNullOrEmpty(peerConnId))
                        _ = serverConnection.AnswerCall(myConnId, peerConnId);
                }
                SetState(PhoneState.InCall);
            }
        }

        private void syuwaButton_Click(object sender, RoutedEventArgs e)
        {
            aitenashiCancelToken?.Cancel();
            StopAllSounds();
            PlaySfx("oki.wav");
            string targetNumber = incomingCallerNumber;
            // 通話相手番号が未設定の場合はNumberDisplay.Textを使う
            if (string.IsNullOrEmpty(targetNumber))
                targetNumber = NumberDisplay.Text;
            if (serverConnection != null && myLocation != null && !string.IsNullOrEmpty(targetNumber))
            {
                _ = serverConnection.EndCall(myLocation.PhoneNumber, targetNumber);
            }
            SetState(PhoneState.Idle);
        }

        private void hashinButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentState == PhoneState.ReadyToDial && !string.IsNullOrEmpty(NumberDisplay.Text))
            {
                if (NumberDisplay.Text == myLocation?.PhoneNumber)
                {
                    ShowSelfCallWarning();
                    return;
                }
                PlaySfx("tori.wav");
                isOutgoingCall = true;
                SetState(PhoneState.Dialing);
                if (serverConnection != null && myLocation != null)
                {
                    _ = serverConnection.CallRequest(myLocation.PhoneNumber, NumberDisplay.Text);
                }
            }
        }

        private void zeroButton_Click(object sender, RoutedEventArgs e) { AppendNumber("0"); }
        private void oneButton_Click(object sender, RoutedEventArgs e) { AppendNumber("1"); }
        private void twoButton_Click(object sender, RoutedEventArgs e) { AppendNumber("2"); }
        private void threeButton_Click(object sender, RoutedEventArgs e) { AppendNumber("3"); }
        private void fourButton_Click(object sender, RoutedEventArgs e) { AppendNumber("4"); }
        private void fiveButton_Click(object sender, RoutedEventArgs e) { AppendNumber("5"); }
        private void sixButton_Click(object sender, RoutedEventArgs e) { AppendNumber("6"); }
        private void sevenButton_Click(object sender, RoutedEventArgs e) { AppendNumber("7"); }
        private void eightButton_Click(object sender, RoutedEventArgs e) { AppendNumber("8"); }
        private void nineButton_Click(object sender, RoutedEventArgs e) { AppendNumber("9"); }

        private void ChangeWorkLocationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectLocation(false) && myLocation != null)
            {
                this.Title = $"館浜鉄道電話 - {myLocation.DisplayName}";
                if (serverConnection != null)
                    _ = serverConnection.UpdateState(myLocation.PhoneNumber, (Tatehama_tetudou_denwa_PCclient.Models.PhoneState)PhoneState.Idle);
            }
        }

        private void AudioSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            if (settingsWindow.ShowDialog() == true)
            {
                InitializeMedia();
            }
        }
    }
}