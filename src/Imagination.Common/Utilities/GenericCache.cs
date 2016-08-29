/***********************************************************************************************************************
 Copyright (c) 2016, Imagination Technologies Limited and/or its affiliated group companies.
 All rights reserved.

 Redistribution and use in source and binary forms, with or without modification, are permitted provided that the
 following conditions are met:
     1. Redistributions of source code must retain the above copyright notice, this list of conditions and the
        following disclaimer.
     2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the
        following disclaimer in the documentation and/or other materials provided with the distribution.
     3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote
        products derived from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
 USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
***********************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using System.Diagnostics;

using Imagination.Model;

namespace Imagination
{
    /// <summary>
    /// Base class from Caching
    /// Has a capacity and when this is reached the oldest item is replaced
    /// </summary>
    /// <typeparam name="TValue">Type that you want to cache</typeparam>
	/// 
    public class GenericCache<TKey, TValue>
    {
		public delegate void RemovedItemEventHandler(TKey key, TValue value);
		public event RemovedItemEventHandler RemovedItem;

        protected int _DefaultLockTimeOut = 100;
        protected int _Count;
        protected int _Capacity;
		//protected ReaderWriterLockSlim _Lock = new ReaderWriterLockSlim();
		protected ReaderWriterSpinLock _Lock = new ReaderWriterSpinLock();
		protected Dictionary<TKey, TValue> _Cache;
		private TKey[] _KeyQueue;
		private int _KeyQueueHeadIndex;
		private int _KeyQueueTailIndex;
		private string _Name;

		private class RemovedEntry
		{
			public TKey Key { get; set; }
			public TValue Value { get; set; }
		}

		public GenericCache()
			: this(1000)
        {
        }

		public GenericCache(int capacity)
        {
            _Count = 0;
            _Capacity = capacity;
			_Cache = new Dictionary<TKey, TValue>(_Capacity);
			_KeyQueue = new TKey[_Capacity];
			_KeyQueueHeadIndex = 0;
			_KeyQueueTailIndex = _Capacity-1;
			_Name = typeof(TValue).Name;
		}

        public void Clear()
        {
			if (_Lock.TryEnterWriteLock(_DefaultLockTimeOut))
			{
				try
				{
					_Cache.Clear();
					_Count = 0;
				}
				finally
				{
					_Lock.ExitWriteLock();
				}
			}
			else
			{
				ApplicationEventLog.WriteEntry("Flow", string.Format("GenericCache:Clear - Failed to acquire lock."), EventLogEntryType.Error);
			}
        }

        /// <summary>
        /// Add item to cache
        /// </summary>
        /// <param name="key">Unique key of finding item</param>
        /// <param name="item">Item to add</param>
        /// <remarks>If already in cache it gets updated</remarks>
		public void Add(TKey key, TValue item)
		{
			if (_Lock.TryEnterWriteLock(_DefaultLockTimeOut))
			{
				RemovedEntry removedItem = null;
				try
				{
					TValue existingItem;
					if (_Cache.TryGetValue(key, out existingItem))
					{
						_Cache[key] = item;
					}
					else if (key != null)
					{
						if ((_Count + 1) > _Capacity)
						{
							removedItem = RemoveOldestRequestedItem();
						}
						else
							_Count++;
						_Cache.Add(key,  item);
						_KeyQueueTailIndex = (_KeyQueueTailIndex + 1) % _Capacity;
						_KeyQueue[_KeyQueueTailIndex] = key;
					}
				}
				finally
				{
					_Lock.ExitWriteLock();
				}
				if ((removedItem != null) && (RemovedItem != null))
					RemovedItem(removedItem.Key, removedItem.Value);

			}
			else
			{
				ApplicationEventLog.WriteEntry("Flow", string.Format("GenericCache:Add - Failed to acquire lock for key={0} value={1}", key, item), EventLogEntryType.Error);
			}
		}

        /// <summary>
        /// Removes item from cache
        /// </summary>
        /// <param name="key">Unique key of finding item.</param>
        public void Remove(TKey key)
        {
			if (_Lock.TryEnterWriteLock(_DefaultLockTimeOut))
			{
				RemovedEntry removedItem = null;
				try
				{
					TValue value;
					if (_Cache.TryGetValue(key, out value))
					{
						bool removed = _Cache.Remove(key);
#if DEBUG
						//ApplicationEventLog.WriteEntry("Flow", string.Format("GenericCache: removing item {0} of type {1} = removed={2}", key, typeof(TValue).Name, removed), EventLogEntryType.Information);
#endif
						removedItem = new RemovedEntry() { Key = key, Value = value };
					}
				}
				finally
				{
					_Lock.ExitWriteLock();
				}
				if ((removedItem != null) && (RemovedItem != null))
					RemovedItem(removedItem.Key, removedItem.Value);
			}
			else
			{
				ApplicationEventLog.WriteEntry("Flow", string.Format("GenericCache:Remove - Failed to acquire lock for key={0} ", key), EventLogEntryType.Error);
			}
        }

		private RemovedEntry RemoveOldestRequestedItem()
		{
			RemovedEntry result = null;
			TValue value;
			TKey key = _KeyQueue[_KeyQueueHeadIndex];
			_KeyQueueHeadIndex = (_KeyQueueHeadIndex + 1) % _Capacity;
			if (_Cache.TryGetValue(key, out value))
			{
#if DEBUG
				//ApplicationEventLog.WriteEntry("Flow", string.Format("GenericCache:Removing old item {0} of type {1}", key, typeof(TValue).Name), EventLogEntryType.Information);
#endif
				_Cache.Remove(key);
				result = new RemovedEntry() { Key = key, Value = value };
			}
			return result;
		}


        /// <summary>
        /// Try to retrieve an item from the cache.
        /// </summary>
        /// <param name="key">Unique key of finding item</param>
        /// <param name="item">The returned item.</param>
        /// <returns>True if an item with the specified key was found.</returns>
        public bool TryGetItem(TKey key, out TValue item)
        {
            bool result = false;
            item = default(TValue);
			if (_Lock.TryEnterReadLock(_DefaultLockTimeOut))
			{
				try
				{
					result = _Cache.TryGetValue(key, out item);
				}
				finally
				{
					_Lock.ExitReadLock();
				}

			}
			else
			{
				ApplicationEventLog.WriteEntry("Flow", string.Format("GenericCache:TryGetItem - Failed to acquire lock for key={0}", key), EventLogEntryType.Error);
			}
            return result;
        }


        /// <summary>
        /// Get/Add item in cache
        /// </summary>
        /// <param name="key">Unique key of finding item</param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
				TValue result;
				TryGetItem(key, out result);
				return result;
            }
            set 
            {
                Add(key,value);
            }
        }
       

    }
}
