using System.Collections;
using _Scripts.Board;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Player
{
    public class Pawn : MonoBehaviour
    {
        [Header("Pawn Sprite Components")] public SpriteRenderer PawnMainSprite;
        public GameObject PawnBaseSprite;

        public Node CurrentNode;
        public Player MainPlayer;

        private int _moveSteps;

        public int CurrentPositionIndex = -1;
        public bool IsInPlay => CurrentPositionIndex >= 0;

        public bool HasKilledOtherPawn;

        public Transform HomePosition;
        
        private Collider2D _pawnCollider2D;
        
        [Header("Pawn Canvas")]
        [SerializeField] private GameObject PawnCanvas;
        [SerializeField] private Image DiceImage1;
        [SerializeField] private Image DiceImage2;
        [SerializeField] private Button DiceMoveStepButton1;
        [SerializeField] private Button DiceMoveStepButton2;

        private void Start()
        {
            _pawnCollider2D = GetComponent<Collider2D>();
        }

        private void OnMouseDown()
        {
            if (MainPlayer.PlayerDiceResults.Count == 2)
            {
                PawnCanvas.SetActive(true);
                DiceImage1.sprite = MainPlayer.DiceResultSprites[MainPlayer.PlayerDiceResults[0] - 1];
                DiceImage2.sprite = MainPlayer.DiceResultSprites[MainPlayer.PlayerDiceResults[1] - 1];
                DiceMoveStepButton1.onClick.AddListener(() => PawnClicked(0));
                DiceMoveStepButton2.onClick.AddListener(() => PawnClicked(1));
            }
            else
            {
                MovePawn(MainPlayer.PlayerDiceResults[0]);
            }
        }

        public void ShowPawnOption()
        {
            PawnBaseSprite.SetActive(true);
        }

        public void HidePawnOption()
        {
            PawnBaseSprite.SetActive(false);
        }

        private void PawnClicked(int index)
        {
            MovePawn(MainPlayer.PlayerDiceResults[index] - 1);
            PawnCanvas.SetActive(false);
        }

        public void EnterBoard()
        {
            Node startNode = MainPlayer.PawnPath[0];
            if (!IsInPlay && startNode.CanPawnEnter(this))
            {   
                startNode.AddPawn(this);
                CurrentNode = startNode;
                StartCoroutine(MovePawnLogic(startNode, 0.2f));
            }
        }


        private void MovePawn()
        {
            // logic to handle movements between start and end node (position)
        }

        public void MovePawn(int moveSteps)
        {
            for (int i = 0; i < moveSteps; i++)
            {
                Node nextNode = MainPlayer.GetNextNodeForPawn(this);

                if (nextNode != null)
                {
                    if (CurrentNode != null)
                    {
                        CurrentNode.RemovePawn(this);
                    }
                    nextNode.AddPawn(this);
                    CurrentPositionIndex++;
                    CurrentNode = nextNode;
                    StartCoroutine(MovePawnLogic(nextNode, 0.5f));
                    nextNode.PositionPawns();
                }
            }

            MainPlayer.PlayerDiceResults.Remove(moveSteps);
        }

        // checking if this pawn can move the given steps forward
        // error here: dont forget to check
        public bool CanPawnMove(int steps)
        {
            int targetIndex = CurrentPositionIndex;

            // If the target index exceeds the path, movement is not allowed
            if (targetIndex >= MainPlayer.PawnPath.Count) return false;

            // Check if movement is blocked
            for (int i = CurrentPositionIndex; i < targetIndex && i < MainPlayer.PawnPath.Count; i++)
            {
                if (MainPlayer.PawnPath[i].PawnsOnNode.Count >= MainPlayer.PawnPath[i].MaxPawnsAllowed)
                {
                    return false;
                }
            }
            return true;
        }


        private IEnumerator MovePawnLogic(Node nextNode, float moveDuration)
        {
            Vector3 startPosition = transform.position;
            Vector3 endPosition = nextNode.transform.position;
            float elapsedTime = 0f;
            // float moveDuration = 1f;

            while (elapsedTime < moveDuration)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = endPosition; // ensuring final position is exact
            nextNode.PositionPawns();
        }
        
        

        public void EnableCollider()
        {
            _pawnCollider2D.enabled = true;
        }

        public void DisableCollider()
        {
            _pawnCollider2D.enabled = false;
        }

        private void ResetToHomePosition()
        {
            CurrentPositionIndex = -1;
            CurrentNode?.RemovePawn(this);
            CurrentNode = null;
            StartCoroutine(MovePawnLogic(HomePosition.GetComponent<Node>(), 0.3f));
        }

        private void KillOtherPawn()
        {
            // kill the pawn
        }

        private void BonusMove()
        {
        }
    }
}