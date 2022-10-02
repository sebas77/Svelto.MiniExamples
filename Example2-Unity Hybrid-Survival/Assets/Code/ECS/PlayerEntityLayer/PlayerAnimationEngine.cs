namespace Svelto.ECS.Example.Survive.Player
{
    public class PlayerAnimationEngine: IQueryingEntitiesEngine, IStepEngine, IReactOnRemoveEx<PlayerEntityComponent>
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        { }

        public void Step()
        {
            //Remembering the syntax to query groups may be hard at first. You get used of it over the time, but 
            //Rider and VS can help you with this work. Follow this trick:
            //first use a random variable name in the foreach like asd
            // foreach (var asd in entitiesDB.QueryEntities<PlayerInputDataComponent, PlayerEntityViewComponent>(Player.Groups))
            //then go over asd and ask Rider or VS to deconstruct the variable,  now it should look like
            //foreach (var ((buffer1, buffer2, count), exclusiveGroupStruct) in entitiesDB.QueryEntities<PlayerInputDataComponent, PlayerEntityViewComponent>(Player.Groups))
            foreach (var ((animation, playersInput, count), _) in entitiesDB
               .QueryEntities<AnimationComponent, PlayerInputDataComponent>(Player.Groups))
            {
                for (var i = 0; i < count; i++)
                {
                    var input = playersInput[i].input;

                    // Create a boolean that is true if either of the input axes is non-zero.
                    var walking = input.x != 0f || input.z != 0f;

                    // Tell the animator whether or not the player is walking (if it's not walking is idling)
                    animation[i].animationState = new AnimationState(PlayerAnimations.IsWalking, walking);
                }
            }
        }
        
        public string name => nameof(PlayerAnimationEngine);
 
        public void Remove((uint start, uint end) rangeOfEntities,
            in EntityCollection<PlayerEntityComponent> collection, ExclusiveGroupStruct groupID)
        {
            var (playerEntity, _) = collection;
            var (animations, _) = entitiesDB.QueryEntities<AnimationComponent>(groupID);
            
            for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                //Disable the RB to avoid further gameobject collisions while the player entity doesn't exist
                playerEntity[i].isKinematic   = true;
                animations[i].animationState = new AnimationState(PlayerAnimations.Die);
            }
        }
    }
}

