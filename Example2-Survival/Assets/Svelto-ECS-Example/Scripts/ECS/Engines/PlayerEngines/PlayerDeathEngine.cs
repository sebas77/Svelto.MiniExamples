using System.Collections;
using Svelto.ECS.Example.Survive.Characters;
using Svelto.ECS.Example.Survive.Characters.Player;

namespace Svelto.ECS.Example.Survive
{
    public class PlayerDeathEngine : IQueryingEntitiesEngine
    {
        public PlayerDeathEngine(PlayerDeathSequencer playerDeathSequence, IEntityFunctions functions)
        {
            _playerDeathSequence = playerDeathSequence;
            _functions = functions;
        }

        public IEntitiesDB entitiesDB { get; set; }
        public void Ready()
        {
            CheckIfDead().Run();
        }

        IEnumerator CheckIfDead()
        {
            while (true)
            {
                int numberOfPlayers;
                var players = entitiesDB.QueryEntities<HealthEntityStruct>(ECSGroups.Player, out numberOfPlayers);
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    if (players[i].dead)
                    {
                        _playerDeathSequence.Next(this, PlayerDeathCondition.Death, players[i].ID);
                        
                        _functions.RemoveEntity<PlayerEntityDescriptor>(players[i].ID);
                    }
                }

                yield return null;
            }
        }
        
        readonly PlayerDeathSequencer _playerDeathSequence;
        readonly IEntityFunctions _functions;
        
    }
}