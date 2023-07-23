// Copyright (c) Sean Nowotny

using UnityEngine;

namespace Logic.BurstedAosDOD
{
    public class BurstedAosDodGameHandler : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Material[] materials;

        private Data data;

        private void Start()
        {
            data = new Data(true);
            RenderSystem.Initialize(prefab, materials);
        }

        private void Update()
        {
            StaticGameHandler.BurstedUpdate(Time.deltaTime, ref data);

            if (Input.GetKeyUp(KeyCode.Space))
            {
                data.EnableRendering = !data.EnableRendering;

                if (data.EnableRendering)
                {
                    RenderSystem.Initialize(prefab, materials);
                }
                else
                {
                    RenderSystem.Clear();
                }
            }

            if (data.EnableRendering)
            {
                RenderSystem.Run(ref data);
            }
        }

        private void OnDestroy()
        {
            data.Dispose();
        }
    }
}