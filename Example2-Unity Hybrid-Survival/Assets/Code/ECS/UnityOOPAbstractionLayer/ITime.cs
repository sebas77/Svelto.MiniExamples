namespace Svelto.ECS.Example.Survive
{
    public interface ITime
    {
        float deltaTime { get; }
    }

    public class Time : ITime
    {
        public float deltaTime => UnityEngine.Time.deltaTime;
    }
}