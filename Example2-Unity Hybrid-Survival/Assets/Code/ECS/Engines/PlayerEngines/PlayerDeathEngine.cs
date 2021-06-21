using Svelto.Common;
using Svelto.ECS.Example.Survive.Player;

namespace Svelto.ECS.Example.Survive
{
    [Sequenced(nameof(PlayerEnginesNames.PlayerDeathEngine))]
    public class PlayerDeathEngine : IStepEngine, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    {  }
        
        public PlayerDeathEngine(IEntityFunctions dbFunctions, IEntityStreamConsumerFactory consumerFactory)
        {
            _DBFunctions       = dbFunctions;
            _consumer = consumerFactory.GenerateConsumer<DeathComponent>("PlayerDeathEngine", 1);
        }

        public void Step()
        {
            while (_consumer.TryDequeue(out _, out EGID id))
            {
                if (id.groupID.FoundIn(Player.Player.Groups))
                {
                    //remove the player entity so the player engines will stop processing it 
                    _DBFunctions.RemoveEntity<PlayerEntityDescriptor>(id);
                    _DBFunctions.RemoveEntity<PlayerGunEntityDescriptor>(entitiesDB.QueryEntity<PlayerWeaponComponent>(id).weapon.ToEGID(entitiesDB));
                }
            }
        }

        public string name => nameof(PlayerDeathEngine);
        
        readonly IEntityFunctions _DBFunctions;
        Consumer<DeathComponent>  _consumer;
    }
}
