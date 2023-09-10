using System.Collections.Generic;
using UnityEngine;

namespace Svelto.ECS.GUI.Extensions.Unity
{
    class WidgetDescriptorGroup : MonoBehaviour
    {
        public IEnumerable<BaseWidgetDescriptorHolder> Widgets => _widgets;

        [SerializeField] BaseWidgetDescriptorHolder[] _widgets;
    }
}