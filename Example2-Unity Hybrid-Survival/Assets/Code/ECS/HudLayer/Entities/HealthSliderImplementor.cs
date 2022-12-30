using Svelto.ECS.Hybrid;
using UnityEngine;
using UnityEngine.UI;

namespace Svelto.ECS.Example.Survive.HUD
{
    public class HealthSliderImplementor : MonoBehaviour, IHealthSliderComponent, IImplementor
    {
        public Slider healthSlider { get; private set; }

        public int value { set => healthSlider.value = value; }

        void Awake() { healthSlider = GetComponent<Slider>(); }
    }
}