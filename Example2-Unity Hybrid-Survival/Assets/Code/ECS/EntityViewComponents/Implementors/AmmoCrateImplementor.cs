using UnityEngine;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Extensions.Unity;

namespace Svelto.ECS.Example.Survive.Weapons
{
    public class AmmoCrateImplementor : MonoBehaviour, IImplementor, IAmmoComponent, IAmmoTriggerComponent
    {
        public DispatchOnChange<AmmoCollisionData> hitChange { get; set; }

        public int ammoValue { get; set; }
        public Quaternion rotation { get => _transform.rotation; set => _transform.rotation = value; }

        void Awake()
        {
            _transform = transform;
        }

        void OnTriggerEnter(Collider other)
        {
            if (hitChange != null && other.attachedRigidbody != null)
            {
                EntityReferenceHolderImplementor otherEntityReferenceImp = other.gameObject.GetComponent<EntityReferenceHolderImplementor>();
                if (otherEntityReferenceImp != null)
                {
                    hitChange.value = new AmmoCollisionData(otherEntityReferenceImp.reference, true);
                }
                    
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (hitChange != null && other.attachedRigidbody != null)
            {
                EntityReferenceHolderImplementor ammoEntityViewComponent = other.gameObject.GetComponent<EntityReferenceHolderImplementor>();
                if (ammoEntityViewComponent != null)
                    hitChange.value = new AmmoCollisionData(ammoEntityViewComponent.reference, false);
            }
        }

        Transform _transform;
    }
}