namespace MiniExamples.DeterministicPhysicDemo
{
    public static class EgidFactory
    {
        private static uint _lastEgid = 0;

        public static uint GetNextId()
        {
            return _lastEgid += 1;
        }
    }
}