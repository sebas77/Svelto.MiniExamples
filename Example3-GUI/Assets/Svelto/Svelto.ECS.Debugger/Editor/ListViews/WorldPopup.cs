using UnityEditor;
using UnityEngine;

namespace Svelto.ECS.Debugger.Editor.ListViews
{
    public class WorldPopup
    {
        GenericMenu Menu
        {
            get
            {
                var currentSelection = _getWorldSelection();
                var menu = new GenericMenu();
                var list = _rootListGetter();
                if (list?.Count > 0)
                {
                    foreach (var root in list)
                    {
                        menu.AddItem(new GUIContent(Debugger.GetNameRoot(root)), currentSelection == root, () => _setWorldSelection(root));
                    }
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("No Worlds"));
                }
                
                return menu;
            }
        }

        readonly RootSelectionGetter _getWorldSelection;
        readonly RootSelectionSetter _setWorldSelection;
        readonly RootListGetter _rootListGetter;

        public WorldPopup(RootSelectionGetter getWorld, RootSelectionSetter setWorld, RootListGetter rootListGetter)
        {
            _getWorldSelection = getWorld;
            _setWorldSelection = setWorld;
            _rootListGetter = rootListGetter;
        }
        
        public void OnGui()
        {
            //TryRestorePreviousSelection(lastSelectedWorldName);

            var worldName = Debugger.GetNameRoot(_getWorldSelection());
            if (EditorGUILayout.DropdownButton(new GUIContent(worldName), FocusType.Passive))
            {
                Menu.ShowAsContext();
            }
        }

//        internal void TryRestorePreviousSelection(bool showingPlayerLoop, string lastSelectedWorldName)
//        {
//            if (!showingPlayerLoop && ScriptBehaviourUpdateOrder.CurrentPlayerLoop.subSystemList != null)
//            {
//                if (lastSelectedWorldName == KNoWorldName)
//                {
//                    if (World.AllWorlds.Count > 0)
//                        _setWorldSelection(World.AllWorlds[0]);
//                }
//                else
//                {
//                    var namedWorld = World.AllWorlds.FirstOrDefault(x => x.Name == lastSelectedWorldName);
//                    if (namedWorld != null)
//                        _setWorldSelection(namedWorld);
//                }
//            }
//        }
    }
}
