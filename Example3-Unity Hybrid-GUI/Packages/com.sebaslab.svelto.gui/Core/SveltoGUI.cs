using System;
using Svelto.ECS.GUI.Commands;
using Svelto.ECS.GUI.Core.Builders;
using Svelto.ECS.GUI.Resources;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS.GUI
{
    public partial class SveltoGUI
    {
        /// <summary>
        /// injecting the lcoialization Service is temporary, we need to find a generic way to register services
        /// at that point GUIParseVariableService will get the service through the get service interface
        /// </summary>
        // TODO: The EnginesRoot and Scheduler are going to be created inside the framework in the future.
        public SveltoGUI(EnginesRoot enginesRoot, IGUIBuilder builder
            , IGUILocalizationService localizationService, SimpleEntitiesSubmissionScheduler scheduler = null)
        {
            _enginesRoot     = enginesRoot;
            _entityFactory   = enginesRoot.GenerateEntityFactory();
            _entityFunctions = enginesRoot.GenerateEntityFunctions();
            _scheduler       = scheduler;

            // Create data structures.
            _resources       = new GUIResources();
            _widgetsMap      = new GUIWidgetMap(_resources);
            _widgetBuilders  = new GUIWidgetBuildersMap();
            _blackboard      = new GUIBlackboard(_resources);
            _commandsManager = new CommandsManager(_blackboard, _resources, _widgetsMap);
            _variableParser  = new GUIParseVariableService(_blackboard, localizationService);

            // Register builder.
            builder.buildersMap       = _widgetBuilders;
            builder.resources         = _resources;
            builder.widgetMap         = _widgetsMap;
            builder.blackboard        = _blackboard;
            builder.BuildWidgetEntity = BuildDynamicWidget;
            _builder                  = builder;

            // Init partial class methods
            InitCustomEvents();
            InitEngines();
        }

        public void Init()
        {
            StaticBuild();
        }

        // NOTE: This is temporary code. Engines will have a base class that gives them access to this method and the
        //       scheduler will always be a simple scheduler.
        public void ForceRefresh()
        {
            _scheduler?.SubmitEntities();
        }

        public GUIParseVariableService GetVariableParserService()
        {
            return _variableParser;
        }

        public IGUIBuilder GetBuilder()
        {
            if (_builder == null)
            {
                throw new NullReferenceException("GUI Builder is null");
            }

            return _builder;
        }

        public T GetBuilder<T>() where T : class, IGUIBuilder
        {
            T builder = _builder as T;
            if (builder == null)
            {
                throw new NullReferenceException($"Trying to cast GUI Builder {typeof(T)} returned null");
            }

            return builder;
        }

        public CommandsManager     GetCommandManager() => _commandsManager;
        public GUIResources        GetResources() => _resources;
        public IGUIWidgetMapReader GetWidgetMap() => _widgetsMap;


        public IEntityFunctions GetEntityFunctions() => _entityFunctions;
        public EntitiesSubmissionScheduler GetScheduler() => _enginesRoot.scheduler;

        public void RegisterWidget(Type widgetType, IWidgetBuilder builder)
        {
            _widgetBuilders.Register(widgetType, builder);
        }

        public void RegisterWidget<DescriptorHolder, Descriptor>(IEntityFactory factory, ExclusiveBuildGroup buildGroup,
            IWidgetInitializer initializer)
            where DescriptorHolder : IWidgetDescriptorHolder
            where Descriptor : GUIExtendibleEntityDescriptor, new()
        {
            _widgetBuilders.Register(typeof(DescriptorHolder),
                new GenericWidgetBuilder<Descriptor>(factory, buildGroup, initializer));
        }

        public void Dispose()
        {
            _builder.Dispose();
            _resources.Dispose();

            GC.SuppressFinalize(this);
        }

        ~SveltoGUI()
        {
            Console.LogWarning("SveltoGUI has been garbage collected, don't forget to call Dispose()!");

            Dispose();
        }

        readonly EnginesRoot      _enginesRoot;
        readonly IEntityFactory   _entityFactory;
        readonly IEntityFunctions _entityFunctions;
        readonly SimpleEntitiesSubmissionScheduler _scheduler;

        readonly CommandsManager         _commandsManager;
        readonly GUIResources            _resources;
        readonly GUIWidgetBuildersMap    _widgetBuilders;
        readonly GUIBlackboard           _blackboard;
        readonly GUIWidgetMap            _widgetsMap;
        readonly IGUIBuilder             _builder;
        readonly GUIParseVariableService _variableParser;
    }
}