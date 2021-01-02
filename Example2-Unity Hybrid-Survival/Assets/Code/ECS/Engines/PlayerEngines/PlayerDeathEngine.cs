using Svelto.Common;
using Svelto.ECS.Example.Survive.Characters;
using Svelto.ECS.Example.Survive.Characters.Player;

namespace Svelto.ECS.Example.Survive
{
    [Sequenced(nameof(PlayerEnginesNames.PlayerDeathEngine))]
    public class PlayerDeathEngine : IStepEngine
    {
        public PlayerDeathEngine(IEntityFunctions dbFunctions, IEntityStreamConsumerFactory consumerFactory)
        {
            _DBFunctions       = dbFunctions;
            _consumer = consumerFactory.GenerateConsumer<DeathComponent>(ECSGroups.PlayersGroup, "PlayerDeathEngine", 1);
        }

        public void Step()
        {
            while (_consumer.TryDequeue(out _, out EGID id))
            {
                //remove the player entity so the player engines will stop processing it 
                _DBFunctions.RemoveEntity<PlayerEntityDescriptor>(id);
                _DBFunctions.RemoveEntity<PlayerGunEntityDescriptor>(new EGID(id.entityID, ECSGroups.PlayersGunsGroup));
            }
        }

        public string name => nameof(PlayerDeathEngine);
        
        readonly IEntityFunctions             _DBFunctions;
        Consumer<DeathComponent>              _consumer;
    }
}
