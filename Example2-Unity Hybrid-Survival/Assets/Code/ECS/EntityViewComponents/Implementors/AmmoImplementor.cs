using Svelto.ECS.Hybrid;
using Svelto.ECS.Extensions.Unity;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Pickups
{
    public class AmmoImplementor : MonoBehaviour, IImplementor, ITransformComponent, ISpawnedComponent, IAmmoTriggerComponent
    {
        public Vector3 position
        {
            get => _ammoTransform.position;
            set => _ammoTransform.position = value;
        }

        public bool spawned { 
            get => _spawned;
            set => SetSpawn(value); 
        }

        public Quaternion rotation      { set => _ammoTransform.rotation = value; }

        void Awake()
        {
            _ammoCollider  = GetComponent<Collider>();
            _ammoTransform = transform;
        }

        public void SetSpawn(bool value)
        {
            _spawned = value;
            var ammoMeshRenderer = GetComponent<MeshRenderer>();
            var ammoSphereCollider = GetComponent<SphereCollider>();

            if (value)
            {
                // Spawned set to true, so activate mesh renderer and sphere collider.
                ammoMeshRenderer.enabled = true;
                ammoSphereCollider.enabled = true;
            }
            else
            {
                ammoMeshRenderer.enabled = false;
                ammoSphereCollider.enabled = false;
            }
        }

        // Collider trigger

        public DispatchOnChange<EntityReference> hitChange { get; set; }


        void OnTriggerEnter(Collider other)
        {
            if (hitChange != null && other.attachedRigidbody != null)
            {
                EntityReferenceHolderImplementor targetEntityViewComponent = other.gameObject.GetComponent<EntityReferenceHolderImplementor>();
                if (targetEntityViewComponent != null)
                    hitChange.value = targetEntityViewComponent.reference;
            }
        }
        
        bool      _spawned;
        Transform _ammoTransform;
        Collider  _ammoCollider;
    }
}