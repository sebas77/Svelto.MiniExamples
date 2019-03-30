using System.Collections;

namespace Svelto.ECS.Example.Survive.Characters
{
    /// <summary>
    ///     The responsibility of this engine is to apply the damage to any
    ///     damageable entity. If the logic applied to the enemy was different
    ///     than the logic applied to the player, I would have created two
    ///     different engines
    /// </summary>
    public class ApplyingDamageToTargetsEngine : IQueryingEntitiesEngine
    {
        public void Ready() { ApplyDamage().Run(); }

        public IEntitiesDB entitiesDB { set; private get; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        IEnumerator ApplyDamage()
        {
            while (true)
            {
                foreach (var group in ECSGroups.TargetGroups)
                {
                    var entities =
                        entitiesDB.QueryEntities<DamageableEntityStruct, HealthEntityStruct>(group, out var count);

                    for (var i = 0; i < count; i++)
                    {
                        var damagedEntites = entities.Item1;
                        var entitiesHealth = entities.Item2;
                        if (damagedEntites[i].damageInfo.shotDamageToApply > 0)
                        {
                            entitiesHealth[i].currentHealth -=
                                damagedEntites[i].damageInfo.shotDamageToApply;
                            damagedEntites[i].damageInfo.shotDamageToApply = 0;
                            damagedEntites[i].damaged                      = true;
                        }
                        else
                        {
                            damagedEntites[i].damaged = false;
                        }
                    }
                }

                yield return null;
            }
        }
    }
}