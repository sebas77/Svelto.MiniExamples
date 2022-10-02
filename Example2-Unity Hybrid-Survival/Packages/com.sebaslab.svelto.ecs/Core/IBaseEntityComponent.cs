namespace Svelto.ECS
{
    ///<summary>Entity Components MUST implement IBaseEntityComponent</summary>
    public interface IBaseEntityComponent
    {
    }

    ///IEntityComponents are unmanaged struct components stored in native memory. If they are not unmanaged they won't be recognised as IEntityComponent!
    public interface IEntityComponent:IBaseEntityComponent
    {
    }
}