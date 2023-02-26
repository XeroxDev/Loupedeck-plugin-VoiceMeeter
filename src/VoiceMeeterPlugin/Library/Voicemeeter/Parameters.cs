namespace Loupedeck.VoiceMeeterPlugin.Library.Voicemeeter
{
    using System;
    using System.Collections.Generic;
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
        private readonly List<IObserver<Int32>> _observers = new List<IObserver<Int32>>();
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

        private sealed class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<Int32>> _observers;
            private readonly IObserver<Int32> _observer;

            public Unsubscriber(List<IObserver<Int32>> observers, IObserver<Int32> observer)
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