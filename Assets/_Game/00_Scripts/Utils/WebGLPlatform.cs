using System.Runtime.InteropServices;
using UnityEngine;

namespace Slafurry.Utils
{
    public static class WebGLPlatform
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void RequestFullscreenWebGL();
#else
        private static void RequestFullscreenWebGL() { }
#endif

        public static void RequestFullscreen()
        {
            RequestFullscreenWebGL();
        }
    }
}
