using UnityEngine;

public class SwipeControl : MonoBehaviour
{
    public float swipeThreshold = 50f;
    public float moveSpeed = 5f; 

    private Vector2 startTouchPos;
    private Vector2 endTouchPos;
    private Vector2 swipeDelta;

    private Rigidbody2D rb;
    private TrailRenderer trailRenderer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.emitting = false;
    }

    private void Update()
    {
        CheckForSwipe();
    }

    private void CheckForSwipe()
    {
        swipeDelta = Vector2.zero;                //reset swipedelta

        // Check for user input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);  //first touch

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPos = touch.position;

                trailRenderer.emitting = true;
            }

            else if (touch.phase == TouchPhase.Ended)
            {
                endTouchPos = touch.position;

                swipeDelta = endTouchPos - startTouchPos;

                if (swipeDelta.magnitude > swipeThreshold)
                {
                    MovePlayer(swipeDelta.normalized);
                }

                trailRenderer.emitting = false;
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void MovePlayer(Vector2 direction)
    {
        rb.velocity = new Vector2(direction.x * moveSpeed, direction.y * moveSpeed);

        Debug.Log(rb.velocity);
    }
}
