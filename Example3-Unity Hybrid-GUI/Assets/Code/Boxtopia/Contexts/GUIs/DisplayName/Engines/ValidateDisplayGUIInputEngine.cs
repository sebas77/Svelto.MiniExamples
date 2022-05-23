using System.Collections;
using System.Collections.Generic;
using Boxtopia.GUIs.Generic;
using Boxtopia.GUIs.InputField;
using Boxtopia.GUIs.LocalisedText;
using Svelto.ECS;
using Svelto.Tasks;
using Svelto.Tasks.Enumerators;
using Svelto.Tasks.Lean;
using Svelto.Tasks.ExtraLean;
using User;
using User.Services.Authentication;
using ServiceLayer;
using Svelto.ServiceLayer;

namespace Boxtopia.GUIs.DisplayName
{
    public class ValidateDisplayGUIInputEngine : IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public ValidateDisplayGUIInputEngine(IServiceRequestsFactory serviceFactory,
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
            while (entitiesDB.Exists<UserEntityComponent>(UniqueEGID.UserToValidate) == false)
                yield return Yield.It;

            using (var consumer =
                _buttonEntityConsumer.GenerateConsumer<ButtonEntityComponent>(ExclusiveGroups.DisplayName,
                    "ValidateDisplayGUIInputEngine", 1))
            {
                while (true)
                {
                    while (consumer.TryDequeue(out var button))
                    {
                        //User Is Now Validated
                        if (button.message == ButtonEvents.OK)
                        {
                            _onScreenOpen = false;

                            entitiesDB.QueryEntity<UserEntityComponent>(UniqueEGID.UserToValidate).name
                                .Set(_validatedString);

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

            var inputField = entitiesDB.QueryUniqueEntity<InputFieldEntityViewComponent>(ExclusiveGroups.DisplayName)
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
                    //disable the submit button until verified
                    entitiesDB.QueryUniqueEntity<ButtonEntityViewComponent>(ExclusiveGroups.DisplayName).buttonState
                            .interactive = false;
                    
                    _currentString = inputFieldText;

                    var checkNameValidity =
                        unifiedAuthVerifyDisplayNameService.Inject(_currentString).Execute();
                    yield return checkNameValidity.Continue();
                    
                    ///Should all the possible errors be handled properly?
                    entitiesDB.QueryUniqueEntity<LocalizedLabelEntityViewComponent>(ExclusiveGroups.FeedbackLabel).label
                            .text = unifiedAuthVerifyDisplayNameService.result == WebRequestResult.Success
                            ? OnSuccess(unifiedAuthVerifyDisplayNameService.response)
                            : OnFailure();
                }
                
                yield return Yield.It;

                //can the entity change? Actually it can't, but these kind of reasoning should be standard
                inputFieldText = entitiesDB.QueryUniqueEntity<InputFieldEntityViewComponent>(ExclusiveGroups.DisplayName)
                    .inputField.text;
            }
        }

        string OnFailure()
        {
            return LocalizationService.Localize(GameStringsID.strSomethingWentWrong);
        }

        string OnSuccess(VerifyDisplayNameResponse response)
        {
            if (response.valid == true)
            {
                entitiesDB.QueryUniqueEntity<ButtonEntityViewComponent>(ExclusiveGroups.DisplayName).buttonState
                    .interactive = true;

                _validatedString = _currentString;

                return LocalizationService.Localize(GameStringsID.strValidDisplayName);
            }

            entitiesDB.QueryUniqueEntity<ButtonEntityViewComponent>(ExclusiveGroups.DisplayName).buttonState
                .interactive = false;

            return LocalizationService.Localize(GameStringsID.strInvalidDisplayName);
        }

        IEnumerator YieldUntilDisplayGUIIsEnabled()
        {
            while (entitiesDB.HasAny<GUIEntityViewComponent>(ExclusiveGroups.DisplayName) == false ||
                   entitiesDB.QueryUniqueEntity<GUIEntityViewComponent>(ExclusiveGroups.DisplayName).guiRoot.enabled ==
                   false)
                yield return Yield.It;
        }

        string                                _currentString;
        readonly IServiceRequestsFactory      _serviceFactory;
        readonly IEntityStreamConsumerFactory _buttonEntityConsumer;
        bool                                  _onScreenOpen;
        readonly IEntityFunctions             _entitiesFunction;
        string                                _validatedString;
    }
}