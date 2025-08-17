using System.Windows;
using NAudio.Wave;
using System.Linq;
using Tatehama_tetudou_denwa_PCclient.Models;

namespace Tatehama_tetudou_denwa_PCclient.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadAudioDevices();
            LoadCurrentSettings();
        }

        private void LoadAudioDevices()
        {
            // Input Devices (Microphones)
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var caps = WaveIn.GetCapabilities(i);
                InputDeviceComboBox.Items.Add(new { DeviceNumber = i, ProductName = caps.ProductName });
            }
            if (InputDeviceComboBox.Items.Count > 0)
            {
                InputDeviceComboBox.SelectedIndex = 0;
            }

            // Output Devices (Speakers)
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var caps = WaveOut.GetCapabilities(i);
                OutputDeviceComboBox.Items.Add(new { DeviceNumber = i, ProductName = caps.ProductName });
            }
            if (OutputDeviceComboBox.Items.Count > 0)
            {
                OutputDeviceComboBox.SelectedIndex = 0;
            }
        }

        private void LoadCurrentSettings()
        {
            // Select previously saved input device
            var selectedInput = InputDeviceComboBox.Items.Cast<dynamic>().FirstOrDefault(item => item.DeviceNumber == AudioSettings.InputDeviceNumber);
            if (selectedInput != null)
            {
                InputDeviceComboBox.SelectedItem = selectedInput;
            }

            // Select previously saved output device
            var selectedOutput = OutputDeviceComboBox.Items.Cast<dynamic>().FirstOrDefault(item => item.DeviceNumber == AudioSettings.OutputDeviceNumber);
            if (selectedOutput != null)
            {
                OutputDeviceComboBox.SelectedItem = selectedOutput;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (InputDeviceComboBox.SelectedItem != null)
            {
                AudioSettings.InputDeviceNumber = InputDeviceComboBox.SelectedValue != null ? (int)InputDeviceComboBox.SelectedValue : 0;
            }
            if (OutputDeviceComboBox.SelectedItem != null)
            {
                AudioSettings.OutputDeviceNumber = OutputDeviceComboBox.SelectedValue != null ? (int)OutputDeviceComboBox.SelectedValue : 0;
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
