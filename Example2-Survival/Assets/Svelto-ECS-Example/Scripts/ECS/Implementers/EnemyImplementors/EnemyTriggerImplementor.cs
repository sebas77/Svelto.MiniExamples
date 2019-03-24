using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    //Implementors act as bridge between Svelto.ECS Engines and third party platforms.
    //This featureaw is of fundamental importance. If you need unity to communicate with the engines
    //you don't need to use awkward workarounds, simply create an implementor as Monobehaviour.
    //In this way you could use, inside the implementor, Unity callbacks, like OnTriggerEnter/OnTriggerExit
    //and change data according the Unity callback. Logic should not be used inside these callback,
    //except setting entity components data.
    //This is the case when implementor can and should be Monobehaviours. Here an example:
    public class EnemyTriggerImplementor : MonoBehaviour, IImplementor, IEnemyTriggerComponent
    {
        public EnemyCollisionData entityInRange { get; private set; }

        void OnTriggerEnter(Collider other)
        {
            entityInRange = new EnemyCollisionData(new EGID(other.gameObject.GetInstanceID(), ECSGroups.EnemyTargets), true);
        }

        void OnTriggerExit(Collider other)
        {
            entityInRange = new EnemyCollisionData(new EGID(other.gameObject.GetInstanceID(), ECSGroups.EnemyTargets), false);
        }

        bool    _targetInRange;
    }
}
