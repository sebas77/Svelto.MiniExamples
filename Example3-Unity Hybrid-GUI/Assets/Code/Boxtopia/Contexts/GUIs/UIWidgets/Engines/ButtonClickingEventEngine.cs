using System;
using Svelto.ECS;

namespace Boxtopia.GUIs.Generic
{
    public class ButtonClickingEventEngine : IReactOnAddAndRemove<ButtonEntityViewComponent>, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            _enqueueButtonChange = EnqueueButtonChange;
        }

        public void Add(ref ButtonEntityViewComponent entityView, EGID egid)
        {
            entityView.buttonClick.buttonEvent = new ReactiveValue<ButtonEvents>(egid.ToEntityReference(entitiesDB), _enqueueButtonChange);
        }

        public void Remove(ref ButtonEntityViewComponent entityView, EGID egid)
        {
            entityView.buttonClick.buttonEvent.StopNotify();
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