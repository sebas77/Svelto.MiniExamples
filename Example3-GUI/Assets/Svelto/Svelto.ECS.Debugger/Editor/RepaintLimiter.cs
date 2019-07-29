using UnityEditor;
using UnityEngine;

namespace Svelto.ECS.Debugger.Editor
{
    internal class RepaintLimiter
    {
        float lastUpdate;
        int lastFrame;
        public const float defaultUpdateFrequency = 0.2f;
        readonly float playingRepaintFrequency;

        public RepaintLimiter(float frequency = defaultUpdateFrequency)
        {
            playingRepaintFrequency = frequency;
        }

        public bool SimulationAdvanced()
        {
            if (EditorApplication.isPlaying) 
            {
                var playUpdate = !EditorApplication.isPaused && Time.unscaledTime > lastUpdate + playingRepaintFrequency;
                var stepUpdate = EditorApplication.isPaused && Time.frameCount != lastFrame;
                return playUpdate || stepUpdate;
            }

            return false;
        }

        public void RecordRepaint()
        {
            lastUpdate = Time.unscaledTime;
            lastFrame = Time.frameCount;
        }
    }
}