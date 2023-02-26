namespace Loupedeck.VoiceMeeterPlugin.Library.Voicemeeter
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;

    /// <summary>
    /// Observable levels monitor.  Use Rx to subscribe to the levels of the selected Channels.
    /// Usage:
    ///  using System.Reactive.Linq;
    ///  var levels = new Voicemeeter.Levels(channels, 20);
    ///  var subscription = levels.Subscribe(x => DoSomethingWithFloatArray(x));
    ///  ...
    ///  subscription.Dispose(); levels.Dispose();
    /// </summary>
    public class Levels : IDisposable, IObservable<Single[]>
    {
        public class Channel
        {
            public LevelType LevelType { get; set; }
            public Int32 ChannelNumber { get; set; }
        };

        private readonly List<Channel> _channels;
        private readonly List<IObserver<Single[]>> _observers = new List<IObserver<Single[]>>();
        private readonly IObservable<Int32> _timer;
        private IDisposable _timerSubscription;

        public Levels(Channel[] channels, Int32 milliseconds = 20)
        {
            this._channels = new List<Channel>(channels);
            this._timer = Observable.Interval(TimeSpan.FromMilliseconds(milliseconds)).Select(_ => 1);
            this.Watch();
        }

        private void Watch() =>
            this._timerSubscription = this._timer.Subscribe(_ =>
            {
                var values = new List<Single>(this._channels.Count);
                foreach (var channel in this._channels)
                {
                    values.Add(Remote.GetLevel(channel.LevelType, channel.ChannelNumber));
                }

                this.Notify(values.ToArray());
            });

        public IDisposable Subscribe(IObserver<Single[]> observer)
        {
            if (!this._observers.Contains(observer))
            {
                this._observers.Add(observer);
            }

            return new Unsubscriber(this._observers, observer);
        }

        private void Notify(Single[] values)
        {
            foreach (var observer in this._observers)
            {
                observer.OnNext(values);
            }
        }

        public void Dispose() => this._timerSubscription?.Dispose();

        private sealed class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<Single[]>> _observers;
            private readonly IObserver<Single[]> _observer;

            public Unsubscriber(List<IObserver<Single[]>> observers, IObserver<Single[]> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (this._observer != null && this._observers.Contains(this._observer))
                {
                    this._observers.Remove(this._observer);
                }
            }
        }
    }
}