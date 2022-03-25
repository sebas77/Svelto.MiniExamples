using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using System;
using System.Linq;
using Buffer = Stride.Graphics.Buffer;

namespace StrideTransformationInstancing
{
    public class InstanceUpdaterUserArray : InstanceUpdaterBase
    {
        protected override int InstanceCountSqrt => 20;

        InstancingUserArray instancingUserArray;

        protected override IInstancing GetInstancingType()
        {
            instancingUserArray = new InstancingUserArray();
            return instancingUserArray;
        }

        protected override void ManageInstancingData()
        {
            var transformUsage = (ModelTransformUsage)(((int)Game.UpdateTime.Total.TotalSeconds) % 3);
            instancingUserArray.ModelTransformUsage = ModelTransformUsage.PostMultiply;
            instancingUserArray.UpdateWorldMatrices(instanceWorldTransformations);
        }
    }
}
