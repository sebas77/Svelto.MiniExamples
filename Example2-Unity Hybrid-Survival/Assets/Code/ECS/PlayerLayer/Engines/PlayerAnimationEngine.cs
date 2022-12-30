using Svelto.ECS.Example.Survive.OOPLayer;


namespace Svelto.ECS.Example.Survive.Player
{
    public class PlayerAnimationEngine: IQueryingEntitiesEngine, IStepEngine, IReactOnRemoveEx<PlayerInputDataComponent>
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void Step()
        {
            //Remembering the syntax to query groups may be hard at first. You get used of it over the time, but 
            //Rider and VS can help you with this work. Follow this trick:
            //first use a random variable name in the foreach like asd
            //foreach (var asd in entitiesDB.QueryEntities<PlayerInputDataComponent, PlayerEntityComponent>(Player.Groups))
            //then go over asd and ask Rider or VS to deconstruct the variable,  now it should look like
            //foreach (var ((buffer1, buffer2, count), exclusiveGroupStruct) in entitiesDB.QueryEntities<PlayerInputDataComponent, PlayerEntityComponent>(Player.Groups))
            foreach (var ((animation, playersInput, count), _) in entitiesDB
               .QueryEntities<AnimationComponent, PlayerInputDataComponent>(PlayerAliveGroup.Groups))
            {
                for (var i = 0; i < count; i++)
                {
                    var input = playersInput[i].input;

                    // Create a boolean that is true if either of the input axes is non-zero.
                    var walking = input.x != 0f || input.z != 0f;

                    // Tell the animator whether or not the player is walking (if it's not walking is idling)
                    var animationState = new AnimationState(PlayerAnimations.IsWalking, walking);

                    if (!animation[i].animationState.Equals(animationState))
                        animation[i].animationState = animationState;
                }
            }
        }
        
        public string name => nameof(PlayerAnimationEngine);
 
        /// <summary>
        /// This call back is called when players are removed. rangeOfEntities are the id if the players removed
        /// the same id can be used to index other player components
        /// </summary>
        /// <param name="rangeOfEntities"></param>
        /// <param name="collection"></param>
        /// <param name="groupID"></param>
        public void Remove((uint start, uint end) rangeOfEntities,
            in EntityCollection<PlayerInputDataComponent> collection, ExclusiveGroupStruct groupID)
        {
            var (animations, rbs, _) = entitiesDB.QueryEntities<AnimationComponent, RigidBodyComponent>(groupID);
            
            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                rbs[i].isKinematic   = true; //Disable the RB to avoid further gameobject collisions while the player entity doesn't exist
                
                animations[i].animationState = new AnimationState((int)PlayerAnimations.Die);
            }
        }
    }
}

