using Svelto.DataStructures;

namespace Svelto.DataStructures
{
    /// <summary>
    /// Represents an unordered sparse set of natural numbers, and provides constant-time operations on it.
    /// </summary>
    public sealed class SparseSet
    {
        public readonly FasterList<int> dense; //Dense set of elements
        public readonly FasterList<int> sparse; //Map of elements to dense set indices

        int  size_     = 0; //Current size (number of elements)
        uint capacity_ = 0; //Current size (number of elements)
        
        public SparseSet()
        {
            this.sparse = new FasterList<int>(1);
            this.dense  = new FasterList<int>(1);
        }

        void clear() { size_ = 0; }

        void reserve(uint u)
        {
            if (u > capacity_)
            {
                dense.ExpandTo(u);
                sparse.ExpandTo(u);
                capacity_ = u;
            }
        }

        public bool has(int val)
        {
            return val < capacity_ && sparse[val] < size_ && dense[sparse[val]] == val;
        }

        public void insert(int val)
        {
            if (!has(val))
            {
                if (val >= capacity_)
                    reserve((uint) (val + 1));

                dense[size_] = val;
                sparse[val]  = size_;
                ++size_;
            }
        }

        public void erase(int val)
        {
            if (has(val))
            {
                dense[sparse[val]]       = dense[size_ - 1];
                sparse[dense[size_ - 1]] = sparse[val];
                --size_;
            }
        }
    };
}