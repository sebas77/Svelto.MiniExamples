using UnityEngine;

public interface IRayCaster
{
    bool CheckHit(Ray ray, float range, int layer, int mask, out Vector3 point, out int instanceID);
    bool CheckHit(Ray ray, float range, int mask, out Vector3 point);
}

public class RayCaster : IRayCaster
{
        public bool CheckHit(Ray ray, float range, int layer, int mask, out Vector3 point, out int instanceID)
        {
            RaycastHit shootHit;
            if (Physics.Raycast(ray,
                                out shootHit, range, mask) == true)
            {
                point = shootHit.point;
                if (shootHit.collider != null)
                {
                    var colliderGameObject = shootHit.collider.gameObject;

                    if (colliderGameObject.layer == layer)
                        instanceID = colliderGameObject.GetInstanceID();
                    else
                        instanceID = -1;
                    
                    return true;
                }
            }

            point = new Vector3();
            instanceID = -1;
            return false;
        }        
        
        public bool CheckHit(Ray ray, float range, int mask, out Vector3 point)
        {
            RaycastHit shootHit;
            if (Physics.Raycast(ray,
                                out shootHit, range, mask) == true)
            {
                point = shootHit.point;
                if (shootHit.collider != null)
                {
                    return true;
                }
            }

            point = new Vector3();
            return false;
        }        
    }
