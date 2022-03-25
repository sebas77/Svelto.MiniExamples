using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Animations;
using Stride.Input;
using Stride.Engine;

namespace StrideTransformationInstancing
{
    [DataContract("PlayAnimation")]
    public class PlayAnimation
    {
        public AnimationClip Clip;
        public AnimationBlendOperation BlendOperation = AnimationBlendOperation.LinearBlend;
        public double StartTime = 0;
    }

    /// <summary>
    /// Script which starts a few animations on its entity
    /// </summary>
    public class AstroBoyAnimation : SyncScript
    {
        /// <summary>
        /// A list of animations to be loaded when the script starts
        /// </summary>
        public readonly List<PlayAnimation> Animations = new List<PlayAnimation>();
        protected InstancingUserArray instancingMany;
        protected Matrix[] instanceWorldTransformations;
        int InstanceCount = 20;

        public override void Start()
        {
            ((Game)Game).WindowMinimumUpdateRate.SetMaxFrequency(60);

            var animComponent = Entity.GetOrCreate<AnimationComponent>();

            if (animComponent != null)
                PlayAnimations(animComponent);

            instancingMany = new InstancingUserArray() { ModelTransformUsage = ModelTransformUsage.PostMultiply };
            if (Entity.Get<InstancingComponent>() == null)
            {
                var instancingComponent = Entity.GetOrCreate<InstancingComponent>();
                instancingComponent.Type = instancingMany; 
            }
            instanceWorldTransformations = new Matrix[InstanceCount];
        }

        public override void Update()
        {
            // generate some matrices
            var offset = InstanceCount / 2;
            var seconds = (float)Game.UpdateTime.Total.TotalSeconds;
            for (int i = 0; i < InstanceCount; i++)
            {
                var x = i * 1 - offset;
                var y = 0;
                var z = (float)Math.Cos(new Vector2(x, y).Length() * 0.5f + seconds);

                instanceWorldTransformations[i] = Matrix.RotationY(seconds) * Matrix.Translation(x * 0.1f, y, z * 0.1f);
            }

            instancingMany.UpdateWorldMatrices(instanceWorldTransformations);
        }

        private void PlayAnimations(AnimationComponent animComponent)
        {
            foreach (var anim in Animations)
            {
                if (anim.Clip != null)
                    animComponent.Add(anim.Clip, anim.StartTime, anim.BlendOperation);
            }

            Animations.Clear();
        }
    }
}
