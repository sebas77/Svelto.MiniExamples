using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Debugger.DebugStructure
{
    public class DebugTree
    {
        public List<DebugRoot> DebugRoots = new List<DebugRoot>();
        
        public delegate void UpdateHandler();
        public event UpdateHandler OnUpdate;
        
        public DebugRoot AddRootToTree(EnginesRoot root)
        {
            var debugRoot = new DebugRoot(root);
            DebugRoots.Add(debugRoot);
            return debugRoot;
        }
        
        public void RemoveRootFromTree(EnginesRoot root)
        {
            DebugRoots.RemoveAll(debug => debug.EnginesRoot == root);
        }

        public void Update()
        {
            foreach (var debugRoot in DebugRoots)
            {
                debugRoot.Process();
            }
            OnUpdate?.Invoke();
        }

        public void Clear()
        {
            DebugRoots.Clear();
        }
    }

    public class DebugRoot
    {
        #region Static

        private static FieldInfo EnginesField;
        private static FieldInfo EntityDBField;
        
        static DebugRoot()
        {
            var typeFields = typeof(EnginesRoot).GetAllFields().ToList();
            EnginesField = typeFields.First(f => f.Name == "_enginesSet");
            EntityDBField = typeFields.First(f => f.Name == "_groupEntityViewsDB");
        }

        #endregion

        public EnginesRoot EnginesRoot; 
        public FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> Root;
        public FasterList<IEngine> Engines;
        public List<DebugGroup> DebugGroups = new List<DebugGroup>();

        public DebugRoot(EnginesRoot root)
        {
            EnginesRoot = root;
            Engines = (FasterList<IEngine>) EnginesField.GetValue(root);
            Root = (FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>>) EntityDBField.GetValue(root);
            Process();
        }

        public void Process()
        {
            DebugGroups.Clear();
            foreach (var keyval in Root)
            {
                    var key = keyval.Key;
                    var val = keyval.Value;
                    DebugGroups.Add(new DebugGroup(key, val, this));
            }
        }
    }

    public class DebugGroup
    {
        public DebugRoot Parent;
        public uint Id;
        private FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> GroupDB; 
        
        public List<DebugEntity> DebugEntities = new List<DebugEntity>();

        public DebugGroup(uint key, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> val, DebugRoot debugRoot)
        {
            Id = key;
            GroupDB = val;
            Parent = debugRoot;
            Process();
        }

        public void Process()
        {
            foreach (var entityStructs in GroupDB)
            {
                var type = entityStructs.Key;
                var valTypeSafe = entityStructs.Value;
                            
                var fields = valTypeSafe.GetType().GetAllFields();
                var valuesField = fields.First(s => s.Name == "_values");
                var valuesInfoField = fields.First(s => s.Name == "_valuesInfo");
                var values = (Array) valuesField.GetValue(valTypeSafe);
                var keys = (Array) valuesInfoField.GetValue(valTypeSafe);
                var count = valTypeSafe.Count;
                var nodeKeyField = valuesInfoField.FieldType.GetElementType().GetAllFields().First(f => f.Name == "key");
                            
                for (int i = 0; i < count; i++)
                {
                    var key = (uint)nodeKeyField.GetValue(keys.GetValue(i));
                    var entity = GetOrAddEntity(key);
                    entity.AddStruct(values.GetValue(i));
                }
            }
        }

        private DebugEntity GetOrAddEntity(uint key)
        {
            var entity = DebugEntities.Find(f => f.Id == key);
            if (entity == null)
            {
                entity = new DebugEntity(key);
                DebugEntities.Add(entity);
            }
            return entity;
        }
    }

    public class DebugEntity
    {
        public uint Id;
        
        public List<DebugStruct> DebugStructs = new List<DebugStruct>();

        public DebugEntity(uint key)
        {
            Id = key;
        }

        public void AddStruct(object value)
        {
            DebugStructs.Add(new DebugStruct(value));
        }
    }

    public class DebugStruct
    {
        public object Value;

        public DebugStruct(object value)
        {
            Value = value;
        }
    }
}