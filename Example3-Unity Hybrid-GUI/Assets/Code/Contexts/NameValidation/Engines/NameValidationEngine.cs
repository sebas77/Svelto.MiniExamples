using System.Threading.Tasks;
using Boxtopia.GUIs.Generic;
using Boxtopia.GUIs.InputField;
using Boxtopia.GUIs.LocalisedText;
using Svelto.ECS;
using User;
using User.Services.Authentication;
using ServiceLayer;
using Svelto.ServiceLayer;

namespace Boxtopia.GUIs.NameValidation
{
    public class NameValidationEngine : IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public NameValidationEngine(IServiceRequestsFactory serviceFactory,
            IEntityStreamConsumerFactory buttonEntityConsumer, IEntityFunctions entitiesFunction)
        {
            _serviceFactory = serviceFactory;
            _buttonEntityConsumer = buttonEntityConsumer;
            _entitiesFunction = entitiesFunction;
        }

        public void Ready()
        {
            CheckOKClicked();
            CheckNameValidity();
        }

        /// <summary>
        /// User To Validate is the first state, so this starts immediately,
        /// also I would like to not need to flush the consumer for nothing
        /// </summary>
        async void CheckOKClicked()
        {
            while (entitiesDB != null && entitiesDB.Exists<UserEntityComponent>(UniqueEGID.UserToValidate) == false)
                await Task.Yield();

            if (entitiesDB == null)
                return;

            using (var consumer =
                _buttonEntityConsumer.GenerateConsumer<ButtonEntityComponent>(ExclusiveGroups.NameValidation,
                    "NameValidationEngine"))
            {
                while (entitiesDB != null)
                {
                    while (consumer.TryDequeue(out var button))
                    {
                        //User Is Now Validated
                        if (button.message == ButtonEvents.OK && string.IsNullOrWhiteSpace(_validatedString) == false)
                        {
                            _onScreenOpen = false;

                            entitiesDB.QueryEntity<UserEntityComponent>(UniqueEGID.UserToValidate).name
                                .Set(_validatedString);

                            _entitiesFunction.SwapEntityGroup<UserEntityDescriptor>(
                                UniqueEGID.UserToValidate, UniqueEGID.UserToRegister);

                            return;
                        }
                    }

                    await Task.Yield();
                }
            }
        }

        /// <summary>
        /// Should this start only when the Display Name Window is on?
        /// </summary>
        async void CheckNameValidity()
        {
            //wait until the display gui is enabled
            while (entitiesDB == null || entitiesDB.HasAny<GUIEntityViewComponent>(ExclusiveGroups.NameValidation) == false ||
                   entitiesDB.QueryUniqueEntity<GUIEntityViewComponent>(ExclusiveGroups.NameValidation).guiRoot.enabled ==
                   false)
                await Task.Yield();

            if (entitiesDB == null)
                return;

            var inputField = entitiesDB.QueryUniqueEntity<InputFieldEntityViewComponent>(ExclusiveGroups.NameValidation)
                .inputField;
            var inputFieldText = inputField.text;
            inputField.limit = 24;

            entitiesDB.QueryUniqueEntity<ButtonEntityViewComponent>(ExclusiveGroups.NameValidation).buttonState
                .interactive = false;
            entitiesDB.QueryUniqueEntity<LocalizedLabelEntityViewComponent>(ExclusiveGroups.FeedbackLabel).label.text =
                string.Empty;

            _currentString = inputFieldText;
            _hasNameBeenEdited = false;
            var nameValidationService = _serviceFactory.Create<INameValidationService>();
            _onScreenOpen = true;

            while (_onScreenOpen == true && entitiesDB != null)
            {
                if (_currentString != inputFieldText)
                {
                    _hasNameBeenEdited = true;

                    //disable the submit button until verified
                     entitiesDB.QueryUniqueEntity<ButtonEntityViewComponent>(ExclusiveGroups.NameValidation).buttonState
                             .interactive = false;

                    _validatedString = null;
                    _currentString = inputFieldText;

                    await nameValidationService.Inject(_currentString).Execute();

                    if (_hasNameBeenEdited)
                        entitiesDB.QueryUniqueEntity<LocalizedLabelEntityViewComponent>(ExclusiveGroups.FeedbackLabel).label
                                .text = nameValidationService.result == WebRequestResult.Success
                                ? OnSuccess(nameValidationService.response)
                                : OnFailure();
                }

                await Task.Yield();

                if (_onScreenOpen == false || entitiesDB == null ||
                    entitiesDB.HasAny<InputFieldEntityViewComponent>(ExclusiveGroups.NameValidation) == false)
                    return;

                //can the entity change? Actually it can't, but these kind of reasoning should be standard
                inputFieldText = entitiesDB.QueryUniqueEntity<InputFieldEntityViewComponent>(ExclusiveGroups.NameValidation)
                    .inputField.text;
            }
        }

        string OnFailure()
        {
            return LocalizationService.Localize(GameStringsID.strSomethingWentWrong);
        }

        string OnSuccess(VerifyDisplayNameResponse response)
        {
            if (response.status == NameValidationStatus.NameRequired)
                return LocalizationService.Localize(GameStringsID.strNameRequired);

            if (response.valid == true)
            {
                entitiesDB.QueryUniqueEntity<ButtonEntityViewComponent>(ExclusiveGroups.NameValidation).buttonState
                    .interactive = true;

                _validatedString = _currentString;

                return LocalizationService.Localize(GameStringsID.strValidDisplayName);
            }

            entitiesDB.QueryUniqueEntity<ButtonEntityViewComponent>(ExclusiveGroups.NameValidation).buttonState
                .interactive = false;

            return LocalizationService.Localize(GameStringsID.strInvalidDisplayName);
        }

        string                                _currentString;
        bool                                  _hasNameBeenEdited;
        readonly IServiceRequestsFactory      _serviceFactory;
        readonly IEntityStreamConsumerFactory _buttonEntityConsumer;
        bool                                  _onScreenOpen;
        readonly IEntityFunctions             _entitiesFunction;
        string                                _validatedString;
    }
}
