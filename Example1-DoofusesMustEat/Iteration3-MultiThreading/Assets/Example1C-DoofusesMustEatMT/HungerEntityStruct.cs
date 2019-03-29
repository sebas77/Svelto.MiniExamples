namespace Svelto.ECS.MiniExamples.Example1C
{
    public struct HungerEntityStruct : IEntityStruct
    {
        public int  hunger;
        public EGID ID { get { return new EGID(); } set { } }
    }
}