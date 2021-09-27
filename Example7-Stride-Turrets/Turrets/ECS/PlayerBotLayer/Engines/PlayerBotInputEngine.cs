using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using Svelto.Common.Internal;

namespace Svelto.ECS.MiniExamples.Turrets
{
    /// <summary>
    /// When input is involved, creating an InputEngine is part of the Svelto patterns. The Input engine has
    /// the sole responsibility to translate system input into input components. The data held by the input components
    /// can be re-contextualised according their purpose. The input components are then used by other engines to
    /// handle the single entities logic.
    /// </summary>
    class PlayerBotInputEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public PlayerBotInputEngine(InputManager input)
        {
            _input = input;
        }

        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    { }

        public string name => this.TypeName();
        
        public void Step(in float deltaTime)
        {
            if (_input.HasKeyboard)
            {
                foreach (var ((speeds, directions, _), _) in entitiesDB
                   .QueryEntities<SpeedComponent, DirectionComponent>(PlayerBotTag.Groups))
                {
                    speeds[0].value = 0.0f;

                    bool directionChanged = false;

                    if (_input.IsKeyDown(Keys.W))
                    {
                        speeds[0].value      =  1.0f;
                        directions[0].vector += new Vector3(-1.0f, 0.0f, 0.0f);
                        directionChanged     =  true;
                    }
                    else
                        if (_input.IsKeyDown(Keys.S))
                        {
                            speeds[0].value      =  1.0f;
                            directions[0].vector += new Vector3(1.0f, 0.0f, 0.0f);
                            directionChanged     =  true;
                        }

                    if (_input.IsKeyDown(Keys.A))
                    {
                        speeds[0].value      =  1.0f;
                        directions[0].vector += new Vector3(0.0f, 0.0f, 1.0f);
                        directionChanged     =  true;
                    }
                    else
                        if (_input.IsKeyDown(Keys.D))
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

        readonly InputManager _input;
    }
}