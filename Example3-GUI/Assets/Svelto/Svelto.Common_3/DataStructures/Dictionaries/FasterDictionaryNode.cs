namespace Svelto.DataStructures
{
    public struct FasterDictionaryNode<TKey>
    {
        public readonly TKey key;
        public readonly int  hashcode;
        public          int  previous;
        public          int  next;

        public FasterDictionaryNode(ref TKey key, int hash, int previousNode)
        {
            this.key = key;
            hashcode = hash;
            previous = previousNode;
            next = -1;
        }

        public FasterDictionaryNode(ref TKey key, int hash)
        {
            this.key = key;
            hashcode = hash;
            previous = -1;
            next = -1;
        }
    }
}