namespace Svelto.ECS.Components
{
    public struct ECSVector2
    {
        public float x, y;

        public ECSVector2(float X, float Y)
        {
            x = X;
            y = Y;
        }
    }
    
    public struct ECSVector3
    {
        public float x, y, z;

        public ECSVector3(float X, float Y, float Z)
        {
            x = X;
            y = Y;
            z = Z;
        }
    }
    
    public struct ECSVector4
    {
        public float x, y, z, w;

        public ECSVector4(float X, float Y, float Z, float W)
        {
            x = X;
            y = Y;
            z = Z;
            w = W;
        }
    }
}