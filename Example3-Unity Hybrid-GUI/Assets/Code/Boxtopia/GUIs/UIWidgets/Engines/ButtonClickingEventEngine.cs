using System;
using Svelto.ECS;

namespace Boxtopia.GUIs.Generic
{
    public class ButtonClickingEventEngine : IReactOnAddAndRemoveEx<ButtonEntityViewComponent>, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            _enqueueButtonChange = EnqueueButtonChange;
        }

        public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<ButtonEntityViewComponent> entities,
            ExclusiveGroupStruct groupID)
        {
            var (buffer, entityIDs, _) = entities;

            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
                buffer[i].buttonClick.buttonEvent = new ReactiveValue<ButtonEvents>(
                    new EGID(entityIDs[i], groupID).ToEntityReference(entitiesDB), _enqueueButtonChange);
        }

        public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<ButtonEntityViewComponent> entities,
            ExclusiveGroupStruct groupID)
        {
            var (buffer, _) = entities;

            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
                buffer[i].buttonClick.buttonEvent.StopNotify();
        }

        void EnqueueButtonChange(EntityReference reference, ButtonEvents value)
        {
            var egid = reference.ToEGID(entitiesDB);
            entitiesDB.QueryEntity<ButtonEntityComponent>(egid) = new ButtonEntityComponent(egid, value);
            
            entitiesDB.PublishEntityChange<ButtonEntityComponent>(egid);
        }
        
        Action<EntityReference, ButtonEvents> _enqueueButtonChange;
   }
}
