using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Scarab.Util
{
    internal class SortableObservableCollection<T> : ObservableCollection<T>
    {
        public SortableObservableCollection(IEnumerable<T> iter) : base(iter) {}
        
        public void SortBy(Func<T, T, int> comparer)
        {
            if (Items is not List<T> items)
                throw new InvalidOperationException("The backing field type is not List<T> on Collection<T>.");

            items.Sort((x, y) => comparer(x, y));

            typeof(ObservableCollection<T>).GetMethod("OnCollectionReset", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(this, new object[0]);
        }
    }
}
