using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Tatehama_tetudou_denwa_PCclient.Services
{
    public class NotifyManager
    {
        private NotifyIcon? notifyIcon;

        public void InitializeNotifyIcon(string iconPath)
        {
            try
            {
                using (var bitmap = new Bitmap(iconPath))
                {
                    IntPtr hIcon = bitmap.GetHicon();
                    notifyIcon = new NotifyIcon
                    {
                        Icon = Icon.FromHandle(hIcon),
                        Visible = true,
                        Text = "館浜鉄道電話"
                    };
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"通知アイコンの初期化に失敗しました。");
            }
        }

        public void ShowNotification(string title, string message)
        {
            notifyIcon?.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
        }

        public void Dispose()
        {
            notifyIcon?.Dispose();
        }
    }
}
