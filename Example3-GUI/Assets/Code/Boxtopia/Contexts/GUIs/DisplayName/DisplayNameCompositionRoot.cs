using System;
using Boxtopia.GUIs.Generic;
using Boxtopia.GUIs.InputField;
using Boxtopia.GUIs.LocalisedText;
using Svelto.Context;
using Svelto.ECS;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Schedulers.Unity;
using User;
using User.Services.Authentication;

namespace Boxtopia.GUIs.DisplayName
{
    public class DisplayNameCompositionRoot : ICompositionRoot
    {
        public void OnContextInitialized<T>(T contextHolder)
        {
            _enginesRoot = new EnginesRoot(new UnityEntitiesSubmissionScheduler("DisplayNameCompositionRoot"));
            var userServicesFactory = new UserServicesFactoryMockup();

            var generateEntityFactory = _enginesRoot.GenerateEntityFactory();
            var generateEntityFunctions = _enginesRoot.GenerateEntityFunctions();
            var entityStreamConsumerFactory = _enginesRoot.GenerateConsumerFactory();

            BuildActualGUIEntities(contextHolder, generateEntityFactory);

            generateEntityFactory.BuildEntity<UserEntityDescriptor>(UniqueEGID.UserToValidate);
            
            var validateDisplayGuiInputEngine =
                new ValidateDisplayGUIInputEngine(userServicesFactory, entityStreamConsumerFactory,
                    generateEntityFunctions);

            _enginesRoot.AddEngine(validateDisplayGuiInputEngine);
            _enginesRoot.AddEngine(new GenericGUIInteraction(entityStreamConsumerFactory));
            _enginesRoot.AddEngine(new LocalizingTextEngine());
            _enginesRoot.AddEngine(new ButtonClickingEventEngine());
            _enginesRoot.AddEngine(new ClosingGUIEngine(entityStreamConsumerFactory));
        }

        static void BuildActualGUIEntities<T>(T contextHolder, IEntityFactory generateEntityFactory)
        {
            //create the main GUI widget and relative entity
            SveltoGUIHelper.Create<DisplayNameDescriptorHolder>(
                new EGID(0, ExclusiveGroups.DisplayName), (contextHolder as UnityContext).transform,
                generateEntityFactory, out var holder);

            //extract all the entities from its nested widgets
            var index = SveltoGUIHelper.CreateAll<ButtonEntityDescriptorHolder>(1, 
                ExclusiveGroups.DisplayName, holder.transform, generateEntityFactory);

            index = SveltoGUIHelper.CreateAll<LocalizedTextDescriptorHolder>(index, 
                ExclusiveGroups.DisplayName, holder.transform, generateEntityFactory);

            index = SveltoGUIHelper.CreateAll<InputFieldDescriptorHolder>(index, 
                ExclusiveGroups.DisplayName, holder.transform, generateEntityFactory);

            SveltoGUIHelper.CreateAll<DisplayNameFeedbackLabelDescriptorHolder>(index, 
                ExclusiveGroups.DisplayName, holder.transform, generateEntityFactory);
        }

        public void OnContextDestroyed()
        {
            BoxtopiaSchedulers.StopAllCoroutines();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            _enginesRoot.Dispose();
        }

        public void OnContextCreated<T>(T contextHolder) { }

        EnginesRoot _enginesRoot;
    }
}