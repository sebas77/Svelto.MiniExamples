using System.Collections;
using Svelto.ECS.Example.Survive.Characters.Player;

namespace Svelto.ECS.Example.Survive.Characters.Sounds
{
    public class DamageSoundEngine : IQueryingEntitiesEngine, IStep<PlayerDeathCondition>, IStep
    {
        public IEntitiesDB entitiesDB { set; private get; }

        public void Ready()
        {
            CheckForDamage().Run();
        }

        void TriggerDeathSound(EGID targetID)
        {
            uint index;
            entitiesDB.QueryEntitiesAndIndex<DamageSoundEntityView>(targetID,
                                                                    out index)[index]
                      .audioComponent.playOneShot = AudioType.death;
        }

        IEnumerator CheckForDamage()
        {
            while (true)
            {
                foreach (var group in ECSGroups.DamageableGroups)
                {
                    int count;
                    var damageableEntities = entitiesDB.QueryEntities<DamageableEntityStruct>(group, out count);
                    var damageSounds       = entitiesDB.QueryEntities<DamageSoundEntityView>(group, out count);
                    for (int i = 0; i < count; i++)
                    {
                        if (damageableEntities[i].damaged == false) continue;

                        damageSounds[i].audioComponent.playOneShot = AudioType.damage;
                    }
                }

                yield return null;
            }
        }

        public void Step(PlayerDeathCondition condition, EGID id)
        {
            TriggerDeathSound(id);
        }

        public void Step(EGID id)
        {
            TriggerDeathSound(id);
        }
    }
}
