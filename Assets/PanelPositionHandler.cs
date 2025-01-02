using UnityEngine;

public class PanelPositionHandler : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    public RectTransform panelRect;
    
    private void OnEnable()
    {
        AdjustPanelPosition();
    }

    private void AdjustPanelPosition()
    {
        if (Camera.main != null)
        {
            Vector3 viewportPoint = Camera.main.WorldToViewportPoint(transform.position);
        
            if (viewportPoint.x < 0.2f)
            {
                panelRect.localPosition = new Vector3(75f, 0f, 0f);
            }
            else if (viewportPoint.x > 0.8f)
            {
                panelRect.localPosition = new Vector3(-75f, 0f, 0f);
            }
            else
            {
                panelRect.localPosition = Vector3.zero;
            }
        }
    }
}