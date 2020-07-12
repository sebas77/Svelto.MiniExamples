using Svelto.ECS;
using Svelto.ECS.Extensions.Unity;
using UnityEngine;

public interface IRayCaster
{
    bool CheckHit(Ray ray, float range, int layer, int mask, out Vector3 point, out EGID instanceID);
    bool CheckHit(Ray ray, float range, int mask,  out Vector3 point);
}

public class RayCaster : IRayCaster
{
    public bool CheckHit(Ray ray, float range, int layer, int mask, out Vector3 point, out EGID instanceID)
    {
        RaycastHit shootHit;
        if (Physics.Raycast(ray, out shootHit, range, mask))
        {
            point = shootHit.point;
            if (shootHit.collider != null)
            {
                var colliderGameObject = shootHit.collider.gameObject;

                if (colliderGameObject.layer == layer)
                {
                    var component = colliderGameObject.GetComponent<EGIDHolderImplementor>();
                    if (component != null)
                    {
                        instanceID = component.ID;
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

    public bool CheckHit(Ray ray, float range, int mask, out Vector3 point)
    {
        RaycastHit shootHit;
        if (Physics.Raycast(ray, out shootHit, range, mask))
        {
            point = shootHit.point;
            if (shootHit.collider != null) return true;
        }

        point = new Vector3();
        return false;
    }
}