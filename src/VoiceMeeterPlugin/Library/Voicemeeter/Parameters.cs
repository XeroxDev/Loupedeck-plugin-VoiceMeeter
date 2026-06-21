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
        private Boolean _disposed;

        public Parameters(Int32 milliseconds = 50)
        {
            this._timer = Observable.Interval(TimeSpan.FromMilliseconds(milliseconds)).Select(_ => 1);
            this.Watch();
        }

        private void Watch() =>
            this._timerSubscription = this._timer.Subscribe(_ =>
            {
                if (this._disposed)
                {
                    return;
                }

                var response = Remote.IsParametersDirty();
                if (response != 0)
                {
                    this.Notify(response);
                }
            });

        public IDisposable Subscribe(IObserver<Int32> observer)
        {
            lock (this._observers)
            {
                if (!this._observers.Contains(observer))
                {
                    this._observers.Add(observer);
                }
            }

            return new Unsubscriber(this._observers, observer);
        }

        private void Notify(Int32 value)
        {
            IObserver<Int32>[] observers;
            lock (this._observers)
            {
                observers = this._observers.ToArray();
            }

            foreach (var observer in observers)
            {
                observer.OnNext(value);
            }
        }

        public void Dispose()
        {
            this._disposed = true;
            this._timerSubscription?.Dispose();
        }

        private sealed class Unsubscriber(List<IObserver<Int32>> observers, IObserver<Int32> observer) : IDisposable
        {
            public void Dispose()
            {
                lock (observers)
                {
                    if (observer != null && observers.Contains(observer))
                    {
                        observers.Remove(observer);
                    }
                }
            }
        }
    }
}
