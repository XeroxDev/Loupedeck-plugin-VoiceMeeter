namespace Loupedeck.VoiceMeeterPlugin.Library.Voicemeeter
{
    using System;
    using System.Collections.Generic;

    public sealed class VoicemeeterClient : IDisposable, IObservable<Single>
    {
        public void Dispose()
        {
            try
            {
                RemoteWrapper.Logout();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private readonly List<IObserver<Single>> _observers = new();

        public IDisposable Subscribe(IObserver<Single> observer)
        {
            if (!this._observers.Contains(observer))
            {
                this._observers.Add(observer);
            }

            return new Unsubscriber(this._observers, observer);
        }

        private void Notify(Single value)
        {
            foreach (var observer in this._observers)
            {
                observer.OnNext(value);
            }
        }

        private sealed class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<Single>> _observers;
            private readonly IObserver<Single> _observer;

            public Unsubscriber(List<IObserver<Single>> observers, IObserver<Single> observer)
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