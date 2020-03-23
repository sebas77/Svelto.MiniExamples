using System.Collections.Generic;
using System.Linq;
using Svelto.ECS.Debugger.DebugStructure;
using Svelto.ECS.Debugger.Editor.ListViews;
using Svelto.ECS.Debugger.Editor.EntityInspector;
using UnityEditor;
using UnityEngine;
using GroupListView = Svelto.ECS.Debugger.Editor.ListViews.GroupListView;

namespace Svelto.ECS.Debugger.Editor
{
    public class SveltoECSEntityDebugger : EditorWindow
    {
        const float kSystemListWidth = 150f;

        float CurrentEntityViewWidth =>
            Mathf.Max(100f, position.width - kSystemListWidth);

        [MenuItem("Window/Analysis/Svelto.ECS Debugger", false)]
        static void OpenWindow()
        {
            GetWindow<SveltoECSEntityDebugger>("Svelto.ECS Entity Debugger");
        }

        static GUIStyle LabelStyle
        {
            get
            {
                return labelStyle ?? (labelStyle = new GUIStyle(EditorStyles.label)
                {
                    margin = EditorStyles.boldLabel.margin,
                    richText = true
                });
            }
        }

        static GUIStyle labelStyle;

        static GUIStyle BoxStyle
        {
            get
            {
                return boxStyle ?? (boxStyle = new GUIStyle(GUI.skin.box)
                {
                    margin = new RectOffset(),
                    padding = new RectOffset(1, 0, 1, 0),
                    overflow = new RectOffset(0, 1, 0, 1)
                });
            }
        }

        static GUIStyle boxStyle;
        static SveltoECSEntityDebugger Instance { get; set; }

        const float kLineHeight = 18f;
        readonly RepaintLimiter repaintLimiter = new RepaintLimiter();

        EntitySelectionProxy selectionProxy;
        public DebugTree Data;
        
        public GroupListView groupListView;
        EntityListView entityListView;
        public WorldPopup m_WorldPopup;

        public DebugRoot RootSelection { get; set; }
        public uint? EntitySelectionId;
        public uint? GroupSelectionId { get; set; }
        
        public DebugGroup GetSelectionGroup() =>
            RootSelection.DebugGroups.FirstOrDefault(f => f.Id == GroupSelectionId);
        public DebugEntity GetSelectionEntity() =>
            RootSelection.DebugGroups.FirstOrDefault(f => f.DebugEntities.Any(a => a.Id == EntitySelectionId))?.DebugEntities.FirstOrDefault(f => f.Id == EntitySelectionId);


        public void SetGroupSelection(uint? manager)
        {
            GroupSelectionId = manager;
            entityListView.SetSelection(new List<int>());
            ReloadAll();
        }
        
        public void SetEntitySelection(uint? entityId)
        {
            EntitySelectionId = entityId;
            var entity = GetSelectionEntity();
            if (entity == null)
            {
                Selection.activeObject = null;
            }
            else
            {
                selectionProxy.SetEntity(GetSelectionEntity);
                Selection.activeObject = selectionProxy;
            }
            ReloadAll();
        }

        public void SetRootSelection(DebugRoot selection)
        {
            if (RootSelection != selection)
            {
                RootSelection = selection;
                groupListView.SetSelection(new List<int>());
                ReloadAll();
            }
        }

        void CreateEntityListView()
        {
            entityListView = EntityListView.CreateGroupListView(() => GetSelectionGroup(), m => SetEntitySelection(m));
        }

        void CreateGroupListView()
        {
            groupListView = GroupListView.CreateGroupListView(() => RootSelection, (m) => SetGroupSelection(m));
            groupListView.multiColumnHeader.ResizeToFit();
        }

        void CreateWorldPopup()
        {
            m_WorldPopup = new WorldPopup(
                () => RootSelection,
                x => SetRootSelection(x),
                () => Data?.DebugRoots
                );
        }

        void OnEnable()
        {
            Instance = this;
            UpdateAll();
            UpdateData();
            Debugger.OnAddEnginesRoot += OnAddEnginesRoot;
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

        void OnDisable()
        {
            if (Instance == this)
                Instance = null;
            if (selectionProxy)
                DestroyImmediate(selectionProxy);

            Debugger.OnAddEnginesRoot -= OnAddEnginesRoot;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
        }

        void OnPlayModeStateChange(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
                ClearAll();
            if (change == PlayModeStateChange.ExitingPlayMode && Selection.activeObject == selectionProxy)
                Selection.activeObject = null;
        }

        void OnAddEnginesRoot(DebugRoot root)
        {
            UpdateData();
            if (RootSelection == null || Data?.DebugRoots.All(r => r != RootSelection) == true)
            {
               SetRootSelection(root);
            }
        }

        void CreateEntitySelectionProxy()
        {
            selectionProxy = ScriptableObject.CreateInstance<EntitySelectionProxy>();
            selectionProxy.hideFlags = HideFlags.HideAndDontSave;
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                UpdateData();
                Repaint();
            }
            else
            {
                Repaint();
            }
        }

        void UpdateData()
        {
            if (Application.isPlaying)
            {
                if (Data == null)
                {
                    Data = Debugger.Instance.DebugInfo;
                    if (Data.DebugRoots.Count > 0)
                        RootSelection = Data.DebugRoots[0];
                    UpdateAll();
                    Data.OnUpdate += ReloadAll;
                }
            }
            else
            {
                Data = null;
                ClearAll();
            }
        }

        void ClearAll()
        {
            Data?.Clear();
            UpdateAll();
            ReloadAll();
        }

        void UpdateAll()
        {
            CreateEntitySelectionProxy();
            CreateWorldPopup();
            CreateGroupListView();
            CreateEntityListView();
        }

        void ReloadAll()
        {
            groupListView.Reload();
            entityListView.Reload();
        }
        
        //OnGUI

        void ShowWorldPopup()
        {
            if (Application.isPlaying)
                m_WorldPopup.OnGui();
        }

        void GroupList()
        {
            var rect = GUIHelpers.GetExpandingRect();
            groupListView.OnGUI(rect);
        }

        void GroupsHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Groups", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            ShowWorldPopup();
            GUILayout.EndHorizontal();
        }

        void EntityHeader()
        {
            if (RootSelection != null || RootSelection != null)
            {
                var rect = new Rect(kSystemListWidth, 3f, CurrentEntityViewWidth, kLineHeight);
                if (GroupSelectionId == null)
                {
                    GUI.Label(rect, "All Entities", EditorStyles.boldLabel);
                }
                else
                {
                    GUI.Label(rect, Debugger.GetNameGroup(GroupSelectionId.Value), LabelStyle);
                }
            }
        }

        void EntityList()
        {
            GUILayout.BeginVertical(BoxStyle);
            entityListView.OnGUI(GUIHelpers.GetExpandingRect());
            GUILayout.EndVertical();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(0f, 0f, kSystemListWidth, position.height));
            GroupsHeader();

            GUILayout.BeginVertical(BoxStyle);
            GroupList();
            GUILayout.EndVertical();

            GUILayout.EndArea();

            EntityHeader();

            GUILayout.BeginArea(new Rect(kSystemListWidth, kLineHeight, CurrentEntityViewWidth, position.height - kLineHeight));
            EntityList();
            GUILayout.EndArea();

            repaintLimiter.RecordRepaint();
        }
    }
}
