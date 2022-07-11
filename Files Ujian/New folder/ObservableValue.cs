using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class ObservableValue<T> : IObservable<T>
    {
        public T Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                CallObservers();
            }
        }
        private T value;
        private List<System.Action<T>> observers = new List<System.Action<T>>();
        public ObservableValue(T value)
        {
            this.value = value;
        }
        public void CallObservers()
        {
            foreach (var observer in observers)
            {
                observer.Invoke(value);
            }
        }

        public void AddObserver(System.Action<T> observer)
        {
            observers.Add(observer);
        }

        public void RemoveObserver(System.Action<T> observer)
        {
            observers.Remove(observer);
        }
    }
}
