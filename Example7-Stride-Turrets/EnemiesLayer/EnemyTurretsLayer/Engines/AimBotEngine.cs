using Svelto.Common.Internal;

namespace Svelto.ECS.MiniExamples.Turrets.EnemyLayer
{
    /// <summary>
    /// Iterate all the GameLayer and made them aim to the current target
    /// </summary>
    class AimBotEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public string name => this.TypeName();
        
        public void Step(in float deltaTime)
        {
            var targetGroups = entitiesDB.FindGroups<PositionComponent, TurretTargetComponent>();

            foreach (var ((matrix, directionComponent, count), _) in entitiesDB
               .QueryEntities<MatrixComponent, LookAtComponent>(BotTag.Groups))
            {
                foreach (var ((targetPosition, countTargets), _) in entitiesDB
                   .QueryEntities<PositionComponent>(targetGroups))
                {
                    //Iterate the current set of turrets
                    for (int i = 0; i < count; i++)
                    {
                        var j      = i < countTargets - 1 ? i : countTargets - 1;
                        var vector = targetPosition[j].position - matrix[i].matrix.TranslationVector;
                        vector.Normalize();
                        
                        //make each turret aim at a different target in a round robin fashion
                        //in this demo there is just one target so they will all point to the same one
                        //I really didn't test other scenario so who knows if this works
                        directionComponent[i].vector = vector;
                    }
                }
            }
        }
    }
}