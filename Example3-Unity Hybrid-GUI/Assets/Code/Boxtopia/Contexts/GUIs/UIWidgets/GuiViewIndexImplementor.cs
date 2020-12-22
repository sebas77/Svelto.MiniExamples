using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Boxtopia.GUIs.Generic
{
    public class GuiViewIndexImplementor : MonoBehaviour, IGuiViewIndex, IImplementor
    {
        public int Index;
        public string GroupName;

        public int index => Index;
        public string groupName => GroupName;
    }
}