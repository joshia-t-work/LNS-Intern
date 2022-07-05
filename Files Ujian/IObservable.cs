using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    interface IObservable<T>
    {
        public void AddObserver(System.Action<T> observer);
        public void RemoveObserver(System.Action<T> observer);
    }
}
