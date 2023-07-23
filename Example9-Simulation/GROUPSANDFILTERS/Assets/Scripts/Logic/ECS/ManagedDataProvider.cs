// Copyright (c) Sean Nowotny

using UnityEngine;

namespace Logic.ECS
{
    public class ManagedDataProvider: MonoBehaviour
    {
        public GameObject Prefab;
        public Material[] Materials;
        public static ManagedDataProvider Instance;

        private void Awake()
        {
            Instance = this;
        }
    }
}