using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Boxtopia.GUIs
{
    public class GUIRootImplementor : MonoBehaviour, IGUIRoot, IImplementor
    {
        public Transform[] views;

        void Awake()
        {
            if (views.Length > 0)
                view = 0;
        }
        
        public new bool enabled
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        public int view
        {
            get => _currentView;
            set
            {
                DBC.Check.Require(value >= 0 && value < views.Length, "index out of range");
                
                for (int i = 0; i < views.Length; i++)
                    views[i].gameObject.SetActive(false);
                
                views[value].gameObject.SetActive(true);

                _currentView = value;
            }
        }

        int _currentView;
    }
} 