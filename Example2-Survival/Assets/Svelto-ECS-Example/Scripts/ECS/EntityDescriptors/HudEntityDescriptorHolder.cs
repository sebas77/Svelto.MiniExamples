using Svelto.ECS.Unity;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.HUD
{
	public class HudEntityDescriptor : GenericEntityDescriptor<HUDEntityView>
	{}
	
    [DisallowMultipleComponent]
	public class HudEntityDescriptorHolder : GenericEntityDescriptorHolder<HudEntityDescriptor>
	{}
}
