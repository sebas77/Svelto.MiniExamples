using Svelto.ECS.Example.Survive.Damage;

namespace Svelto.ECS.Example.Survive.Player
{
    public class PlayerTag : GroupTag<PlayerTag> { };
    
    //in this cased a GroupCompound is used to identify entities that are players and can be damaged
    //the DamageableTag is a shared adjective between entity descriptors and can be used to query all
    //the entities that are damageable in the abstracted damage layer engines
    //Group compound tags should be seen ad adjective and states of a tag, can also be seen as an alternative
    //approach to have components that can be removed and added in entities to model dynamic states
    public class Player : GroupCompound<PlayerTag, DamageableTag> { };
}