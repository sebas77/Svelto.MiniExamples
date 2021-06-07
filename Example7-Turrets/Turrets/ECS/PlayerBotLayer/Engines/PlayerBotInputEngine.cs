using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;

namespace Svelto.ECS.MiniExamples.Turrets
{
    //Should be an input engine for each player bot state?
    //in theory an input engine should never set values, should only switch groups
    //
    
    //TODO an input engine can store any kind of value, because at the end of the day values are states, but on input related components only
    class PlayerBotInputEngine : SyncScript, IQueryingEntitiesEngine
    {
        public override void Update()
        {
            if (Input.HasKeyboard)
            {
                foreach (var ((speeds, directions, count), _) in entitiesDB
                   .QueryEntities<SpeedComponent, DirectionComponent>(PlayerBotTag.Groups))
                {
                    if (Input.IsKeyDown(Keys.W))
                    {
                        for (int i = 0; i < count; i++)
                        {
                            speeds[i].value      = 1.0f;
                            directions[i].vector = new Vector3(1, 0, 0);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            speeds[i].value      = 0.0f;
                            directions[i].vector = new Vector3(0, 0, 0);
                        }
                    }
                }
            }
        }

        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    { }
    }
}