using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    //Implementors act as bridge between Svelto.ECS Engines and third party platforms.
    //This feature is of fundamental importance to mix ECS with OOP libraries. If you need unity to communicate with the
    //Svelto Engines you don't need to use awkward workarounds, simply create an implementor as Monobehaviour.
    //In this way you could use, inside the implementor, Unity callbacks, like OnTriggerEnter/OnTriggerExit
    //and change data according the Unity callback. Logic should not be used inside these callback,
    //except setting entity components data.
    //This is the case when implementor can and should be Monobehaviours. Here an example:
    public class EnemyTriggerImplementor : MonoBehaviour, IImplementor, IEnemyTriggerComponent
    {
        bool _targetInRange;

        void OnTriggerEnter(Collider other)
        {
            hitChange.value =
                new EnemyCollisionData(new EGID((uint) other.gameObject.GetInstanceID(), ECSGroups.EnemyTargets), true);
        }

        void OnTriggerExit(Collider other)
        {
            hitChange.value =
               new EnemyCollisionData(new EGID((uint) other.gameObject.GetInstanceID(), ECSGroups.EnemyTargets), false);
        }

        public DispatchOnChange<EnemyCollisionData> hitChange { get; set; }
    }
}