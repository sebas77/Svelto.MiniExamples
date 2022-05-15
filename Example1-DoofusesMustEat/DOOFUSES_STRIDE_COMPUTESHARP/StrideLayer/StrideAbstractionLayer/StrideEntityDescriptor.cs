namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp.StrideLayer
{
    public class StrideEntityDescriptor: ExtendibleEntityDescriptor<TransformableEntityDescriptor>
    {
        public StrideEntityDescriptor()
        {
            Add<StrideComponent>();
        }
    }
}