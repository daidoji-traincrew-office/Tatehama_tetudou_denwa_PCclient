using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tatehama_tetudou_denwa_PCclient.Services
{
    public class AudioManager
    {
        private Dictionary<string, WaveFileReader> soundCache = new();
        private WaveOutEvent? loopingDevice;
        private CancellationTokenSource? aitenashiCancelToken;
        public int OutputDeviceNumber { get; set; } = 0;

        public AudioManager()
        {
            // サウンドファイルのキャッシュ初期化
            // ...必要に応じて初期化処理...
        }

        public async Task PlaySfxAsync(string key)
        {
            if (WaveOut.DeviceCount == 0 || !soundCache.ContainsKey(key)) return;
            var sound = soundCache[key];
            sound.Position = 0;
            var tcs = new TaskCompletionSource<bool>();
            var tempDevice = new WaveOutEvent { DesiredLatency = 100, DeviceNumber = OutputDeviceNumber };
            tempDevice.Init(sound);
            tempDevice.PlaybackStopped += (s, e) => {
                tempDevice.Dispose();
                tcs.SetResult(true);
            };
            tempDevice.Play();
            await tcs.Task;
        }

        public void StopAllSounds()
        {
            loopingDevice?.Stop();
            loopingDevice?.Dispose();
            loopingDevice = null;
            aitenashiCancelToken?.Cancel();
        }

        public async Task PlayAitenashiSequenceAsync(string? partnerNumber, string? displayText)
        {
            StopAllSounds();
            aitenashiCancelToken?.Cancel();
            aitenashiCancelToken = new CancellationTokenSource();
            var token = aitenashiCancelToken.Token;
            if (!string.IsNullOrEmpty(partnerNumber) || !string.IsNullOrEmpty(displayText))
            {
                await PlaySfxAsync("tori.wav");
                var rand = new Random();
                string aitenashi = rand.NextDouble() < 0.9 ? "aitenashi_1.wav" : "aitenashi_2.wav";
                await PlaySfxAsync(aitenashi);
                // ...UI更新はMainWindow側で...
                if (!token.IsCancellationRequested)
                    await PlaySfxAsync("kire.wav");
                if (!token.IsCancellationRequested)
                    await Task.Delay(300);
                if (!token.IsCancellationRequested)
                    await PlaySfxAsync("kire2.wav");
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(350);
                    if (token.IsCancellationRequested) break;
                    await PlaySfxAsync("kire2.wav");
                }
            }
        }
    }
}
