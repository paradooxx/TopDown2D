using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Player
{
    public class PawnPanelPositionHandler : MonoBehaviour
    {
        [SerializeField] private GameObject Panel;
        [SerializeField] private RectTransform PanelRect;
        [SerializeField] private float ChangeThreshold = 0.1f;
    
        private void OnEnable()
        {
            AdjustPanelPosition();
        }

        private void AdjustPanelPosition()
        {
            if (Camera.main != null)
            {
                Vector3 viewportPoint = Camera.main.WorldToViewportPoint(transform.position);
        
                if (viewportPoint.x < ChangeThreshold)
                {
                    PanelRect.localPosition = new Vector3(75f, 0f, 0f);
                }
                else if (viewportPoint.x > 1 - ChangeThreshold)
                {
                    PanelRect.localPosition = new Vector3(-75f, 0f, 0f);
                }
                else
                {
                    PanelRect.localPosition = Vector3.zero;
                }
            }
        }
    }
}
