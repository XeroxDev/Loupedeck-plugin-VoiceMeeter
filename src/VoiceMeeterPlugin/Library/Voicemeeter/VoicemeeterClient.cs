namespace Loupedeck.VoiceMeeterPlugin.Library.Voicemeeter
{
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

        private readonly List<IObserver<Single>> _observers = [];

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

        private sealed class Unsubscriber(List<IObserver<Single>> observers, IObserver<Single> observer) : IDisposable
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