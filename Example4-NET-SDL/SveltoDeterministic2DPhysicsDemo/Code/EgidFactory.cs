namespace MiniExamples.DeterministicPhysicDemo
{
    public static class EgidFactory
    {
        static uint _lastEgid = 0;

        public static uint GetNextId()
        {
            return _lastEgid += 1;
        }
    }
}