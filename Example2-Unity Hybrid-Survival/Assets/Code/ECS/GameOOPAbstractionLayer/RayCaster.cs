using Svelto.ECS.Extensions.Unity;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class RayCaster : IRayCaster
    {
        public bool CheckHit(in Ray ray, float range, int layer, int mask, out Vector3 point, out EntityReference instanceID)
        {
            if (Physics.Raycast(ray, out var shootHit, range, mask))
            {
                point = shootHit.point;
                if (shootHit.collider != null)
                {
                    var colliderGameObject = shootHit.collider.gameObject;

                    if (colliderGameObject.layer == layer)
                    {
                        var component = colliderGameObject.GetComponent<EntityReferenceHolder>();
                        if (component != null)
                        {
                            instanceID = component.reference;
                            return true;
                        }
                    }
                
                    instanceID = default;
                    return true;
                }
            }

            point      = new Vector3();
            instanceID = default;
            return false;
        }

        public bool CheckHit(in Ray ray, float range, int mask, out Vector3 point)
        {
            if (Physics.Raycast(ray, out var shootHit, range, mask))
            {
                point = shootHit.point;
                if (shootHit.collider != null) return true;
            }

            point = new Vector3();
            return false;
        }
    }
}