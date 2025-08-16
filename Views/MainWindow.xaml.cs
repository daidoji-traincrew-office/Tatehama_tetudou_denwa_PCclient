using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Media;

namespace Tatehama_tetudou_denwa_PCclient;

public class CallListItem
{
    public string DisplayName { get; set; }
    public string PhoneNumber { get; set; }

    public override string ToString()
    {
        return DisplayName;
    }
}

public partial class MainWindow : Window
{
    private enum PhoneState
    {
        Idle,
        OffHook,
        Dialing,
        InCall
    }

    private PhoneState currentState = PhoneState.Idle;
    private SoundPlayer pushSoundPlayer = new SoundPlayer("sound/push.wav");

    public MainWindow()
    {
        InitializeComponent();
        PopulateCallList();
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

    private void AppendNumber(string number)
    {
        if (NumberDisplay.Text.Length < 4)
        {
            NumberDisplay.Text += number;
            pushSoundPlayer.Play();
        }
    }

    private void syoukyoButton_Click(object sender, RoutedEventArgs e)
    {
        if (NumberDisplay.Text.Length > 0)
        {
            NumberDisplay.Text = NumberDisplay.Text.Substring(0, NumberDisplay.Text.Length - 1);
            pushSoundPlayer.Play();
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

    private void jyuwaButton_Click(object sender, RoutedEventArgs e)
    {
        if (currentState == PhoneState.Idle)
        {
            currentState = PhoneState.OffHook;
            jyuwaButton.Background = new ImageBrush(new BitmapImage(new Uri("/image/jyuwa-aka.png", UriKind.Relative))) { Stretch = Stretch.Fill };
            syuwaButton.Background = new ImageBrush(new BitmapImage(new Uri("/image/syuwa-ao.png", UriKind.Relative))) { Stretch = Stretch.Fill };
        }
    }

    private void syuwaButton_Click(object sender, RoutedEventArgs e)
    {
        currentState = PhoneState.Idle;
        NumberDisplay.Text = "";
        jyuwaButton.Background = new ImageBrush(new BitmapImage(new Uri("/image/jyuwa.png", UriKind.Relative))) { Stretch = Stretch.Fill };
        syuwaButton.Background = new ImageBrush(new BitmapImage(new Uri("/image/syuwa.png", UriKind.Relative))) { Stretch = Stretch.Fill };
        hashinButton.Background = new ImageBrush(new BitmapImage(new Uri("/image/hashin.png", UriKind.Relative))) { Stretch = Stretch.Fill };
    }

    private void hashinButton_Click(object sender, RoutedEventArgs e)
    {
        if (currentState == PhoneState.OffHook && !string.IsNullOrEmpty(NumberDisplay.Text))
        {
            currentState = PhoneState.Dialing;
            hashinButton.Background = new ImageBrush(new BitmapImage(new Uri("/image/hashin-aka.png", UriKind.Relative))) { Stretch = Stretch.Fill };
        }
    }

    private void CallList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CallList.SelectedItem is CallListItem selectedItem)
        {
            NumberDisplay.Text = selectedItem.PhoneNumber;
        }
    }

    private void ChangeWorkLocationMenuItem_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement logic
    }

    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement logic
    }
}