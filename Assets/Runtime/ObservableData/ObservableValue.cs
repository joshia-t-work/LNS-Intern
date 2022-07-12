using System;
using System.Collections.Generic;

namespace LNS.Observable
{
    public class ObservableValue<T> : IObservable<T>
    {
        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                CallObservers();
            }
        }
        private T _value;
        private List<Action<T>> _observers = new List<Action<T>>();
        public ObservableValue(T value)
        {
            _value = value;
        }
        public void CallObservers()
        {
            foreach (var observer in _observers)
            {
                observer.Invoke(_value);
            }
        }

        public void AddObserver(Action<T> observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(Action<T> observer)
        {
            _observers.Remove(observer);
        }
    }
}
