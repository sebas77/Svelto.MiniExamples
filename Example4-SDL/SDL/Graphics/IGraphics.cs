using MiniExamples.DeterministicPhysicDemo.Graphics;

namespace MiniExamples.DeterministicPhysicDemo.Graphics
{
    public interface IGraphics
    {
        bool Init();
        void RenderStart();
        void RenderEnd();
        void Cleanup();

        void DrawText(Colour color, int x, int y, string text, int ptSize = 12);
        void DrawTextAbsolute(Colour color, int x, int y, string text, int ptSize = 12);
        void DrawCircle(Colour colour, int centreX, int centreY, int radius);
        void DrawCross(Colour colour, int centreX, int centreY, int radius);
        void DrawPlus(Colour colour, int centreX, int centreY, int radius);
        void DrawBox(Colour colour, int minX, int minY, int maxX, int maxY);
        void DrawLine(Colour colour, int fromX, int fromY, int toX, int toY);
    }
}