namespace Svelto.ECS
{
    ///<summary>EntityStruct MUST implement IEntityStruct</summary>
    public interface IEntityStruct
    {
    }

    /// <summary>
    /// use INeedEGID on an IEntityStruct only if you need the EGID
    /// </summary>
    public interface INeedEGID
    {
        //The set is used only for the framework, but it must stay there
        EGID ID { get; set; }
    }
}