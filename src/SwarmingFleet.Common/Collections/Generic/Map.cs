
namespace SwarmingFleet.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    public class Map<TLeft, TRight> : IEnumerable<KeyValuePair<TLeft, TRight>>
    {
        private readonly object _lock = new object();
        private readonly Dictionary<TLeft, TRight> _forward = new Dictionary<TLeft, TRight>();
        private readonly Dictionary<TRight, TLeft> _reverse = new Dictionary<TRight, TLeft>();

        public Map()
        { 
        }


        public void Add((TLeft left, TRight right) item)
        {
            this.Add(item.left, item.right);
        }

        public bool Add(TLeft left, TRight right)
        {
            lock (this._lock)
                return this._forward.TryAdd(left, right)
                    && this._reverse.TryAdd(right, left);
        }

        public TLeft this[TRight key]
        {
            get
            {
                lock (this._lock) 
                    return this._reverse[key];
            }
            set  
            {
                lock (this._lock)
                    this._reverse[key] = value;
            }
        }
        public TRight this[TLeft key]
        {
            get
            {
                lock (this._lock)
                    return this._forward[key];
            }
            set
            {
                lock (this._lock)
                    this._forward[key] = value;
            }
        }

        public bool ContainsKey(TLeft key)
        {
            lock (this._lock)
                return this._forward.ContainsKey(key);
        }

        public bool TryGetValue(TLeft key, [MaybeNullWhen(false)] out TRight value)
        {
            lock (this._lock)
                return this._forward.TryGetValue(key, out value);
        }


        public int Count
        {
            get
            {
                lock (this._lock)
                    return this._forward.Count;
            }
        }

        public bool ContainsKey(TRight key)
        {
            lock (this._lock)
                return this._reverse.ContainsKey(key);
        }

        public bool TryGetValue(TRight key, [MaybeNullWhen(false)] out TLeft value)
        {
            lock (this._lock)
                return this._reverse.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<TLeft, TRight>> GetEnumerator()
        {
            lock (this._lock)
                return this._forward.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
