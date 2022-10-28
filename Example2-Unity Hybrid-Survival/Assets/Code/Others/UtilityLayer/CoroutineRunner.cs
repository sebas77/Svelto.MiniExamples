using System;
using System.Collections;
using UnityEngine;

namespace Svelto.ECS.Example.Survive
{
    public static class CoroutineRunner
    {
        static CoroutineRunner()
        {
            var gameObject = new GameObject("CoroutineRunner");
            _coroutineRunnerMB = gameObject.AddComponent<CoroutineRunnerMB>();
        }

        public static void Run(IEnumerator coroutine)
        {
            _coroutineRunnerMB.StartCoroutine(coroutine);
        }

        public static void RunEveryFrame(Action step)
        {
            Run(EveryFrame());

            IEnumerator EveryFrame()
            {
                while (Application.isPlaying)
                {
                    step();

                    yield return null;
                }                
            }
        }

        static readonly CoroutineRunnerMB _coroutineRunnerMB;
    }
}