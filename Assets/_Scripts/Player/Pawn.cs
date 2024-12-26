using System;
using System.Collections;
using _Scripts.Board;
using _Scripts.Managers;
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
        public int MovePawnToThisIndex;
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

        public delegate void PawnMovedHandler();
        public event PawnMovedHandler OnPawnMoveComplete;

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
            
            else if (MainPlayer.PlayerDiceResults.Count == 1)
            {
                MovePawn(MainPlayer.PlayerDiceResults[0]);
                HidePawnOption();
                foreach (Pawn p in MainPlayer._enteredPawns)
                {
                   p.DisableCollider(); 
                   p.HidePawnOption();
                }
            }
            else
            {
                GameManager.INSTANCE.ChangeTurn();
            }

            // check this somewhere else
            if (MainPlayer.PlayerDiceResults.Count == 0)
            {
                GameManager.INSTANCE.ChangeTurn();
                if (MainPlayer.HasBonusMove())
                {
                    // bonus move logic from player
                    MainPlayer.BonusMove(MainPlayer.bonusMove);
                    MovePawn(MainPlayer.bonusMove);
                }
                else
                {
                    
                }
            }
        }
        
        // enables pawn indicator
        public void ShowPawnOption()
        {
            PawnBaseSprite.SetActive(true);
        }

        // disables pawn indicator
        public void HidePawnOption()
        {
            PawnBaseSprite.SetActive(false);
        }
        
        // assigned in pawn dice click option buttons
        private void PawnClicked(int index)
        {
            MovePawn(MainPlayer.PlayerDiceResults[index] - 1);
            PawnCanvas.SetActive(false);
            MainPlayer.PlayerDiceResults.RemoveAt(index);
        }

        public void EnterBoard()
        {
            Node startNode = MainPlayer.PawnPath[0];
            if (!IsInPlay && startNode.CanPawnEnter(this))
            {   
                startNode.AddPawn(this);
                CurrentPositionIndex = 0;
                CurrentNode = startNode; 
                transform.position = startNode.transform.position;
                StartCoroutine(PlayEnterBoardAnimation());
            }
        }

        private void MovePawn()
        {
            // logic to handle movements between start and end node (position)
        }

        public void MovePawn(int moveSteps)
        {
            int targetPositionIndex = CurrentPositionIndex + moveSteps;
            if (targetPositionIndex >= MainPlayer.PawnPath.Count)
            {
                targetPositionIndex = MainPlayer.PawnPath.Count - 1;
            }
            Node lastNodeInMove = MainPlayer.PawnPath[targetPositionIndex];
            
            for (int i = 0; i < moveSteps; i++)
            {
                Node nextNode = MainPlayer.GetNextNodeForPawn(this);

                if (nextNode)
                {
                    if (CurrentNode)
                    {
                        CurrentNode.RemovePawn(this);
                    }
                    nextNode.AddPawn(this);
                    CurrentPositionIndex++;
                    CurrentNode = nextNode;
                    StartCoroutine(MovePawnLogic(nextNode, 0.5f));
                }

                if (nextNode == lastNodeInMove)
                {
                    Debug.Log("Last Node in the move reached: " + lastNodeInMove);
                    // try to kill other pawn in the node if it is present
                    
                    lastNodeInMove.EliminatePawn(this);
                    MainPlayer.bonusMove = 20;
                    Debug.Log("Bonus Move");
                }
                nextNode.PositionPawns();
            }

            MainPlayer.PlayerDiceResults.Remove(moveSteps);
            OnPawnMoveComplete?.Invoke();
        }

        // call this in a ui button thorugh editor
        public void MovePawnToIndex()
        {
            if (CurrentPositionIndex == MovePawnToThisIndex) return;
            if (CurrentPositionIndex != MovePawnToThisIndex)
            {
                MainPlayer.PawnPath[MovePawnToThisIndex].AddPawn(this);
                if (CurrentPositionIndex == -1)
                {
                    MainPlayer._myPawns.Remove(this);
                    MainPlayer._enteredPawns.Add(this);
                }
                if (CurrentPositionIndex != -1)
                {
                    MainPlayer.PawnPath[CurrentPositionIndex].RemovePawn(this);
                }
                else
                {
                    // do nothing
                }

                if (MainPlayer.PawnPath[MovePawnToThisIndex].PawnsOnNode.Count == 2)
                {
                    // do nothing
                }
                else if (MainPlayer.PawnPath[MovePawnToThisIndex].PawnsOnNode.Count != 2)
                {
                    transform.position = MainPlayer.PawnPath[MovePawnToThisIndex].transform.position;
                    CurrentPositionIndex = MovePawnToThisIndex;
                }
                
                MainPlayer.PawnPath[MovePawnToThisIndex].PositionPawns();
            }
            else
            {
                return;
            }

            Debug.Log("{this}" + IsInPlay);
        }
        

        // checking if this pawn can move the given steps forward
        // error here: dont forget to check
        public bool CanPawnMove(int steps)
        {
            if (CurrentPositionIndex == -1)
                CurrentPositionIndex = 0;
            
            int targetIndex = CurrentPositionIndex + steps - 1;
            // if (targetIndex >= MainPlayer.PawnPath.Count - 1) return false;
            for (int i = CurrentPositionIndex; i < targetIndex && i < MainPlayer.PawnPath.Count - 1; i++)
            {
                if (!MainPlayer.PawnPath[i].CanPawnEnter(this))
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
        
        private IEnumerator PlayEnterBoardAnimation()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 enlargedScale = originalScale * 1.5f;

            float animationDuration = 0.3f;
            float elapsedTime = 0f;

            //scaling up
            while (elapsedTime < animationDuration / 2)
            {
                transform.localScale = Vector3.Lerp(originalScale, enlargedScale, elapsedTime / (animationDuration / 2));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            elapsedTime = 0f;

            //scaling down
            while (elapsedTime < animationDuration / 2)
            {
                transform.localScale = Vector3.Lerp(enlargedScale, originalScale, elapsedTime / (animationDuration / 2));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localScale = originalScale;
        }
        
        public void EnableCollider()
        {
            _pawnCollider2D.enabled = true;
        }

        public void DisableCollider()
        {
            _pawnCollider2D.enabled = false;
        }

        public void ResetToHomePosition() 
        {
            Debug.Log(IsInPlay);
            if (CurrentPositionIndex == -1) return;
            CurrentPositionIndex = -1;
            CurrentNode?.RemovePawn(this);
            CurrentNode = null;
            MainPlayer._enteredPawns.Remove(this);
            MainPlayer._myPawns.Add(this);
            // StartCoroutine(MovePawnLogic(HomePosition.GetComponent<Node>(), 0.3f));
            transform.position = HomePosition.position;
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