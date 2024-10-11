namespace Loupedeck.VoiceMeeterPlugin.Library.Voicemeeter
{
    using System.Reactive.Linq;

    /// <summary>
    /// Observable parameters monitor.  Use Rx to subscribe to parameter changes.
    /// Usage:
    ///  using System.Reactive.Linq;
    ///  var parameters = new Voicemeeter.Parameters();
    ///  var subscription = parameters.Parameters(x => DoSomethingTheParametersChanged(x));
    ///  ...
    ///  subscription.Dispose(); levels.Dispose();
    /// </summary>
    public class Parameters : IDisposable, IObservable<Int32>
    {
        private readonly List<IObserver<Int32>> _observers = [];
        private readonly IObservable<Int32> _timer;
        private IDisposable _timerSubscription;

        public Parameters(Int32 milliseconds = 20)
        {
            this._timer = Observable.Interval(TimeSpan.FromMilliseconds(milliseconds)).Select(_ => 1);
            this.Watch();
        }

        private void Watch() =>
            this._timerSubscription = this._timer.Subscribe(_ =>
            {
                var response = Remote.IsParametersDirty();
                if (response > 0)
                {
                    this.Notify(response);
                }
            });

        public IDisposable Subscribe(IObserver<Int32> observer)
        {
            if (!this._observers.Contains(observer))
            {
                lock (this._observers)
                {
                    this._observers.Add(observer);
                }
            }

            return new Unsubscriber(this._observers, observer);
        }

        private void Notify(Int32 value)
        {
            lock (this._observers)
            {
                foreach (var observer in this._observers)
                {
                    observer.OnNext(value);
                }
            }
        }

        public void Dispose() => this._timerSubscription?.Dispose();

        private sealed class Unsubscriber(List<IObserver<Int32>> observers, IObserver<Int32> observer) : IDisposable
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