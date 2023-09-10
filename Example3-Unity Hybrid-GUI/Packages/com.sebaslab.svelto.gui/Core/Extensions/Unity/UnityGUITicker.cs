using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Svelto.ECS.GUI.Extensions.Unity
{
    public class UnityGUITicker
    {
        public UnityGUITicker(SveltoGUI gui)
        {
            _ticker = new GameObject("UnityGUITicker").AddComponent<UnityTicker>();
            _ticker.gui = gui;
        }

        public void Start()
        {
            _ticker.gui.Init();
        }

        public void Pause()
        {
            _ticker.enabled = false;
        }

        public void Resume()
        {
            _ticker.enabled = true;
        }

        public void Dispose()
        {
            if (_ticker != null)
                Object.Destroy(_ticker.gameObject);

            GC.SuppressFinalize(this);
        }

        ~UnityGUITicker()
        {
            Svelto.Console.LogWarning("UnityGUI ticker has been garbage collected. This may cause problems");
            Dispose();
        }

        readonly UnityTicker _ticker;

        class UnityTicker : MonoBehaviour
        {
            public SveltoGUI gui;

            void Update()
            {
                gui.Tick();
            }
        }
    }
}