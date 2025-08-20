using System;
using Tatehama_tetudou_denwa_PCclient.Services;
using Tatehama_tetudou_denwa_PCclient.Models;

namespace Tatehama_tetudou_denwa_PCclient.ViewModels
{
    public class MainViewModel
    {
        public AudioManager Audio { get; } = new AudioManager();
        public StateManager State { get; } = new StateManager();
        public NotifyManager Notify { get; } = new NotifyManager();
        public ServerConnection Server { get; } = new ServerConnection();
        // 必要に応じてプロパティやコマンドを追加
    }
}
