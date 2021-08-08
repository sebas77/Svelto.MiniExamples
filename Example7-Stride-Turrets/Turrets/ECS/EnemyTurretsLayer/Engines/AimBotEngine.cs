using Stride.Core.Mathematics;
using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class AimBotEngine : SyncScript, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public override void Update()
        {
            var targetGroups = entitiesDB.FindGroups<PositionComponent, TurretTargetComponent>();

            foreach (var ((matrix, directionComponent, count), _) in entitiesDB
               .QueryEntities<MatrixComponent, LookAtComponent>(BotTag.Groups))
            {
                foreach (var ((targetPosition, countTargets), _) in entitiesDB
                   .QueryEntities<PositionComponent>(targetGroups))
                {
                    for (int i = 0; i < count; i++)
                    {
                        var j      = i < countTargets - 1 ? i : countTargets - 1;
                        var vector = targetPosition[j].position - matrix[i].matrix.TranslationVector;
                        vector.Normalize();
                        directionComponent[i].vector = vector;
                    }
                }
            }
        }
    }
}