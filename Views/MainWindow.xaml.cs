using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Tatehama_tetudou_denwa_PCclient;

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

    public MainWindow()
    {
        InitializeComponent();
    }

    private void AppendNumber(string number)
    {
        if (currentState == PhoneState.OffHook)
        {
            NumberDisplay.Text += number;
        }
    }

    private void syoukyoButton_Click(object sender, RoutedEventArgs e)
    {
        if (currentState == PhoneState.OffHook && NumberDisplay.Text.Length > 0)
        {
            NumberDisplay.Text = NumberDisplay.Text.Substring(0, NumberDisplay.Text.Length - 1);
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

    private void ChangeWorkLocationMenuItem_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement logic
    }

    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement logic
    }
}
