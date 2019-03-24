using Svelto.ECS.Example.Survive.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace Svelto.ECS.Example.Survive.Implementors.HUD
{
    public class HealthSliderImplementor : MonoBehaviour, IHealthSliderComponent, IImplementor
    {
        public Slider healthSlider { get; private set; }

        void Awake()
        {
            healthSlider = GetComponent<Slider>();
        }

        public int value
        {
            set { healthSlider.value = value; }
        }
    }
}
