using UnityEngine;

namespace _Scripts.UI
{
    public class ScreenSizeManager : MonoBehaviour
    {
        [SerializeField] private float referenceOrthographicSize = 12.5f;   // orthographic camera size for iphone 13 pro max used in editor
        [SerializeField] private float referenceAspect = 1284f / 2778f;    // screen aspect for iphone 13 pro max
        [SerializeField] private float minimumOrthographicSize = 10.5f;    // minimum allowed camera size

        public Camera mainCamera;

        private void OnEnable()
        {
            AdjustCameraSize();
        }

        private void AdjustCameraSize()
        {
            float currentAspect = (float)Screen.width / Screen.height;
            float ratio = referenceAspect / currentAspect;
            float newSize = referenceOrthographicSize * ratio;
            mainCamera.orthographicSize = Mathf.Max(newSize, minimumOrthographicSize);
        }
    }
}