using System;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class ObjectTrigger: MonoBehaviour
    {
        void Awake()
        {
            _reference = gameObject.GetComponent<EntityReferenceHolder>();
        }

        /// <summary>
        /// it's annoying that there isn't a physic matrix dedicated for triggers. This means that OnTriggerEnter
        /// will be executed for any collider entering in the enemy trigger area, including the static ones  
        /// </summary>
        /// <param name="other"></param>
        void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody != null)
            {
                EntityReferenceHolder entity = other.gameObject.GetComponent<EntityReferenceHolder>();
                if (entity != null)
                    _onCollidedWithTarget(
                        new EntityReference(_reference.reference),
                        new CollisionData(new EntityReference(entity.reference), true));
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody != null)
            {
                EntityReferenceHolder entity = other.gameObject.GetComponent<EntityReferenceHolder>();
                if (entity != null)
                    _onCollidedWithTarget(
                        new EntityReference(_reference.reference),
                        new CollisionData(new EntityReference(entity.reference), false));
            }
        }

        public void Register(Action<EntityReference, CollisionData> onCollidedWithTarget)
        {
            _onCollidedWithTarget = onCollidedWithTarget;
        }
        
        Action<EntityReference, CollisionData> _onCollidedWithTarget;
        EntityReferenceHolder _reference;
    }
}