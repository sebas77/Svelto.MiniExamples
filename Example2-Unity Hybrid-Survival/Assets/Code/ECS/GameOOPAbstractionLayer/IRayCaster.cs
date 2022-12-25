using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public interface IRayCaster
    {
        bool CheckHit(in Ray ray, float range, int layer, int mask, int enemyMask, out Vector3 point,
            out EntityReference instanceID);
        bool CheckHit(in Ray ray, float range, int mask,  out Vector3 point);
    }
}