namespace Svelto.ECS.MiniExamples.Example1
{
    struct HungerEntityStruct : IEntityStruct
    {
        public int  hunger;
        public EGID ID { get; set; }
    }
}