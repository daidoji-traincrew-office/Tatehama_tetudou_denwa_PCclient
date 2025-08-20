using System;
using System.Windows.Threading;

namespace Tatehama_tetudou_denwa_PCclient.Services
{
    public enum PhoneState { Idle, ReadyToDial, Dialing, InCall, Busy, Ringing }

    public class StateManager
    {
        public PhoneState CurrentState { get; private set; } = PhoneState.Idle;
        public event Action<PhoneState>? StateChanged;
        private DispatcherTimer? inCallDurationTimer, flashingTimer, ringingTimer;
        private TimeSpan inCallDuration;

        public StateManager()
        {
            inCallDurationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            inCallDurationTimer.Tick += (s, e) => inCallDuration = inCallDuration.Add(TimeSpan.FromSeconds(1));
            flashingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            ringingTimer = new DispatcherTimer();
        }

        public void SetState(PhoneState state)
        {
            CurrentState = state;
            StateChanged?.Invoke(state);
            // 必要に応じてタイマーやUI制御
        }

        public void ResetTimers()
        {
            inCallDuration = TimeSpan.Zero;
            inCallDurationTimer?.Stop();
            flashingTimer?.Stop();
            ringingTimer?.Stop();
        }
    }
}
