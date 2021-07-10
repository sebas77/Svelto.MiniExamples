using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;

namespace Svelto.ECS.MiniExamples.Turrets
{
    /// <summary>
    /// When input is involved, creating an InputEngine is part of the Svelto patterns. The Input engine has
    /// the sole responsibility to translate system input into input components. The data held by the input components
    /// can be re-contextualised according their purpose. The input components are then used by other engines to
    /// handle the single entities logic.
    /// </summary>
    class PlayerBotInputEngine : SyncScript, IQueryingEntitiesEngine
    {
        public override void Update()
        {
            if (Input.HasKeyboard)
            {
                foreach (var ((speeds, directions, _), _) in entitiesDB
                   .QueryEntities<SpeedComponent, DirectionComponent>(PlayerBotTag.Groups))
                {
                    speeds[0].value = 0.0f;

                    bool directionChanged = false;
                    
                    if (Input.IsKeyDown(Keys.W))
                    {
                        speeds[0].value      =  1.0f;
                        directions[0].vector += new Vector3(-1.0f, 0.0f, 0.0f);
                        directionChanged     =  true;
                    }
                    else
                    if (Input.IsKeyDown(Keys.S))
                    {
                        speeds[0].value      =  1.0f;
                        directions[0].vector += new Vector3(1.0f, 0.0f, 0.0f);
                        directionChanged     =  true;
                    }
                    
                    if (Input.IsKeyDown(Keys.A))
                    {
                        speeds[0].value      =  1.0f;
                        directions[0].vector += new Vector3(0.0f, 0.0f, 1.0f);
                        directionChanged     =  true;
                    }
                    else
                    if (Input.IsKeyDown(Keys.D))
                    {
                        speeds[0].value      =  1.0f;
                        directions[0].vector += new Vector3(0.0f, 0.0f, -1.0f);
                        directionChanged     =  true;
                    }
                    
                    if (directionChanged)
                        directions[0].vector.Normalize();
                }
            }
        }

        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    { }
    }
}