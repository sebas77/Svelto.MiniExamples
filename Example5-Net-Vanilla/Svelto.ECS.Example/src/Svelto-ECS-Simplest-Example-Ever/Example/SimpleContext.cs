using System;
using Svelto.ECS;
using Svelto.ECS.Schedulers;
using Svelto.ECS.Vanilla.Example.SimpleEntityEngine;

/// <summary>
///     This is the common pattern to declare Svelto Exclusive Groups (usually split by composition root)
/// </summary>
public static class ExclusiveGroups
{
    public static ExclusiveGroup group0 = new ExclusiveGroup();
    public static ExclusiveGroup group1 = new ExclusiveGroup();
}

namespace Svelto.ECS.Vanilla.Example
{
    /// <summary>
    ///     The Context is the framework starting point.
    ///     As Composition root, it gives to the coder the responsibility to create, initialize and inject dependencies.
    ///     Every application can have more than one context and every context can have one
    ///     or more composition roots (a facade, but even a factory, can be a composition root)
    /// </summary>
    public class SimpleContext
    {
        readonly EnginesRoot _enginesRoot;

        public SimpleContext()
        {
            //An EnginesRoot holds all the engines created. it needs a EntitySubmissionScheduler to know when to
            //add the EntityViews generated inside the EntityDB.
            var simpleSubmissionEntityViewScheduler = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(simpleSubmissionEntityViewScheduler);

            //an EnginesRoot must never be injected inside other classes only IEntityFactory and IEntityFunctions
            //implementation can
            var entityFactory   = _enginesRoot.GenerateEntityFactory();
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();

            //Add the Engine to manage the SimpleEntities
            var behaviourForEntityClassEngine = new BehaviourForEntityClassEngine(entityFunctions);
            _enginesRoot.AddEngine(behaviourForEntityClassEngine);

            //build Entity with ID 1 in group 0
            entityFactory.BuildEntity<SimpleEntityDescriptor>(new EGID(0, ExclusiveGroups.group0));

            //as we are using a basic scheduler, we need to schedule the entity submission ourselves
            simpleSubmissionEntityViewScheduler.SubmitEntities();

            //as we don't have any ticking system for this basic example, we tick explicitly 
            behaviourForEntityClassEngine.Update();

            Console.Log("Done - click any button to quit");

            System.Console.ReadKey();

            Environment.Exit(0);
        }
   }
    
        //An EntityComponent must always implement the IEntityComponent interface
    //don't worry, boxing/unboxing will never happen.
    public struct EntityComponent : IEntityComponent
    {
        public int  counter;
    }

    /// <summary>
    ///     The EntityDescriptor identifies your Entity. It's essential to identify
    ///     your entities with a name that comes from the Game Design domain.
    /// </summary>
    class SimpleEntityDescriptor : GenericEntityDescriptor<EntityComponent>
    {}

    namespace SimpleEntityEngine
    {
        public class BehaviourForEntityClassEngine :
                //this interface makes the engine reactive, it's absolutely optional, you need to read my articles
                //and wiki about it.
                IReactOnAddAndRemove<EntityComponent>,
                IReactOnSwap<EntityComponent>,
                //while this interface is optional too, it's almost always used as it gives access to the entitiesDB
                IQueryingEntitiesEngine
        {
            //extra entity functions
            readonly IEntityFunctions _entityFunctions;

            public BehaviourForEntityClassEngine(IEntityFunctions entityFunctions)
            {
                _entityFunctions = entityFunctions;
            }

            public EntitiesDB entitiesDB { get; set; }

            public void Ready() { }

            public void Add(ref EntityComponent entityView, EGID egid)
            {
                _entityFunctions.SwapEntityGroup<SimpleEntityDescriptor>(egid, ExclusiveGroups.group1);
            }

            public void Remove(ref EntityComponent entityView, EGID egid) { }
            
            public void MovedTo(ref EntityComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
            {
                Console.Log("Swap happened");
            }

            public void Update()
            {
                var (entityViews, count) = entitiesDB.QueryEntities<EntityComponent>(ExclusiveGroups.group1);

                if (count > 0)
                {
                    for (var i = 0; i < count; i++)
                        entityViews[i].counter++;

                    Console.Log("Entity Struct engine executed");
                }
                else
                {
                    throw new Exception("can't be");
                }
            }
        }
    }

}