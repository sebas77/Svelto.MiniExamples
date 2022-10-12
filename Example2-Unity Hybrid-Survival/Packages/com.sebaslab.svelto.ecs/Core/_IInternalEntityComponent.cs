namespace Svelto.ECS
{
    ///<summary>This interfaces shouldn't be used outside the svelto assembly</summary>
    public interface _IInternalEntityComponent
    {
    }

    ///IEntityComponents are unmanaged struct components stored in native memory. If they are not unmanaged they won't be recognised as IEntityComponent!
    public interface IEntityComponent:_IInternalEntityComponent
    {
    }
}