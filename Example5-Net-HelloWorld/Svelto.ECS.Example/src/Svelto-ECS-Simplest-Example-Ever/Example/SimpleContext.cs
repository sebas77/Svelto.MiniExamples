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
    ///     The Context is the application starting point.
    ///     As a Composition root, it gives to the coder the responsibility to create, initialize and
    ///     inject dependencies.
    ///     Every application can have more than one context and every context can have one
    ///     or more composition roots (a facade, but even a factory, can be a composition root)
    /// </summary>
    public class SimpleContext
    {
        public SimpleContext()
        {
            //an entity submission scheduler is needed to submit entities to the Svelto database, Svelto is not 
            //responsible to decide when to submit entities, it's the user's responsibility to do so.
            var entitySubmissionScheduler = new SimpleEntitiesSubmissionScheduler();
            //An EnginesRoot holds all the engines and entities created. it needs a EntitySubmissionScheduler to know when to
            //add previously built entities to the Svelto database. Using the SimpleEntitiesSubmissionScheduler
            //is expected as it gives complete control to the user about when the submission happens
            _enginesRoot = new EnginesRoot(entitySubmissionScheduler);

            //an entity factory allows to build entities inside engines
            var entityFactory = _enginesRoot.GenerateEntityFactory();
            //the entity functions allows other operations on entities, like remove and swap
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();

            //Add the Engine to manage the SimpleEntities
            var behaviourForEntityClassEngine = new BehaviourForEntityClassEngine(entityFunctions);
            _enginesRoot.AddEngine(behaviourForEntityClassEngine);

            //build Entity with ID 0 in group0
            entityFactory.BuildEntity<SimpleEntityDescriptor>(new EGID(0, ExclusiveGroups.group0));

            //submit the previously built entities to the Svelto database
            entitySubmissionScheduler.SubmitEntities();

            //as Svelto doesn't provide an engine/system ticking system, it's the user's responsibility to
            //update engines  
            behaviourForEntityClassEngine.Update();

            Console.Log("Done - click any button to quit");

            System.Console.ReadKey();

            Environment.Exit(0);
        }

        readonly EnginesRoot _enginesRoot;
    }

    //An EntityComponent must always implement the IEntityComponent interface
    //don't worry, boxing/unboxing will never happen.
    public struct EntityComponent : IEntityComponent
    {
        public int counter;
    }

    /// <summary>
    ///     The EntityDescriptor identifies your Entity. It's essential to identify
    ///     your entities with a name that comes from the Game Design domain.
    /// </summary>
    class SimpleEntityDescriptor : GenericEntityDescriptor<EntityComponent> { }

    namespace SimpleEntityEngine
    {
        public class BehaviourForEntityClassEngine :
                //this interface makes the engine reactive, it's absolutely optional, you need to read my articles
                //and wiki about it.
                IReactOnAddEx<EntityComponent>, IReactOnSwapEx<EntityComponent>, IReactOnRemoveEx<EntityComponent>,
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

            public void Update()
            {
                //Simple query to get all the entities with EntityComponent in group1
                var (components, entityIDs, count) = entitiesDB.QueryEntities<EntityComponent>(ExclusiveGroups.group1);

                uint entityID;
                for (var i = 0; i < count; i++)
                {
                    components[i].counter++;
                    entityID = entityIDs[i];
                }

                Console.Log("Entity Struct engine executed");
            }

            //the following methods are called by Svelto.ECS when an entity is added to a group
            public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<EntityComponent> entities
              , ExclusiveGroupStruct groupID)
            {
                var (_, entityIDs, _) = entities;

                for (uint index = rangeOfEntities.start; index < rangeOfEntities.end; index++)
                    //Swap entities between groups is a very common operation and it's necessary to
                    //move entities between groups/sets. A Group represent a state/adjective of an entity, so changing
                    //group means change state/behaviour as different engines will process different groups.
                    //it's the Svelto equivalent of adding/remove components to an entity at run time
                    _entityFunctions.SwapEntityGroup<SimpleEntityDescriptor>(new EGID(entityIDs[index], groupID), ExclusiveGroups.group1);
            }

            //the following methods are called by Svelto.ECS when an entity is swapped from a group to another
            public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<EntityComponent> entities
              , ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
            {
                var (_, entityIDs, _) = entities;
                
                for (var index = rangeOfEntities.start; index < rangeOfEntities.end; index++)
                {
                    Console.Log($"entity {entityIDs[index]} moved from {fromGroup} to {toGroup}");
                    //like for the swap operation, removing entities from the Svelto database is a very common operation.
                    //For both operations is necessary to specify the EntityDescriptor to use. This has also a philosophical
                    //reason to happen, it's to always remind which entity type we are operating with. 
                    _entityFunctions.RemoveEntity<SimpleEntityDescriptor>(new EGID(entityIDs[index], toGroup));
                }
            }

            //the following methods are called by Svelto.ECS when an entity is removed from a group
            public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<EntityComponent> entities, ExclusiveGroupStruct groupID)
            {
                var (_, entityIDs, _) = entities;

                for (uint index = rangeOfEntities.start; index < rangeOfEntities.end; index++)
                    Console.Log($"entity {entityIDs[index]} removed from {groupID.ToString()}");
            }
        }
    }
}