using Svelto.ECS.Debugger.DebugStructure;
using Svelto.ECS.Debugger.Editor.ListViews;
using UnityEditor;
using UnityEngine;

namespace Svelto.ECS.Debugger.Editor.EntityInspector
{
    public class EntitySelectionProxy : ScriptableObject
    {
        public delegate void EntityControlDoubleClickHandler(DebugEntity entity);

        public event EntityControlDoubleClickHandler EntityControlDoubleClick;

        //public EntityContainer Container { get; private set; }
        public EntitySelectionGetter Entity { get; private set; }
        [SerializeField] int entityIndex;
        //[SerializeField] public List<object> EntityStructs;
        public bool Exists => true;

        public void OnEntityControlDoubleClick(DebugEntity entity)
        {
            EntityControlDoubleClick(entity);
        }

        public void SetEntity(EntitySelectionGetter getter)
        {
            this.Entity = getter;
            //EntityStructs = () => entity.DebugStructs.Select(s => s.Value).ToList());
            EditorUtility.SetDirty(this);
        }
    }
}