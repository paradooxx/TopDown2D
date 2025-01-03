using UnityEngine;

namespace _Scripts.UI
{
    public class ScreenSizeManager : MonoBehaviour
    {
        [SerializeField] private float ReferenceOrthographicSize = 12.5f;   // orthographic camera size for iphone 13 pro max used in editor
        [SerializeField] private float ReferenceAspect = 1284f / 2778f;    // screen aspect for iphone 13 pro max
        [SerializeField] private float MinimumOrthographicSize = 10.5f;    // minimum allowed camera size
        public Camera MainCamera;

        private void OnEnable()
        {
            AdjustCameraSize();
        }

        private void AdjustCameraSize()
        {
            float currentAspect = (float)Screen.width / Screen.height;
            float ratio = ReferenceAspect / currentAspect;
            float newSize = ReferenceOrthographicSize * ratio;
            MainCamera.orthographicSize = Mathf.Max(newSize, MinimumOrthographicSize);
        }
    }
}