using System;
using Svelto.ECS.GUI.Resources;
using UnityEngine.Serialization;

namespace Svelto.ECS.GUI.Commands
{

    [Serializable]
    public struct SerializedCommandData
    {
        [FormerlySerializedAs("CommandName")] public string commandName;
        [FormerlySerializedAs("Target")]      public string target;
        [FormerlySerializedAs("Parameter1")]  public string parameter1;
        [FormerlySerializedAs("Parameter2")]  public string parameter2;
        [FormerlySerializedAs("Parameter2")]  public string parameter3;
    }

    public struct CommandData
    {
        public int         id;
        public EcsResource target;

        // TODO: Make it a dynamic array.
        public EcsResource parameter1;
        public EcsResource parameter2;
        public EcsResource parameter3;
    }
}