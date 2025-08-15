using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Media;
using System;

namespace Tatehama_tetudou_denwa_PCclient
{
    public partial class MainWindow : Window
    {
        private readonly SoundPlayer _pushSound = new SoundPlayer(@"sound\push.wav");
        private readonly SoundPlayer _toriSound = new SoundPlayer(@"sound\tori.wav");
        private readonly SoundPlayer _okiSound = new SoundPlayer(@"sound\oki.wav");
        private readonly SoundPlayer _beruSound = new SoundPlayer(@"sound\beru.wav");
        private readonly SoundPlayer _denshiberuSound = new SoundPlayer(@"sound\denshiberu.wav");
        private readonly SoundPlayer _aitenashi1Sound = new SoundPlayer(@"sound\aitenashi_1.wav");
        private readonly SoundPlayer _aitenashi2Sound = new SoundPlayer(@"sound\aitenashi_2.wav");
        private readonly SoundPlayer _watyuSound = new SoundPlayer(@"sound\watyu.wav");
        private readonly SoundPlayer _yobidashityuuSound = new SoundPlayer(@"sound\yobidashityuu.wav");

        private readonly Random _random = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void PlaySound(SoundPlayer player)
        {
            try
            {
                player.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"サウンド再生エラー: {ex.Message}");
            }
        }

        private void Keypad_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PlaySound(_pushSound);
            if (sender is Image image && image.Tag != null)
            {
                NumberDisplay.Text += image.Tag.ToString();
            }
        }

        private void Syoukyo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PlaySound(_pushSound);
            if (!string.IsNullOrEmpty(NumberDisplay.Text))
            {
                NumberDisplay.Text = NumberDisplay.Text.Substring(0, NumberDisplay.Text.Length - 1);
            }
        }

        private void CallList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CallList.SelectedItem is ListViewItem selectedItem)
            {
                int index = CallList.Items.IndexOf(selectedItem);
                if (index >= 0)
                {
                    if (index < 4)
                    {
                        NumberDisplay.Text = (1000 + index).ToString();
                    }
                    else
                    {
                        NumberDisplay.Text = (2000 + (index - 4)).ToString();
                    }
                }
            }
        }

        private void Jyuwa_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PlaySound(_toriSound);
            StatusDisplay.Text = "受話器上げ";
        }

        private void Syuwa_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PlaySound(_okiSound);
            NumberDisplay.Text = "";
            StatusDisplay.Text = "待機中";
            _yobidashityuuSound.Stop();
            _beruSound.Stop();
            _denshiberuSound.Stop();
        }

        private void Hashin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StatusDisplay.Text = "呼び出し中...";
            PlaySound(_yobidashityuuSound);
        }

        private void ChangeWorkplace_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("勤務地変更がクリックされました。");
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("設定がクリックされました。");
        }
    }
}