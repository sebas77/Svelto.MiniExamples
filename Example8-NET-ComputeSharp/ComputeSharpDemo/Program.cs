﻿// See https://aka.ms/new-console-template for more information

using System.Numerics;
using System.Runtime.InteropServices;
using ComputeSharp;
using Svelto.ECS;
using Svelto.ECS.Schedulers;

var simpleSubmissionEntityViewScheduler = new SimpleEntitiesSubmissionScheduler();
//Build Svelto Entities and Engines container, called EnginesRoot
var _enginesRoot = new EnginesRoot(simpleSubmissionEntityViewScheduler);

var entityFactory   = _enginesRoot.GenerateEntityFactory();

//Add an Engine to the enginesRoot to manage the SimpleEntities
var behaviourForEntityClassEngine = new TestComputeShaderEngine();
_enginesRoot.AddEngine(behaviourForEntityClassEngine);

entityFactory.PreallocateEntitySpace<ComputeSharpEntityDescriptor>(ExclusiveGroups.group0, 100);

for (uint i = 0; i < 100; i++)
{
    var initializer = entityFactory.BuildEntity<ComputeSharpEntityDescriptor>(new EGID(i, ExclusiveGroups.group0));
    
    initializer.Init(new PositionComponent()
    {
        position = new Vector3(1.0f, 1.0f, 1.0f)
    });
}

//submit the previously built entities to the Svelto database
simpleSubmissionEntityViewScheduler.SubmitEntities();

//Step the engine execute the shader sync
behaviourForEntityClassEngine.Step();

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class ComputeSharpEntityDescriptor: IEntityDescriptor
{
    public IComponentBuilder[] componentsToBuild { get; } =
    {
        new ComputeComponentBuilder<PositionComponent>()
    };
}

public struct PositionComponent: IEntityComputeSharpComponent
{
    public Vector3 position;
}

public static class ExclusiveGroups
{
    public static ExclusiveGroup group0 = new ExclusiveGroup();
}

public class TestComputeShaderEngine: IQueryingEntitiesEngine, IStepEngine
{
    public void Ready()
    {
    }
    
    public EntitiesDB entitiesDB { get; set; }
    
    public void Step()
    {
       (ComputeSharpBuffer<PositionComponent> buffer, int count) = entitiesDB.QueryEntities<PositionComponent>(ExclusiveGroups.group0);
        // Run the shader
       GraphicsDevice.GetDefault().For(count, new MultiplyByTwo(buffer.ToComputeBuffer()));

       buffer.ReadBack();
       
       (buffer, count) = entitiesDB.QueryEntities<PositionComponent>(ExclusiveGroups.group0);
       {
           for (int i = 0; i < count; i++)
           {
               Console.WriteLine(buffer[i].position);
           }
       }
    }

    public string name { get; }
}

[AutoConstructor] 
readonly partial struct MultiplyByTwo : IComputeShader
{
    readonly ReadWriteBuffer<PositionComponent> buffer;

    /// <inheritdoc/>
    public void Execute()
    {
        this.buffer[ThreadIds.X].position *= ThreadIds.X;
    }
}