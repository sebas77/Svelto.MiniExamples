using System;
using System.Threading;
using System.Threading.Tasks;
using Boxtopia.GUIs.Generic;
using Boxtopia.GUIs.InputField;
using Boxtopia.GUIs.LocalisedText;
using Svelto.Context;
using Svelto.ECS;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Schedulers;
using User;
using User.Services.Authentication;

///ATTENTION, this example is left just for historical reasons as a note of some interfaces that I need to decide if to delete or not
///There is no way I would use a solution similar to what exposed here to handle GUI in ECS

namespace Boxtopia.GUIs.NameValidation
{
    public class NameValidationCompositionRoot : ICompositionRoot
    {
        public void OnContextInitialized<T>(T contextHolder)
        {
            _enginesRoot = new EnginesRoot(new EntitiesSubmissionScheduler());
            var userServicesFactory = new MockUserServicesFactory();

            var generateEntityFactory = _enginesRoot.GenerateEntityFactory();
            var generateEntityFunctions = _enginesRoot.GenerateEntityFunctions();
            var entityStreamConsumerFactory = _enginesRoot.GenerateConsumerFactory();

            BuildActualGUIEntities(contextHolder, generateEntityFactory);

            generateEntityFactory.BuildEntity<UserEntityDescriptor>(UniqueEGID.UserToValidate);

            var nameValidationEngine =
                new NameValidationEngine(userServicesFactory, entityStreamConsumerFactory,
                    generateEntityFunctions);

            _enginesRoot.AddEngine(nameValidationEngine);
            _enginesRoot.AddEngine(new GenericGUIInteraction(entityStreamConsumerFactory));
            _enginesRoot.AddEngine(new LocalizingTextEngine());
            _enginesRoot.AddEngine(new ButtonClickingEventEngine());
            _enginesRoot.AddEngine(new ClosingGUIEngine(entityStreamConsumerFactory));

            //flush the built entities into the database
            _enginesRoot.scheduler.SubmitEntities();

            Tick();
        }

        async void Tick()
        {
            try
            {
                while (_cancellationTokenSource.IsCancellationRequested == false)
                {
                    _enginesRoot.scheduler.SubmitEntities();
                    await Task.Yield();
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
        }

        static void BuildActualGUIEntities<T>(T contextHolder, IEntityFactory generateEntityFactory)
        {
            //create the main GUI widget and relative entity
            SveltoGUIHelper.Create<NameValidationDescriptorHolder>(
                new EGID(0, ExclusiveGroups.NameValidation), (contextHolder as UnityContext).transform,
                generateEntityFactory, out var holder);

            //extract all the entities from its nested widgets
            var index = SveltoGUIHelper.CreateAll<ButtonEntityDescriptorHolder>(1, 
                ExclusiveGroups.NameValidation, holder.transform, generateEntityFactory);

            index = SveltoGUIHelper.CreateAll<LocalizedTextDescriptorHolder>(index, 
                ExclusiveGroups.NameValidation, holder.transform, generateEntityFactory);

            index = SveltoGUIHelper.CreateAll<InputFieldDescriptorHolder>(index, 
                ExclusiveGroups.NameValidation, holder.transform, generateEntityFactory);

            SveltoGUIHelper.CreateAll<NameValidationFeedbackLabelDescriptorHolder>(index, 
                ExclusiveGroups.NameValidation, holder.transform, generateEntityFactory);
        }

        public void OnContextDestroyed(bool isInit)
        {
            _cancellationTokenSource.Cancel();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            _enginesRoot.Dispose();
        }

        public void OnContextCreated<T>(T contextHolder) { }

        EnginesRoot _enginesRoot;
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    }
}
