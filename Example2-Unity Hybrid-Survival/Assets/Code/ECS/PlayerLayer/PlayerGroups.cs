using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.Enemies;

namespace Svelto.ECS.Example.Survive.Player
{
    public class Player: GroupTag<Player> { };

    //in this case a GroupCompound is used to identify entities that are players and can be damaged
    //the DamageableTag is a shared adjective between entity descriptors and can be used to query all
    //the entities that are damageable in the abstracted damage layer engines
    //Group compound tags should be seen adjectives and states of a tag, can also be seen as an alternative
    //approach to have components that can be removed and added in entities to model dynamic states

    //The player is also an enemy target. The tag is provided by the EnemyLayer (thus more abstract than the player one)
    //however GroupCompound supports up to 4 tags only, so if they are not enough filters could be used
    //i.e.: the enemy layers could have worked just with filters and player added in the enemy target filter

    public class PlayerAliveGroup: GroupCompound<Player, Damageable, EnemyTarget> { };

    public class PlayerDeadGroup: GroupCompound<Player, Dead>
    {
        static PlayerDeadGroup()
        {
            bitmask = ExclusiveGroupBitmask.DISABLED_BIT;
        }
    };
}