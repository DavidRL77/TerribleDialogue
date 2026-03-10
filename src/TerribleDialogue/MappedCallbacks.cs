using System;
using System.Collections.Generic;
using System.Text;
using static TerribleDialogue.DialogueManager;

namespace TerribleDialogue
{
    /// <summary>
    /// Dictionary of a list of callbacks mapped by <typeparamref name="K"/>
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    internal class MappedCallbacks<K, V>
    {
        private readonly Dictionary<K, List<V>> callbacks = new Dictionary<K, List<V>>();

        public void AddCallback(K key, V value)
        {
            if(!callbacks.TryGetValue(key, out List<V> list))
            {
                list = new List<V>();
                callbacks[key] = list;
            }

            list.Add(value);
        }

        public void RemoveCallback(K key, V value)
        {
            if(callbacks.TryGetValue(key, out List<V> list))
            {
                if(list.Remove(value) && list.Count == 0)
                {
                    callbacks.Remove(key);
                }
            }
        }

        public void Invoke(K key, Action<V> action)
        {
            if(callbacks.TryGetValue(key, out List<V> list))
            {
                for(int i = 0; i < list.Count; i++)
                {
                    action.Invoke(list[i]);
                }
            }
        }
    }
}
