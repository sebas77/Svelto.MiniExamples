using System;
using Svelto.ECS;

namespace Boxtopia.GUIs.Generic
{
    public class ButtonClickingEventEngine : IReactOnAddAndRemove<ButtonEntityViewStruct>, IQueryingEntitiesEngine
    {
        Action<EGID, ButtonEvents> _enqueueButtonChange;
        public IEntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            _enqueueButtonChange = EnqueueButtonChange;
        }

        public void Add(ref ButtonEntityViewStruct entityView)
        {
            entityView.buttonClick.buttonEvent = new DispatchOnSet<ButtonEvents>(entityView.ID);
            
            entityView.buttonClick.buttonEvent.NotifyOnValueSet(_enqueueButtonChange);
        }

        public void Remove(ref ButtonEntityViewStruct entityView)
        {
            entityView.buttonClick.buttonEvent.StopNotify(_enqueueButtonChange);
        }

        void EnqueueButtonChange(EGID egid, ButtonEvents value)
        {
            entitiesDB.QueryEntity<ButtonEntityStruct>(egid) = new ButtonEntityStruct(egid, value);
            
            entitiesDB.PublishEntityChange<ButtonEntityStruct>(egid);
        }
   }
}