using Svelto.ECS.Example.Survive.HUD;
using Svelto.ECS.Hybrid;
using UnityEngine;
using UnityEngine.UI;

namespace Svelto.ECS.Example.Survive.Implementors.HUD
{
    public class AmmoManagerImplementor : MonoBehaviour, IImplementor, IAmmoComponent
    {
        public int ammo 
        {
            get => _ammo;
            set
            {
                _ammo = value;
                _text.text = "ammo: " + _ammo.ToString();
            }
        }

        void Awake()
        {
            // Set up the reference.
            _text = GetComponent<Text>();

            // Reset the score.
            _ammo = 0;
        }

        int  _ammo;
        Text _text;
    }
}