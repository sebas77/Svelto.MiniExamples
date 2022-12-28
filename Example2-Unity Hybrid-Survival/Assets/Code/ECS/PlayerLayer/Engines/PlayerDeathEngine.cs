using Svelto.Common;

namespace Svelto.ECS.Example.Survive.Player
{
    [Sequenced(nameof(PlayerEnginesNames.PlayerDeathEngine))]
    public class PlayerDeathEngine : IStepEngine, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }
        
        public PlayerDeathEngine(IEntityFunctions dbFunctions)
        {
            _DBFunctions = dbFunctions;
        }

        public void Step()
        {
//            {
//                if (id.groupID.FoundIn(PlayerGroup.Groups))
//                {
//                    //remove the player entity so the player engines will stop processing it 
//                    _DBFunctions.RemoveEntity<PlayerEntityDescriptor>(id);
//                    _DBFunctions.RemoveEntity<PlayerGunEntityDescriptor>(entitiesDB
//                       .QueryEntity<WeaponComponent>(id).weapon.ToEGID(entitiesDB));
//                }
//            }
        }

        public string name => nameof(PlayerDeathEngine);

        readonly IEntityFunctions _DBFunctions;
    }
}