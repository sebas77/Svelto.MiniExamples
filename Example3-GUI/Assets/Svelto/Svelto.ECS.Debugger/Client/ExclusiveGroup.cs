namespace Svelto.ECS.Debugger
{
    public class ExclusiveGroup : ECS.ExclusiveGroup
    {
        public ExclusiveGroup(string name) : base()
        {
            var id = (uint) this;
            Debugger.RegisterNameGroup(id, name);
        }

        public ExclusiveGroup(ushort range, string name) : base(range)
        {
            var id = (uint) this;
            Debugger.RegisterNameGroup(id, name);
        }
    }
}