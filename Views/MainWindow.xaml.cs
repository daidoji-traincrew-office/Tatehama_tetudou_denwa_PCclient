using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using NAudio.Wave;

namespace Tatehama_tetudou_denwa_PCclient;

/// <summary>
/// Stream for looping playback from NAudio project.
/// </summary>
public class LoopStream : WaveStream
{
    private readonly WaveStream sourceStream;

    public LoopStream(WaveStream sourceStream)
    {
        this.sourceStream = sourceStream;
        this.EnableLooping = true;
    }

    public bool EnableLooping { get; set; }

    public override WaveFormat WaveFormat => sourceStream.WaveFormat;

    public override long Length => sourceStream.Length;

    public override long Position
    {
        get => sourceStream.Position;
        set => sourceStream.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalBytesRead = 0;
        while (totalBytesRead < count)
        {
            int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
            if (bytesRead == 0)
            {
                if (sourceStream.Position == 0 || !EnableLooping)
                {
                    break;
                }
                sourceStream.Position = 0;
            }
            totalBytesRead += bytesRead;
        }
        return totalBytesRead;
    }
}

public class CallListItem
{
    public string DisplayName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public override string ToString()
    {
        return DisplayName;
    }
}

public partial class MainWindow : Window
{
    private enum PhoneState { Idle, ReadyToDial, Dialing, InCall, Busy, Ringing }
    private PhoneState currentState = PhoneState.Idle;

    // Audio
    private WaveOutEvent? loopingDevice;
    private Dictionary<string, WaveFileReader> soundCache = new Dictionary<string, WaveFileReader>();

    // Image Resources
    private ImageBrush? brushJyuwa, brushJyuwaAka, brushJyuwaKiro, brushJyuwaAo, brushSyuwa, brushSyuwaAka, brushSyuwaAo, brushHashin, brushHashinAo;

    // Timers
    private DispatcherTimer callOutcomeTimer, inCallDurationTimer, flashingTimer;
    private TimeSpan inCallDuration;

    // State Flags
    private bool isOutgoingCall = false;
    private bool isHashinFlashing, isSyuwaFlashing, isJyuwaFlashing;
    private bool flashToggle = false;

    public MainWindow()
    {
        InitializeComponent();
        InitializeMedia();
        PopulateCallList();
        
        callOutcomeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        callOutcomeTimer.Tick += CallOutcomeTimer_Tick;

        inCallDurationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        inCallDurationTimer.Tick += InCallDurationTimer_Tick;

        flashingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        flashingTimer.Tick += FlashingTimer_Tick;
        
        SetState(PhoneState.Idle);

        this.Closing += (s, e) => { loopingDevice?.Dispose(); foreach(var sound in soundCache.Values) sound.Dispose(); };
    }

    #region Initialization

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
        catch (Exception ex) { MessageBox.Show($"画像読込エラー: {path}\n{ex.Message}"); return null; }
    }

    private void InitializeMedia()
    {
        try
        {
            loopingDevice = new WaveOutEvent();
            string soundDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sound");
            string[] soundFiles = { "push.wav", "yobidashityuu.wav", "watyu.wav", "beru.wav", "tori.wav", "oki.wav" };
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
        catch (Exception ex) { MessageBox.Show($"メディア初期化エラー: \n{ex.Message}"); }
    }

    private void PopulateCallList()
    {
        var callListItems = new List<CallListItem>
        {
            new CallListItem { DisplayName = "総合司令" },
            new CallListItem { DisplayName = "館浜駅乗務員詰所" },
            new CallListItem { DisplayName = "大道寺列車区" },
            new CallListItem { DisplayName = "赤山町駅乗務員詰所" },
            new CallListItem { DisplayName = "館浜" },
            new CallListItem { DisplayName = "駒野" },
            new CallListItem { DisplayName = "河原崎" },
            new CallListItem { DisplayName = "海岸公園" },
            new CallListItem { DisplayName = "虹ケ浜" },
            new CallListItem { DisplayName = "津崎" },
            new CallListItem { DisplayName = "浜園" },
            new CallListItem { DisplayName = "羽衣橋" },
            new CallListItem { DisplayName = "新井川" },
            new CallListItem { DisplayName = "新野崎" },
            new CallListItem { DisplayName = "江ノ原" },
            new CallListItem { DisplayName = "大道寺" },
            new CallListItem { DisplayName = "藤江" },
            new CallListItem { DisplayName = "水越" },
            new CallListItem { DisplayName = "高見沢" },
            new CallListItem { DisplayName = "日野森" },
            new CallListItem { DisplayName = "奥峰口" },
            new CallListItem { DisplayName = "西赤山" },
            new CallListItem { DisplayName = "赤山町" }
        };

        for (int i = 0; i < callListItems.Count; i++)
        {
            if (i < 4)
            {
                callListItems[i].PhoneNumber = (1001 + i).ToString();
            }
            else
            {
                callListItems[i].PhoneNumber = (2001 + (i - 4)).ToString();
            }
            CallList.Items.Add(callListItems[i]);
        }
    }

    #endregion

    #region Audio Playback

    private void PlaySfx(string key)
    {
        if (!soundCache.ContainsKey(key)) return;
        var sound = soundCache[key];
        sound.Position = 0;
        var tempDevice = new WaveOutEvent { DesiredLatency = 100 };
        tempDevice.Init(sound);
        tempDevice.PlaybackStopped += (s, e) => tempDevice.Dispose();
        tempDevice.Play();
    }

    private void PlayLoopingSound(string key)
    {
        if (loopingDevice == null || !soundCache.ContainsKey(key)) return;
        loopingDevice.Stop();
        var sound = soundCache[key];
        sound.Position = 0;
        var loopStream = new LoopStream(sound);
        loopingDevice.Init(loopStream);
        loopingDevice.Play();
    }

    private void StopAllSounds() { loopingDevice?.Stop(); }

    #endregion

    #region State & UI Management

    private void SetState(PhoneState newState)
    {
        StopAllSounds();
        callOutcomeTimer.Stop();
        inCallDurationTimer.Stop();
        flashingTimer.Stop();
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
                if (NumberDisplay.Text.Length == 4) { isHashinFlashing = true; flashingTimer.Start(); }
                break;
            case PhoneState.Dialing:
                hashinButton.Background = brushHashinAo;
                isSyuwaFlashing = true;
                flashingTimer.Start();
                PlayLoopingSound("yobidashityuu.wav");
                callOutcomeTimer.Start();
                break;
            case PhoneState.InCall:
                syuwaButton.Background = brushSyuwaAka;
                if (isOutgoingCall) { hashinButton.Background = brushHashinAo; }
                else { jyuwaButton.Background = brushJyuwaAo; }
                NumberDisplay.Visibility = Visibility.Collapsed;
                CallTimerDisplay.Visibility = Visibility.Visible;
                inCallDuration = TimeSpan.Zero;
                CallTimerDisplay.Text = "00:00";
                inCallDurationTimer.Start();
                break;
            case PhoneState.Busy:
                NumberDisplay.Text = "話し中";
                NumberDisplay.FontSize = 24;
                PlaySfx("watyu.wav");
                break;
            case PhoneState.Ringing:
                NumberDisplay.Text = "着信中";
                isJyuwaFlashing = true;
                isSyuwaFlashing = true;
                flashingTimer.Start();
                PlayLoopingSound("beru.wav");
                break;
        }
    }

    private void FlashingTimer_Tick(object? sender, EventArgs e)
    {
        flashToggle = !flashToggle;
        if (isHashinFlashing) hashinButton.Background = flashToggle ? brushHashinAo : brushHashin;
        if (isSyuwaFlashing) syuwaButton.Background = flashToggle ? brushSyuwaAka : brushSyuwa;
        if (isJyuwaFlashing) jyuwaButton.Background = flashToggle ? brushJyuwaAo : brushJyuwa;
    }

    private void CallOutcomeTimer_Tick(object? sender, EventArgs e) { SetState(NumberDisplay.Text == "1004" ? PhoneState.Busy : PhoneState.InCall); }
    private void InCallDurationTimer_Tick(object? sender, EventArgs e) { inCallDuration = inCallDuration.Add(TimeSpan.FromSeconds(1)); CallTimerDisplay.Text = inCallDuration.ToString(@"mm\:ss"); }

    #endregion

    #region UI Event Handlers

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
        if (CallList.SelectedItem is CallListItem selectedItem) { NumberDisplay.Text = selectedItem.PhoneNumber; SetState(PhoneState.ReadyToDial); }
    }

    private void jyuwaButton_Click(object sender, RoutedEventArgs e)
    {
        if (currentState == PhoneState.Ringing)
        {
            PlaySfx("tori.wav");
            isOutgoingCall = false;
            SetState(PhoneState.InCall);
        }
    }

    private void syuwaButton_Click(object sender, RoutedEventArgs e)
    {
        if (currentState != PhoneState.Idle)
        {
            PlaySfx("oki.wav");
            SetState(PhoneState.Idle);
        }
    }

    private void hashinButton_Click(object sender, RoutedEventArgs e)
    {
        if (currentState == PhoneState.ReadyToDial && !string.IsNullOrEmpty(NumberDisplay.Text))
        {
            PlaySfx("tori.wav");
            isOutgoingCall = true;
            SetState(PhoneState.Dialing);
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

    private void ChangeWorkLocationMenuItem_Click(object sender, RoutedEventArgs e) { /* TODO */ }
    private void SimulateRinging_Click(object sender, RoutedEventArgs e) { SetState(PhoneState.Ringing); }

    #endregion
}
