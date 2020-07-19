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
            entityView.buttonClick.buttonEvent = new DispatchOnSet<ButtonEvents>(egid);
            
            entityView.buttonClick.buttonEvent.NotifyOnValueSet(_enqueueButtonChange);
        }

        public void Remove(ref ButtonEntityViewComponent entityView, EGID egid)
        {
            entityView.buttonClick.buttonEvent.StopNotify();
        }

        void EnqueueButtonChange(EGID egid, ButtonEvents value)
        {
            entitiesDB.QueryEntity<ButtonEntityComponent>(egid) = new ButtonEntityComponent(egid, value);
            
            entitiesDB.PublishEntityChange<ButtonEntityComponent>(egid);
        }
        
        Action<EGID, ButtonEvents> _enqueueButtonChange;
   }
}