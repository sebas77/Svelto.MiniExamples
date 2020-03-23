using System.Collections.Generic;
using Svelto.ECS.Debugger.DebugStructure;

namespace Svelto.ECS.Debugger.Editor.ListViews
{
    public delegate DebugEntity EntitySelectionGetter();
    public delegate DebugGroup GroupSelectionGetter();
    public delegate DebugRoot RootSelectionGetter();
    public delegate void RootSelectionSetter(DebugRoot root);
    public delegate List<DebugRoot> RootListGetter();
    public delegate void GroupSelectionCallback(uint? manager);
    public delegate void EntitySelectionCallback(uint? entity);
}