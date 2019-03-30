using Svelto.ECS.Example.Survive.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace Svelto.ECS.Example.Survive.Implementors.HUD
{
    public class DamageHUDImplementor : MonoBehaviour, IImplementor, IDamageHUDComponent
    {
        public float flashSpeed = 5f;                               // The speed the damageImage will fade at.
        public Color flashColour = new Color(1f, 0f, 0f, 0.1f);     // The colour the damageImage is set to, to flash.

        public float speed { get { return flashSpeed; } }
        public Color flashColor { get { return flashColour; } }

        public Color imageColor
        {
            get { return _image.color; }
            set { _image.color = value; }
        }

        void Awake()
        {
            _image = GetComponent<Image>();
        }

        Image _image;
    }
}
