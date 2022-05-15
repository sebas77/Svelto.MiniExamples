using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    public static class MouseUtilityClass
    {
        public static bool ScreenPositionToWorldPositionRaycast(Vector2 screenPos, CameraComponent camera,
            Simulation simulation, out HitResult result)
        {
            Matrix invViewProj = Matrix.Invert(camera.ViewProjectionMatrix);

            Vector3 sPos;
            sPos.X = screenPos.X * 2f - 1f;
            sPos.Y = 1f - screenPos.Y * 2f;

            sPos.Z = 0f;
            Vector4 vectorNear = Vector3.Transform(sPos, invViewProj);
            vectorNear /= vectorNear.W;

            sPos.Z = 1f;
            Vector4 vectorFar = Vector3.Transform(sPos, invViewProj);
            vectorFar /= vectorFar.W;

            result = simulation.Raycast(vectorNear.XYZ(), vectorFar.XYZ());
            return result.Succeeded;
        }
    }
}