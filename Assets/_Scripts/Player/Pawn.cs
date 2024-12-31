using System;
using System.Collections;
using _Scripts.Board;
using _Scripts.Managers;
using TMPro;
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

        public bool IsPawnClicked;
        public bool IsPawnMovable;

        public Transform HomePosition;
        public static event Action OnPawnMoveCompleted;
        private Collider2D _pawnCollider2D;

        [Header("Pawn Canvas")] [SerializeField]
        private GameObject PawnCanvas;

        [SerializeField] private Image DiceImage1;
        [SerializeField] private Image DiceImage2;
        [SerializeField] private Button DiceMoveStepButton1;
        [SerializeField] private Button DiceMoveStepButton2;

        public TMP_Text AvailableMovesText;

        [SerializeField] private int MovePawnToThisIndex;
        public bool isHome;

        private void Start()
        {
            _pawnCollider2D = GetComponent<Collider2D>();
            DiceMoveStepButton1.onClick.AddListener(() => PawnClicked(0));
            DiceMoveStepButton2.onClick.AddListener(() => PawnClicked(1));
        }

        private void OnMouseDown()
        {
            if (MainPlayer.PlayerDiceResults.Count == 2)
            {
                for (int i = 0; i < MainPlayer._enteredPawns.Count; i++)
                {
                    int thisPlayerIndex = MainPlayer._enteredPawns.IndexOf(this);
                    MainPlayer._enteredPawns[i].IsPawnClicked = i != thisPlayerIndex;
                    MainPlayer._enteredPawns[i].PawnCanvas.SetActive(!MainPlayer._enteredPawns[i].IsPawnClicked);
                }
            }

            if (IsPawnClicked)
            {
                PawnCanvas.SetActive(false);
                IsPawnClicked = false;
            }
            else
            {
                if (MainPlayer.PlayerDiceResults.Count == 2)
                {
                    PawnMainSprite.sortingOrder = 20;
                    PawnPlayWhenClicked(MainPlayer.PlayerDiceResults[0], MainPlayer.PlayerDiceResults[1]);
                }

                else if (MainPlayer.PlayerDiceResults.Count == 1)
                {
                    int remMove = MainPlayer.PlayerDiceResults[0];
                    MainPlayer.PlayerDiceResults.Clear();
                    MovePawn(remMove);
                    HidePawnOption();
                }
                else if (MainPlayer.HasBonusMove)
                {
                    MovePawn(MainPlayer.BonusMove);
                    MainPlayer.HasBonusMove = false;
                    MainPlayer.BonusMove = 0;
                }

                IsPawnClicked = true;
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
            MovePawn(MainPlayer.PlayerDiceResults[index]);
            // MainPlayer.PlayerDiceResults.RemoveAt(index);
            PawnCanvas.SetActive(false);
        }

        public void PawnPlayWhenClicked(int move1, int move2)
        {
            if (CanPawnMove(move1) && CanPawnMove(move2))
            {
                PawnCanvas.SetActive(true);
                DiceImage1.sprite = MainPlayer.DiceResultSprites[MainPlayer.PlayerDiceResults[0] - 1];
                DiceImage2.sprite = MainPlayer.DiceResultSprites[MainPlayer.PlayerDiceResults[1] - 1];
            }
            else if (CanPawnMove(move1) || CanPawnMove(move2))
            {
                switch (true)
                {
                    case bool _ when CanPawnMove(move1):
                        MovePawn(move1);
                        PawnCanvas.SetActive(false);
                        break;
                    case bool _ when CanPawnMove(move2):
                        MovePawn(move2);
                        PawnCanvas.SetActive(false);
                        break;
                }
            }
            else
            {
                HidePawnOption();
                DisableCollider();
            }
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

                // update main players pawn collection
                // remove this pawn from original collection and add to entered pawns
                MainPlayer._enteredPawns.Add(this);
                MainPlayer._myPawns.Remove(this);
                MainPlayer._pawnsInPlay = MainPlayer._enteredPawns.Count;

                StartCoroutine(PlayEnterBoardAnimation());
                startNode.EliminatePawn(this);
            }

            MainPlayer.OnPawnMoveComplete();
        }

        public void MovePawn(int moveSteps)
        {
            if (!IsInPlay) return;
            StartCoroutine(MovePawn(moveSteps, 0.2f));
        }

        private IEnumerator MovePawn(int moveSteps, float moveDuration)
        {
            int targetPositionIndex = CurrentPositionIndex + moveSteps;
            if (targetPositionIndex >= MainPlayer.PawnPath.Count)
            {
                targetPositionIndex = MainPlayer.PawnPath.Count - 1;
            }

            Node lastNodeInMove = MainPlayer.PawnPath[targetPositionIndex];

            foreach (Pawn p in MainPlayer._enteredPawns)
            {
                p.DisableCollider();
                p.HidePawnOption();
                p.AvailableMovesText.text = "";
                p.IsPawnMovable = false;
            }

            for (int i = 0; i < moveSteps; i++)
            {
                Node nextNode = GetNextNodeForPawn();

                if (nextNode)
                {
                    if (CurrentNode)
                    {
                        CurrentNode.RemovePawn(this);
                    }

                    nextNode.AddPawn(this);
                    CurrentPositionIndex++;
                    CurrentNode = nextNode;

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

                    transform.position = endPosition;

                    nextNode.PositionPawns();
                }

                if (nextNode == lastNodeInMove)
                {
                    // try to kill other pawn in the node if it is present
                    lastNodeInMove.EliminatePawn(this);
                }
            }

            MainPlayer.PlayerDiceResults.Remove(moveSteps);
            MainPlayer.OnPawnMoveComplete();
        }

        // call this in an ui button through editor
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


                transform.position = MainPlayer.PawnPath[MovePawnToThisIndex].transform.position;
                CurrentPositionIndex = MovePawnToThisIndex;

                MainPlayer._pawnsInPlay = MainPlayer._enteredPawns.Count;

                MainPlayer.PawnPath[MovePawnToThisIndex].PositionPawns();
                GameManager.INSTANCE.CheckForVictory();
            }
        }

        // checking if this pawn can move the given steps forward
        public bool CanPawnMove(int steps)
        {
            if (CurrentPositionIndex == -1)
                CurrentPositionIndex = 0;

            int targetIndex = CurrentPositionIndex + steps;
            int stepsToVictory = MainPlayer.PawnPath.Count - CurrentPositionIndex - 1;

            // ensuring the target index is within bounds
            if (targetIndex > MainPlayer.PawnPath.Count - 1) return false;

            // check if the target node is a StartNode or starNode
            if (MainPlayer.PawnPath[targetIndex].IsStartNode && (MainPlayer.PawnPath[targetIndex].IsStarNode))
            {
                if(MainPlayer.PawnPath[targetIndex].CanPawnEnter(this))
                    return true;
                else
                    return false;
            }

            // Check if each node in the path to the target index can be entered
            for (int i = CurrentPositionIndex + 1; i <= targetIndex; i++)
            {
                if (!MainPlayer.PawnPath[i].CanPawnEnter(this))
                {
                    return false;
                }
            }

            // Ensure steps do not exceed steps to victory
            if (steps > stepsToVictory) return false;

            return true;
        }

        private Node GetNextNodeForPawn()
        {
            int currentIndex = CurrentPositionIndex;

            if (currentIndex + 1 < MainPlayer.PawnPath.Count)
            {
                Node nextNode = MainPlayer.PawnPath[currentIndex + 1];
                return nextNode;
            }
            else
            {
                return null;
            }
        }

        // get previous node for the pawn
        // used when pawn is killed and needs to return to home
        private Node GetPreviousNodeForPawn()
        {
            int currentIndex = CurrentPositionIndex;

            if (currentIndex - 1 > -1)
            {
                Node previousNode = MainPlayer.PawnPath[currentIndex - 1];
                return previousNode;
            }
            else
            {
                return null;
            }
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
                transform.localScale =
                    Vector3.Lerp(originalScale, enlargedScale, elapsedTime / (animationDuration / 2));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            elapsedTime = 0f;

            //scaling down
            while (elapsedTime < animationDuration / 2)
            {
                transform.localScale =
                    Vector3.Lerp(enlargedScale, originalScale, elapsedTime / (animationDuration / 2));
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
            if (CurrentPositionIndex == -1) return;

            CurrentPositionIndex = -1;
            CurrentNode?.RemovePawn(this);
            CurrentNode = null;
            MainPlayer._enteredPawns.Remove(this);
            MainPlayer._myPawns.Add(this);
            transform.position = HomePosition.position;
            StartCoroutine(PlayEnterBoardAnimation());
        }

        private IEnumerator ResetToHomePositionCo()
        {
            int moveSteps = CurrentPositionIndex;
            for (int i = 0; i < moveSteps; i++)
            {
                Node previousNode = GetPreviousNodeForPawn();
                if (previousNode)
                {
                    if (CurrentNode)
                    {
                        CurrentNode?.RemovePawn(this);
                    }

                    previousNode.AddPawn(this);
                    CurrentNode = previousNode;
                    CurrentPositionIndex--;

                    Vector3 startPosition = transform.position;
                    Vector3 endPosition = previousNode.transform.position;
                    Vector3 originalScale = transform.localScale;
                    Vector3 enlargedScale = originalScale * 1.5f;

                    float elapsedTime = 0f;
                    float moveDuration = 0.01f;

                    while (elapsedTime < moveDuration)
                    {
                        float t = elapsedTime / moveDuration; // normalized time for scaling and movement

                        // moving pawn
                        transform.position = Vector3.Lerp(startPosition, endPosition, t);
                        if (t < 0.5f)
                        {
                            transform.localScale = Vector3.Lerp(originalScale, enlargedScale, t / 0.5f);
                        }
                        else
                        {
                            transform.localScale = Vector3.Lerp(enlargedScale, originalScale, (t - 0.5f) / 0.5f);
                        }

                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }

                    transform.position = endPosition;
                    transform.localScale = originalScale;
                    previousNode.PositionPawns();
                }
                // transform.position = HomePosition.position;
            }

            MainPlayer._enteredPawns.Remove(this);
            MainPlayer._myPawns.Add(this);
            CurrentPositionIndex = -1;
            // StartCoroutine(PlayEnterBoardAnimation());
            CurrentNode = null;
        }
    }
}