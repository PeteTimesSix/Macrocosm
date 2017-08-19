using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Macrocosm.patches;

namespace Macrocosm.extensions
{
    public class KeyedPriorityQueue<T>
    {
        protected List<KeyValuePair<float, T>> innerList = new List<KeyValuePair<float, T>>();

        //protected IComparer<T> comparer;

        public int Count
        {
            get
            {
                return this.innerList.Count;
            }
        }

        public IEnumerable<T> Values {
            get
            {
                List<T> values = new List<T>();
                foreach(KeyValuePair<float, T> kvPair in innerList)
                {
                    yield return kvPair.Value;
                }
            }
        }

        public KeyedPriorityQueue()
        {
            //this.comparer = Comparer<T>.Default;
        }

        /*
        public KeyedPriorityQueue(IComparer<T> comparer)
        {
            this.comparer = comparer;
        }*/

        public void Push(T item, float priority)
        {
            int num = this.innerList.Count;
            this.innerList.Add(new KeyValuePair<float, T>(priority, item));
            while (num != 0)
            {
                int num2 = (num - 1) / 2;
                if (this.CompareElements(num, num2) >= 0)
                {
                    return;
                }
                this.SwapElements(num, num2);
                num = num2;
            }
        }

        public T Pop()
        {
            float prio;
            return Pop(out prio);
        }

        public T Pop(out float priority)
        {
            KeyValuePair<float, T> resultPair = this.innerList[0];
            int num = 0;
            this.innerList[0] = this.innerList[this.innerList.Count - 1];
            this.innerList.RemoveAt(this.innerList.Count - 1);
            while (true)
            {
                int num2 = num;
                int num3 = 2 * num + 1;
                int num4 = 2 * num + 2;
                if (this.innerList.Count > num3 && this.CompareElements(num, num3) > 0)
                {
                    num = num3;
                }
                if (this.innerList.Count > num4 && this.CompareElements(num, num4) > 0)
                {
                    num = num4;
                }
                if (num == num2)
                {
                    break;
                }
                this.SwapElements(num, num2);
            }
            priority = resultPair.Key;
            return resultPair.Value;
        }

        public T Peek(out float priority)
        {
            priority = this.innerList[0].Key;
            return this.innerList[0].Value;
        }

        public bool Contains(T value)
        {
            foreach(KeyValuePair<float, T> kvPair in innerList)
            {
                if (kvPair.Value.Equals(value))
                    return true;
            }
            return false;
        }

        public void Clear()
        {
            this.innerList.Clear();
        }

        protected void SwapElements(int i, int j)
        {
            KeyValuePair<float, T> temp = this.innerList[i];
            this.innerList[i] = this.innerList[j];
            this.innerList[j] = temp;
        }

        protected float CompareElements(int i, int j)
        {
            return this.innerList[i].Key - this.innerList[j].Key;
        }

        internal bool ContainsLimited(T item, float priority)
        {
            if (innerList.Count == 0)
                return false;

            int index = 0;
            KeyValuePair<float, T> kvPair = innerList.FirstOrDefault();
            while(kvPair.Key <= priority)
            {
                if (item.Equals(kvPair.Value))
                    return true;

                index++;
                if (index >= innerList.Count)
                    break;
                else
                    kvPair = innerList[index];
            }
            return false;
        }

        internal bool ContainsLimited(T item, float priority, out T duplicate)
        {
            if (innerList.Count == 0)
            {
                duplicate = default(T);
                return false;
            }

            int index = 0;
            KeyValuePair<float, T> kvPair = innerList.FirstOrDefault();
            while (kvPair.Key <= priority)
            {
                if (item.Equals(kvPair.Value))
                {
                    duplicate = kvPair.Value;
                    return true;
                }

                index++;
                if (index >= innerList.Count)
                    break;
                else
                    kvPair = innerList[index];
            }

            duplicate = default(T);
            return false;
        }
    }
}
