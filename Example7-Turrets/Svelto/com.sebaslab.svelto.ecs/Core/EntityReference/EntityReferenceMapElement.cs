namespace Svelto.ECS
{
    struct EntityReferenceMapElement
    {
        internal EGID egid;
        internal uint version;

        internal EntityReferenceMapElement(EGID egid)
        {
            this.egid = egid;
            version = 0;
        }
    }
}