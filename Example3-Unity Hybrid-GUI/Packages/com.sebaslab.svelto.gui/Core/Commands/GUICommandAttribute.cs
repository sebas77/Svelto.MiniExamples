using System;

namespace Svelto.ECS.GUI.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GUICommandAttribute : Attribute
    {
        public string commandName;
        public string category;
        public bool   autoRegister;

        public GUICommandAttribute(string name, string category = "Generic", bool autoRegister = true)
        {
            this.commandName = name;
            this.category = category;
            this.autoRegister = autoRegister;
        }
    }
}