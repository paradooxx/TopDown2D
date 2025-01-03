using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Utils
{
    [System.Serializable]
    public class SerializableQueue
    {
        [SerializeField] private List<int> queueList = new List<int>();
        private Queue<int> queue = new Queue<int>();

        public Queue<int> Queue
        {
            get
            {
                // if queue is empty, populate it from the list
                if (queue.Count == 0 && queueList.Count > 0)
                {
                    queue = new Queue<int>(queueList);
                }
                return queue;
            }
        }

        public void Enqueue(int value)
        {
            queue.Enqueue(value);
            queueList = new List<int>(queue); // update the inspector view
        }

        public int Dequeue()
        {
            int value = queue.Dequeue();
            queueList = new List<int>(queue); // update the inspector view
            return value;
        }

        public void Clear()
        {
            queue.Clear();
            queueList.Clear();
        }

        public int Count => queue.Count;
    }
}