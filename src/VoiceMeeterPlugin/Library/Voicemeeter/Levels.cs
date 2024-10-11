namespace Loupedeck.VoiceMeeterPlugin.Library.Voicemeeter
{
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
        private readonly List<IObserver<Single[]>> _observers = [];
        private readonly IObservable<Int32> _timer;
        private IDisposable _timerSubscription;

        public Levels(Int32 milliseconds = 20)
        {
            this._channels = [];
            this._timer = Observable.Interval(TimeSpan.FromMilliseconds(milliseconds)).Select(_ => 1);
            this.Watch();
        }
        
        public void AddChannel(Channel channel)
        {
            // first check if there's already a channel with the same LevelType and ChannelNumber
            if (this._channels.Any(c => c.LevelType == channel.LevelType && c.ChannelNumber == channel.ChannelNumber))
            {
                return;
            }
            
            this._channels.Add(channel);
        }

        private void Watch() =>
            this._timerSubscription = this._timer.Subscribe(_ =>
            {
                if (this._channels.Count == 0)
                {
                    return;
                }
                var values = new List<Single>(this._channels.Count);
                values.AddRange(this._channels.Select(channel => Remote.GetLevel(channel.LevelType, channel.ChannelNumber)));

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

        private sealed class Unsubscriber(List<IObserver<Single[]>> observers, IObserver<Single[]> observer) : IDisposable
        {
            public void Dispose()
            {
                if (observer != null && observers.Contains(observer))
                {
                    observers.Remove(observer);
                }
            }
        }
    }
}