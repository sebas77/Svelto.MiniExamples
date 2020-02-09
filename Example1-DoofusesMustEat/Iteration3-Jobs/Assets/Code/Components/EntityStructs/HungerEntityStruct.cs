namespace Svelto.ECS.MiniExamples.Example1B
{
    public struct HungerEntityStruct : IEntityStruct, INeedEGID
    {
        public int  hunger;
        public EGID ID { get; set; }
    }
}