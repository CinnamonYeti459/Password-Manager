using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;

namespace Password_Manager.Models
{
    public class UserIdleDetector : IDisposable
    {
        private readonly InputElement _inputElement;
        private readonly DispatcherTimer _idleTimer;
        private readonly TimeSpan _idleTime;

        public event EventHandler? UserBecameIdle;

        public UserIdleDetector(InputElement inputElement, TimeSpan? idleTime = null)
        {
            _inputElement = inputElement ?? throw new ArgumentNullException(nameof(inputElement));
            _idleTime = idleTime ?? TimeSpan.FromSeconds(60);

            _idleTimer = new DispatcherTimer
            {
                Interval = _idleTime
            };
            _idleTimer.Tick += OnIdleTimerTick;

            // Subscribe to input events (Mouse moved, Mouse clicked, Keys pressed)
            _inputElement.AddHandler(InputElement.PointerMovedEvent, OnInputDetected, Avalonia.Interactivity.RoutingStrategies.Tunnel);
            _inputElement.AddHandler(InputElement.KeyDownEvent, OnInputDetected, Avalonia.Interactivity.RoutingStrategies.Tunnel);
            _inputElement.AddHandler(InputElement.PointerPressedEvent, OnInputDetected, Avalonia.Interactivity.RoutingStrategies.Tunnel);

            Start();
        }

        private void OnInputDetected(object? sender, RoutedEventArgs e)
        {
            ResetTimer();
        }

        private void OnIdleTimerTick(object? sender, EventArgs e)
        {
            Stop();
            UserBecameIdle?.Invoke(this, EventArgs.Empty);
        }

        public void Start()
        {
            ResetTimer();
            _idleTimer.Start();
        }

        public void Stop()
        {
            _idleTimer.Stop();
        }

        private void ResetTimer()
        {
            _idleTimer.Stop();
            _idleTimer.Start();
        }

        public void Dispose()
        {
            Stop();
            _idleTimer.Tick -= OnIdleTimerTick;

            _inputElement.RemoveHandler(InputElement.PointerMovedEvent, OnInputDetected);
            _inputElement.RemoveHandler(InputElement.KeyDownEvent, OnInputDetected);
            _inputElement.RemoveHandler(InputElement.PointerPressedEvent, OnInputDetected);
        }
    }
}