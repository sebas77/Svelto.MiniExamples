using Svelto.ECS.Example.Survive.HUD;
using Svelto.ECS.Hybrid;
using UnityEngine;
using UnityEngine.UI;

namespace Svelto.ECS.Example.Survive.Implementors.HUD
{
    public class DamageHUDImplementor : MonoBehaviour, IImplementor, IDamageHUDComponent
    {
        public Color flashColour = new Color(1f, 0f, 0f, 0.1f); // The colour the damageImage is set to, to flash.
        public float flashSpeed  = 5f;                          // The speed the damageImage will fade at.

        public float speed      => flashSpeed;
        public Color flashColor => flashColour;

        public Color imageColor { get => _image.color; set => _image.color = value; }

        void  Awake() { _image = GetComponent<Image>(); }
        
        Image _image;
    }
}