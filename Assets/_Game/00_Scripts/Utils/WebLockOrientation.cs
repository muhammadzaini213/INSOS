using UnityEngine;

namespace Slafurry.Utils
{
    public class WebLockOrientation : MonoBehaviour
    {
        private bool _locked;

        private void Update()
        {
            if (_locked)
                return;

            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                _locked = true;
                WebGLPlatform.RequestFullscreen();
                Screen.orientation = ScreenOrientation.LandscapeLeft;
            }
        }
    }
}
