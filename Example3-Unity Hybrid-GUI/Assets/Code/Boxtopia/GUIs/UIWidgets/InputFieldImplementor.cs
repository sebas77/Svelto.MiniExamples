using System;
using System.Text.RegularExpressions;
using Boxtopia.GUIs.LocalisedText;
using ServiceLayer;
using Svelto.ECS.Hybrid;
using TMPro;
using UnityEngine;

namespace Boxtopia.GUIs.InputField
{
    public static class StandardValidator
    {
        static readonly Regex _regex = new Regex(@"^[A-Za-z0-9-_.]+$");
        
        public static char Validate(string s, int charIndex, char ch)
        {
            return _regex.IsMatch(ch.ToString()) ? ch : '\0';
        }
    }
    
    public class InputFieldImplementor:MonoBehaviour, IInputField, ILabelText, IImplementor
    {
        public string LocalisationKey;
        
        public GameStringsID textKey
        {
            get
            {
                if (_key == GameStringsID.NOT_INITIALIZED)
                    _key = Enum.TryParse(LocalisationKey, true, out GameStringsID result) == false ? GameStringsID.strTranslationNotFound : result;
                
                return _key;
            }
        }

        public string text
        {
            get
            {
                VerifyLabel();
                
                return _inputLabel.text;
            }
            set
            {
                VerifyLabel();

                _inputLabel.text = value;
            }
        }

        public int limit
        {
            get => _inputLabel.characterLimit;
            set => _inputLabel.characterLimit = value;
        }
    
        void VerifyLabel()
        {
            if (_inputLabel == null)
            {
                _inputLabel = GetComponent<TMP_InputField>();
                _inputLabel.onValidateInput = StandardValidator.Validate;
            }
        }

        TMP_InputField                  _inputLabel;
        GameStringsID _key;
    }
}