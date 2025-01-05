using UnityEngine;

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

        /*
         when pawn has two moves to play and player needs to select the moves,
         player needs to tap the pawn to enable move select panel,
         the panel goes out of screen bounds when the pawn is at left most or right most side of the screen,
         the function handles the panel position when it moves out of screen bounds and brings the panel inside screen bounds
         */
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
