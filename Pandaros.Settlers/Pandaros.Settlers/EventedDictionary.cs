using Pandaros.Settlers.Extender;
using Pipliz.JSON;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Pandaros.Settlers
{

    public enum DictionaryEventType
    {
        AddItem,
        RemoveItem,
        ChangeItem
    }

    public class DictionaryChangedEventArgs<K, V> : EventArgs
    {
        public DictionaryChangedEventArgs(DictionaryEventType changeType, K key, V value, V oldValue)
        {
            EventType = changeType;
            Key = key;
            Value = value;
            ReplacedValue = oldValue;
        }

        public DictionaryEventType EventType { get; private set; }

        public K Key { get; private set; }

        public V Value { get; private set; }

        public V ReplacedValue { get; private set; }
    }

    public class EventedDictionary<K, V> : IDictionary<K, V>
    {

        public delegate void DictionaryChanged(object sender, DictionaryChangedEventArgs<K, V> e);

        public event DictionaryChanged OnDictionaryChanged;

        private readonly IDictionary<K, V> _innerDict;

        public EventedDictionary()
        {
            _innerDict = new Dictionary<K, V>();
        }

        private void TriggerEvent(DictionaryChangedEventArgs<K, V> dictChanged)
        {
            if (OnDictionaryChanged != null && 
                OnDictionaryChanged.GetInvocationList().Length != 0)
                OnDictionaryChanged(this, dictChanged);
        }

        public V this[K key]
        {
            get
            {
                Add(key, default(V));
                return _innerDict[key];
            }
            set
            {
                if (!CanAdd(key, value))
                {
                    var oldVal = _innerDict[key];
                    _innerDict[key] = value;
                    TriggerEvent(new DictionaryChangedEventArgs<K, V>(DictionaryEventType.ChangeItem, key, value, oldVal));
                }
            }
        }

        public ICollection<K> Keys => _innerDict.Keys;

        public ICollection<V> Values => _innerDict.Values;

        public int Count => _innerDict.Count;

        public bool IsReadOnly => _innerDict.IsReadOnly;

        public void Add(K key, V value)
        {
            CanAdd(key, value);
        }

        private bool CanAdd(K key, V value)
        {
            bool added = false;

            if (!ContainsKey(key))
            {
                _innerDict.Add(key, value);
                TriggerEvent(new DictionaryChangedEventArgs<K, V>(DictionaryEventType.AddItem, key, value, default(V)));
                added = true;
            }

            return added;
        }

        public void Add(KeyValuePair<K, V> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            var copy = new Dictionary<K, V>(_innerDict);
            _innerDict.Clear();

            foreach (var item in copy)
                TriggerEvent(new DictionaryChangedEventArgs<K, V>(DictionaryEventType.RemoveItem, item.Key, item.Value, default(V)));
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return _innerDict.Contains(item);
        }

        public bool ContainsKey(K key)
        {
            return _innerDict.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            _innerDict.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
           return _innerDict.GetEnumerator();
        }

        public bool Remove(K key)
        {
            bool removed = false;

            if (ContainsKey(key))
            {
                var val = _innerDict[key];
                removed = _innerDict.Remove(key);

                if (removed)
                    TriggerEvent(new DictionaryChangedEventArgs<K, V>(DictionaryEventType.RemoveItem, key, val, default(V)));
            }

            return removed;
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            bool removed = false;

            if (Contains(item))
            {
                var val = _innerDict[item.Key];
                removed = _innerDict.Remove(item);

                if (removed)
                    TriggerEvent(new DictionaryChangedEventArgs<K, V>(DictionaryEventType.RemoveItem, item.Key, val, default(V)));
            }

            return removed;
        }

        public bool TryGetValue(K key, out V value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
