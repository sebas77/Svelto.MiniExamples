namespace Svelto.ECS.MiniExamples.Example1C
{
    public struct MealEntityStruct : IEntityComponent, INeedEGID
    {
        public int mealLeft;
        public int eaters;

        public MealEntityStruct(int amountOfFood) : this() { mealLeft = amountOfFood; }
        public EGID ID { get; set; }
    }
}