using System;
using System.Collections.Generic;
using System.Linq;
using Svelto.ECS.Debugger.DebugStructure;
using UnityEngine;

namespace Svelto.ECS.Debugger
{
    public class Debugger : MonoBehaviour
    {
        private static object _lock = new object();
        private static Debugger _instance;
        public static Debugger Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Search for existing instance.
                        _instance = FindObjectOfType<Debugger>();
 
                        // Create new instance if one doesn't already exist.
                        if (_instance == null)
                        {
                            // Need to create a new GameObject to attach the singleton to.
                            var singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<Debugger>();
                            singletonObject.name = nameof(Debugger) + " (Singleton)";
 
                            // Make instance persistent.
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
 
                    return _instance;
                }
            }
        }
        private static Dictionary<uint, string> GroupDebugNames = new Dictionary<uint, string>();
        private static Dictionary<EnginesRoot, string> EnginesRootDebugNames = new Dictionary<EnginesRoot, string>();
        [NonSerialized]
        public DebugTree DebugInfo = new DebugTree();

        public delegate void OnAddRoot(DebugRoot debugRoot);

        public static event OnAddRoot OnAddEnginesRoot;
#if UNITY_EDITOR && SVELTO_DEBUGGER        
        private void Update()
        {
            DebugInfo.Update();
        }
#endif
        public void AddEnginesRoot(EnginesRoot root)
        {
            if (root is EnginesRootNamed named)
            {
                EnginesRootDebugNames[root] = named.Name;
            }
            var debugRoot = DebugInfo.AddRootToTree(root);
            OnAddEnginesRoot?.Invoke(debugRoot);
        }
        
        public void RemoveEnginesRoot(EnginesRoot root)
        {
            DebugInfo.RemoveRootFromTree(root);
        }

        public static void RegisterNameGroup(uint id, string name)
        {
            GroupDebugNames[id] = name;
        }
        public static string GetNameGroup(uint id)
        {
            if (!GroupDebugNames.ContainsKey(id))
                return $"Unknown group: {id}";
            return GroupDebugNames[id];
        }
        public static string GetNameRoot(DebugRoot root)
        {
            var engroot = Instance?.DebugInfo.DebugRoots.FirstOrDefault(r => root == r)?.EnginesRoot;
            if (engroot == null || !EnginesRootDebugNames.ContainsKey(engroot))
                return $"Unknown root";
            return EnginesRootDebugNames[engroot];
        }
    }
}