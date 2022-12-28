using Svelto.ECS.Example.Survive.Enemies;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class SyncVFXEntitiesToObjects: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncVFXEntitiesToObjects(GameObjectResourceManager manager)
        {
            _manager = manager;
        }

        public void Ready() { }
        public EntitiesDB entitiesDB { get; set; }
        
        public void Step()
        {
            var groups = entitiesDB.FindGroups<GameObjectEntityComponent, VFXComponent>();
            
            foreach (var ((entity, vfxs, count), _) in entitiesDB
                            .QueryEntities<GameObjectEntityComponent, VFXComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    ref var vfxEventReference = ref vfxs[i].vfxEvent;

                    if (vfxEventReference.play == true)
                    {
                        var go = _manager[entity[i].resourceIndex];

                        //could probably do with a check if the state actually changed
                        var vfx = go.GetComponent<EntityVFX>();

                        vfx.play(vfxEventReference.position);

                        vfxEventReference.play = false;
                    }
                }
            }
        }

        public string name => nameof(SyncVFXEntitiesToObjects);

        readonly GameObjectResourceManager _manager;
    }
}