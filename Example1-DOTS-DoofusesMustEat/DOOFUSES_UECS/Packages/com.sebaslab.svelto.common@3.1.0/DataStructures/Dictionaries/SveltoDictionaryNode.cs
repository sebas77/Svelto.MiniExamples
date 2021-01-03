namespace Svelto.DataStructures
{
    public struct SveltoDictionaryNode<TKey>
    {
        public readonly TKey key;
        public readonly int  hashcode;
        public          int  previous;
        public          int  next;

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