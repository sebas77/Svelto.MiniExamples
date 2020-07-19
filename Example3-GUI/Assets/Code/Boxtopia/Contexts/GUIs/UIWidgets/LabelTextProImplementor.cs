using System;
using ServiceLayer;
using Svelto.ECS.Hybrid;
using TMPro;
using UnityEngine;

namespace Boxtopia.GUIs.LocalisedText
{
    public class LabelTextProImplementor : MonoBehaviour, ILabelText, IImplementor
    {
        public string LocalisationKey;

        public GameStringsID textKey
        {
            get
            {
                if (_key == GameStringsID.NOT_INITIALIZED)
                    _key = Enum.TryParse(LocalisationKey, true, out GameStringsID result) == false
                        ? GameStringsID.strTranslationNotFound
                        : result;
                
                return _key;
            }
        }

        public string text
        {
            set
            {
                if (_reference == null)
                {
                    _reference = GetComponent<TextMeshProUGUI>();
                }

                _reference.text = value;
            }
        }

        TextMeshProUGUI _reference;
        GameStringsID   _key;
    }
}