using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private bool isMoving = false;
    [SerializeField] Vector2 moveSpeed;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        if(isMoving)
            rb.velocity = movementInput * moveSpeed;
        else
            rb.velocity = Vector2.zero;
    }

    private void OnMove(InputValue inputValue)
    {
        movementInput = inputValue.Get<Vector2>();
        isMoving = movementInput != Vector2.zero;
    }

    private Vector2 swipeStartPos;

    private void Update()
    {
        //for mouse swipe
        if (Input.GetMouseButtonDown(0))
        {
            SwipeStart(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            SwipeEnd(Input.mousePosition);
        }

        //for touch to move
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
        
            if(touch.phase == UnityEngine.TouchPhase.Began)
            {
                SwipeStart(touch.position);
            }
            else if(touch.phase == UnityEngine.TouchPhase.Ended)
            {
                SwipeEnd(touch.position);
            }
        }
    }

    private void MovePlayer(Vector2 direction)
    {
        isMoving = false;
        movementInput = direction;
        isMoving = true;
    }

    private void SwipeStart(Vector2 startPos)
    {
        movementInput = Vector2.zero;
        isMoving = false;
        swipeStartPos = startPos;
    }

    private void SwipeEnd(Vector2 endPos)
    {
        Vector2 swipeDelta = endPos - swipeStartPos;
        MovePlayer(swipeDelta.normalized);
    }
}
