namespace Svelto.DataStructures
{
    public struct SveltoDictionaryNode<TKey>
    {
        public   TKey key;
        internal int  hashcode;
        internal int  previous;
        internal int  next;

        public SveltoDictionaryNode(ref TKey key, int hash, int previousNode)
        {
            this.key = key;
            hashcode = hash;
            previous = previousNode;
            next = -1;
        }

        public SveltoDictionaryNode(ref TKey key, int hash)
        {
            this.key = key;
            hashcode = hash;
            previous = -1;
            next = -1;
        }
    }
}