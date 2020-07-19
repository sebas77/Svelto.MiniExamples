using Svelto.ECS;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Boxtopia.GUIs.Generic
{
    public enum ButtonEvents
    {
        UNDEFINED,
        OK,
        CANCEL,
        SELECT,
        ZOOM_IN,
        ZOOM_OUT,
        QUIT,
        WANNAQUIT
    }

    public class ButtonImplementor : MonoBehaviour, IButtonClick, IUIState, IImplementor
    {
        public ButtonEvents message;
        public DispatchOnSet<ButtonEvents> buttonEvent { get; set; }
        public ButtonEvents action { set => message = value; }

        void Awake()
        {
            _button = GetComponent<UnityEngine.UI.Button>();

            _button.onClick.AddListener(OnButtonClicked);
        }

        public bool interactive
        {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        void OnButtonClicked()
        {
            if (buttonEvent != null)
                buttonEvent.value = message;
        }

        UnityEngine.UI.Button _button;
    }
}