namespace Svelto.ECS.MiniExamples.Example1C
{
    public struct MealEntityComponent : IEntityComponent, INeedEGID
    {
        public int mealLeft;
        public int eaters;

        public MealEntityComponent(int amountOfFood) : this() { mealLeft = amountOfFood; }
        public EGID ID { get; set; }
    }
}