namespace Svelto.ECS.MiniExamples.Doofuses.Stride.StrideLayer
{
    public class StrideEntityDescriptor: ExtendibleEntityDescriptor<TransformableEntityDescriptor>
    {
        public StrideEntityDescriptor()
        {
            Add<StrideComponent>();
        }
    }
}