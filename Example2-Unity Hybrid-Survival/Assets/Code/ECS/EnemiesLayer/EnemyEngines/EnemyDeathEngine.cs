using System.Collections;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.OOPLayer;

using UnityEngine;
using AudioType = Svelto.ECS.Example.Survive.Damage.AudioType;

namespace Svelto.ECS.Example.Survive.Enemies
{
    [Sequenced(nameof(EnemyEnginesNames.EnemyDeathEngine))]
    public class EnemyDeathEngine: IQueryingEntitiesEngine, IStepEngine, IReactOnSwapEx<EnemyComponent>,
            IReactOnRemoveEx<EnemyComponent>
    {
        public EnemyDeathEngine(IEntityFunctions entityFunctions,
            ITime time, WaitForSubmissionEnumerator waitForSubmission, GameObjectResourceManager manager)
        {
            _entityFunctions = entityFunctions;
            _time = time;
            _waitForSubmission = waitForSubmission;
            _manager = manager;
            _animations = new FasterList<IEnumerator>();
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            _sveltoFilters = entitiesDB.GetFilters();
        }

        public void Step()
        {
            var deadEntitiesFilter = _sveltoFilters.GetTransientFilter<HealthComponent>(FilterIDs.DeadEntitiesFilter);

            foreach (var (filteredIndices, group) in deadEntitiesFilter)
            {
                if (EnemyAliveGroup.Includes(group)) //is it an enemy?
                {
                    var (animation, sound, entityIDs, _) = entitiesDB.QueryEntities<AnimationComponent, SoundComponent>(group);
                    var indicesCount = filteredIndices.count;
                    for (int i = 0; i < indicesCount; i++)
                    {
                        var filteredIndex = filteredIndices[i];
                        
                        animation[filteredIndex].animationState = new AnimationState((int)EnemyAnimations.Die);
                        sound[filteredIndex].playOneShot = (int)AudioType.death;

                        _animations.Add(PlayDeathSequence(new EGID(entityIDs[filteredIndex], group)));
                    }
                }
            }

            for (uint i = 0; i < _animations.count; i++)
                if (_animations[i].MoveNext() == false)
                    _animations.UnorderedRemoveAt(i--);
        }

        public string name => nameof(EnemyDeathEngine);

        public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<EnemyComponent> entities,
            ExclusiveGroupStruct groupID)
        {
            if (EnemyDeadGroup.Includes(groupID)) //is a dead enemy?
            {
                var (enemies, _) = entities;
                var (gos, _) = entitiesDB.QueryEntities<GameObjectEntityComponent>(groupID);

                for (int i = (int)(rangeOfEntities.end - 1); i >= (int)rangeOfEntities.start; i--)
                {
                    //recycle the gameobject
                    _manager.Recycle(gos[i].resourceIndex, (int)enemies[i].enemyType);
                }
            }
        }

        /// <summary>
        /// One of the available form of communication in Svelto.ECS: React On Swap allow to do what it says
        /// </summary>
        public void MovedTo((uint start, uint end) rangeOfEntities,
            in EntityCollection<EnemyComponent> entities,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
        {
            if (EnemyDeadGroup.Includes(toGroup)) //is an enemy just killed?
            {
                var (gos, navs, _) = entitiesDB.QueryEntities<GameObjectEntityComponent, NavMeshComponent>(toGroup);

                for (int i = (int)(rangeOfEntities.end - 1); i >= (int)rangeOfEntities.start; i--)
                {
                    navs[i].navMeshEnabled = false;
                    navs[i].setCapsuleAsTrigger = true;
                    gos[i].layer = GAME_LAYERS.NOT_SHOOTABLE_LAYER;
                }
            }
        }

        IEnumerator PlayDeathSequence(EGID egid)
        {
            //Any build/swap/remove do not happen immediately, but at specific sync points
            //swapping group because we don't want any engine to pick up this entity while it's animating for death
            _entityFunctions.SwapEntityGroup<EnemyEntityDescriptor>(egid, EnemyDeadGroup.BuildGroup);

            //wait for the swap to happen
            while (_waitForSubmission.MoveNext())
                yield return null;

            var wait = new WaitForSecondsEnumerator(1);

            //new egid after the swap
            var newEgid = new EGID(egid.entityID, EnemyDeadGroup.BuildGroup);
            
            //sinking for 2 seconds
            while (wait.MoveNext())
            {
                //I cannot assume that the database doesn't change while the enemy sinks, so I have to query every frame 
                entitiesDB.QueryEntity<PositionComponent>(newEgid).position += -Vector3.up * 1.2f * _time.deltaTime;

                yield return null;
            }

            //Note: possibly this should stay in the enemyFactory:
            //is now possible to delete the entity from the database
            _entityFunctions.RemoveEntity<EnemyEntityDescriptor>(newEgid);
        }

        readonly IEntityFunctions _entityFunctions;
        readonly ITime _time;
        readonly WaitForSubmissionEnumerator _waitForSubmission;
        readonly GameObjectResourceManager _manager;
        readonly FasterList<IEnumerator> _animations;
        EntitiesDB.SveltoFilters _sveltoFilters;
    }
}