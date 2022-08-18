using System.Collections.Generic;
using UnityEngine;

namespace TamborGame.Utilities 
{
    public class PriorityNode<T>
    {
        public T value;
        public float priority;

        public PriorityNode()
        {
            value = default;
            priority = 0f;
        }

        public PriorityNode(T v, float p)
        {
            value = v;
            priority = p;
        }
    }

    public class PriorityQueue<T>
    {
        List<PriorityNode<T>> list;

        public PriorityQueue()
        {
            list = new List<PriorityNode<T>>();
        }

        public void Insert(PriorityNode<T> node)
        {
            list.Add(node);
        }

        public PriorityNode<T> Pull()
        {
            PriorityNode<T> lowest = new PriorityNode<T>();
            lowest.priority = Mathf.Infinity;

            foreach (PriorityNode<T> node in list)
            {
                if (lowest.priority > node.priority)
                    lowest = node;
            }
            list.Remove(lowest);
            return lowest;
        }

        public int Count()
        {
            return list.Count;
        }
    }
}