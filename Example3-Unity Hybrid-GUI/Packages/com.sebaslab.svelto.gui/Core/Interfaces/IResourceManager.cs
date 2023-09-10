using System;
using Svelto.ECS.GUI.Resources;

namespace Svelto.ECS.GUI
{
    internal interface IResourceManager : IDisposable
    {
        void Release(EcsResource resource);
    }
}