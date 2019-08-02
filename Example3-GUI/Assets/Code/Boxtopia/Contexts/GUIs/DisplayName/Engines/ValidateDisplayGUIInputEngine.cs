using System.Collections;
using System.Collections.Generic;
using Boxtopia.GUIs.Generic;
using Boxtopia.GUIs.InputField;
using Boxtopia.GUIs.LocalisedText;
using Svelto.ECS;
using Svelto.ECS.Experimental;
using Svelto.Tasks;
using Svelto.Tasks.Enumerators;
using Svelto.Tasks.Lean;
using Svelto.Tasks.ExtraLean;
using User;
using User.Services.Authentication;
using ServiceLayer;
using Svelto.ServiceLayer.Experimental.Unity;

namespace Boxtopia.GUIs.DisplayName
{
    public class ValidateDisplayGUIInputEngine : IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }

        public ValidateDisplayGUIInputEngine(IUserServicesFactory serviceFactory,
            IEntityStreamConsumerFactory buttonEntityConsumer, IEntityFunctions entitiesFunction)
        {
            _serviceFactory = serviceFactory;
            _buttonEntityConsumer = buttonEntityConsumer;
            _entitiesFunction = entitiesFunction;
        }

        public void Ready()
        {
            CheckOKClicked().RunOn(ExtraLean.BoxtopiaSchedulers.UIScheduler);
            CheckNameValidity().RunOn(Lean.BoxtopiaSchedulers.UIScheduler);
        }

        /// <summary>
        /// User To Validate is the first state, so this starts immediately,
        /// also I would like to not need to flush the consumer for nothing
        /// </summary>
        /// <returns></returns>
        IEnumerator CheckOKClicked()
        {
            while (entitiesDB.Exists<UserEntityStruct>(UniqueEGID.UserToValidate) == false)
                yield return Yield.It;
            
            using (var consumer =
                _buttonEntityConsumer.GenerateConsumer<ButtonEntityStruct>(ExclusiveGroups.DisplayName,
                    "ValidateDisplayGUIInputEngine", 1))
            {
                while (true)
                {
                    while (consumer.TryDequeue(out var button))
                    {
                        //User Is Now Validated
                        if (button.message == ButtonEvents.OK)
                        {
                            //if CheckNameValidity was a task routine, I could have stopped it here
                            _onScreenOpen = false;

                            entitiesDB.QueryEntity<UserEntityStruct>(UniqueEGID.UserToValidate).name
                                .Set(_currentString);

                            _entitiesFunction.SwapEntityGroup<UserEntityDescriptor>(
                                UniqueEGID.UserToValidate, UniqueEGID.UserToRegister);

                            yield break;
                        }
                    }

                    yield return Yield.It;
                }
            }
        }

        /// <summary>
        /// Should this start only when the Display Name Window is on?
        /// </summary>
        /// <returns></returns>
        IEnumerator<TaskContract> CheckNameValidity()
        {
            //this probably could be simplified as probably with task routines I could stop this task
            //when the display view is not on
            var yieldUntilDisplayGuiIsEnabled = YieldUntilDisplayGUIIsEnabled();
            yield return yieldUntilDisplayGuiIsEnabled.Continue();

            var inputField = entitiesDB.QueryUniqueEntity<InputFieldEntityViewStruct>(ExclusiveGroups.DisplayName)
                .inputField;
            var inputFieldText = inputField.text;
            inputField.limit = 24;

            _currentString = inputFieldText;
            var wait = new WaitForSecondsEnumerator(1);
            var unifiedAuthVerifyDisplayNameService = _serviceFactory.Create<IUnifiedAuthVerifyDisplayNameService>();
            _onScreenOpen = true;

            while (_onScreenOpen == true)
            {
                if (_currentString != inputFieldText)
                {
                    _currentString = inputFieldText;

                    yield return unifiedAuthVerifyDisplayNameService.Inject(_currentString).Execute().Continue();

                    ///Should all the possible errors be handled properly?
                    entitiesDB.QueryUniqueEntity<LocalizedLabelEntityViewStruct>(ExclusiveGroups.FeedbackLabel).label
                            .text = unifiedAuthVerifyDisplayNameService.result == WebRequestResult.Success
                            ? OnSuccess(unifiedAuthVerifyDisplayNameService.response)
                            : OnFailure();

                    yield return wait.Continue();
                }

                yield return Yield.It;

                //can the entity change? Actually it can't, but these kind of reasoning should be standard
                inputFieldText = entitiesDB.QueryUniqueEntity<InputFieldEntityViewStruct>(ExclusiveGroups.DisplayName)
                    .inputField.text;
            }
        }

        string OnFailure()
        {
            entitiesDB.QueryUniqueEntity<ButtonEntityViewStruct>(ExclusiveGroups.DisplayName).buttonState.interactive =
                false;

            return LocalizationService.Localize(GameStringsID.strInvalidDisplayName);
        }

        string OnSuccess(VerifyDisplayNameResponse response)
        {
            if (response.Available == true)
            {
                //if I make this kind of assumptions (just one button present on the gui), I need to assert
                //somewhere else (maybe initialization) if there is more than one button on the gui to communicate
                //my intention. Otherwise let's always use the group bind.
                entitiesDB.QueryUniqueEntity<ButtonEntityViewStruct>(ExclusiveGroups.DisplayName).buttonState
                    .interactive = true;

                return LocalizationService.Localize(GameStringsID.strValidDisplayName);
            }

            //According Mike's logic, the service return success because the name has been validated, even if
            //it's actually in use.
            return LocalizationService.Localize(GameStringsID.strAlreadyDisplayName);
        }

        IEnumerator YieldUntilDisplayGUIIsEnabled()
        {
            while (entitiesDB.HasAny<GUIEntityViewStruct>(ExclusiveGroups.DisplayName) == false ||
                   entitiesDB.QueryUniqueEntity<GUIEntityViewStruct>(ExclusiveGroups.DisplayName).guiRoot.enabled ==
                   false)
                yield return Yield.It;
        }

        string                                _currentString;
        readonly IUserServicesFactory         _serviceFactory;
        readonly IEntityStreamConsumerFactory _buttonEntityConsumer;
        bool                                  _onScreenOpen;
        readonly IEntityFunctions             _entitiesFunction;
    }
}