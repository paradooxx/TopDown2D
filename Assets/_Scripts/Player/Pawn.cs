using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Board;
using _Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;

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
        public bool IsHome => CurrentPositionIndex == 71;

        public bool IsPawnClicked;
        public bool IsPawnMovable;

        public Transform HomePosition;
        [SerializeField] private Collider2D PawnCollider2D;

        [Header("Pawn Canvas")] [SerializeField]
        private GameObject PawnCanvas;

        [SerializeField] private List<Image> DiceImage;
        [SerializeField] private List<Button> DiceButtons;

        public TMP_Text AvailableMovesText;

        [Header("Bot Move Score")] 
        public int BotMoveScore;
        public int BotTwoMovesScore;
        public List<int> TakeStep = new List<int> {0, 0};
        [SerializeField] private int MovePawnToThisIndex;

        private void Start()
        {
            for (int i = 0; i < DiceButtons.Count; i++)
            {
                var index = i;
                DiceButtons[i].onClick.AddListener(() => PawnClicked(index));
            }
        }

        private void OnMouseDown()
        {
            if (MainPlayer.GetActiveDiceCount() == 2)
            {
                for (int i = 0; i < MainPlayer.EnteredPawns.Count; i++)
                {
                    int thisPlayerIndex = MainPlayer.EnteredPawns.IndexOf(this);
                    MainPlayer.EnteredPawns[i].IsPawnClicked = i != thisPlayerIndex;
                    MainPlayer.EnteredPawns[i].PawnCanvas.SetActive(!MainPlayer.EnteredPawns[i].IsPawnClicked);
                }
            }

            if (IsPawnClicked)
            {
                PawnCanvas.SetActive(false);
                IsPawnClicked = false;
            }
            else
            {
                if (MainPlayer.GetActiveDiceCount() == 2)
                {
                    PawnMainSprite.sortingOrder = 20;
                    PawnPlayWhenClicked(MainPlayer.DiceStates[0].Value, MainPlayer.DiceStates[1].Value);
                }

                else if (MainPlayer.GetActiveDiceCount() == 1)
                {
                    int remMove = MainPlayer.DiceStates.Find(d => d.diceState == true).Value;
                    MovePawn(remMove);
                    HidePawnOption();
                    MainPlayer.ResetDiceStates();
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
            MovePawn(MainPlayer.DiceStates[index].Value);
            // MainPlayer.PlayerDiceResults.RemoveAt(index);
            PawnCanvas.SetActive(false);
        }

        public void PawnPlayWhenClicked(int move1, int move2)
        {
            if (CanPawnMove(move1) && CanPawnMove(move2))
            {
                PawnCanvas.SetActive(true);
                for (int i = 0; i < DiceImage.Count; i++)
                {
                    DiceImage[i].sprite = MainPlayer.DiceResultSprites[MainPlayer.DiceStates[i].Value - 1];
                }
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
                Vector3 homePosition = transform.position;
                
                // Setup the initial state
                startNode.AddPawn(this);
                CurrentPositionIndex = 0;
                CurrentNode = startNode;

                // updating main player's pawn collections
                MainPlayer.EnteredPawns.Add(this);
                MainPlayer.HomePawns.Remove(this);
                MainPlayer.PawnsInPlay = MainPlayer.EnteredPawns.Count;
                
                // Create a sequence for the entire animation
                Sequence enterSequence = DOTween.Sequence();

                // First move from home position to start node
                enterSequence.Append(
                    transform.DOMove(startNode.transform.position, 0.3f)
                        .SetEase(Ease.OutQuad)
                );

                // Then do the pop animation
                enterSequence.Append(
                    transform.DOScale(transform.localScale * 1.5f, 0.01f)
                        .SetEase(Ease.OutQuad)
                );

                enterSequence.Append(
                    transform.DOScale(transform.localScale, 0.01f)
                        .SetEase(Ease.InQuad)
                );
                // Handle completion
                enterSequence.OnComplete(() => { EliminatePawn(startNode); });

                // Play the sequence
                enterSequence.Play();
            }

            // MainPlayer.PlayerDiceResults.Remove(5);
            MainPlayer.DisableDiceState(5);
            MainPlayer.DimDiceSprite();
            MainPlayer.OnPawnMoveComplete();
        }

        public void MovePawn(int moveSteps)
        {
            if (!IsInPlay) return;
            float moveDuration = 0.3f;
            int targetPositionIndex = CurrentPositionIndex + moveSteps;
            if (targetPositionIndex >= MainPlayer.PawnPath.Count)
            {
                targetPositionIndex = MainPlayer.PawnPath.Count - 1;
            }

            Node lastNodeInMove = MainPlayer.PawnPath[targetPositionIndex];

            // disable all pawns during movement
            foreach (Pawn p in MainPlayer.EnteredPawns)
            {
                p.DisableCollider();
                p.HidePawnOption();
                p.AvailableMovesText.text = "";
                p.IsPawnMovable = false;
            }

            Sequence moveSequence = DOTween.Sequence();
            moveSequence.PrependInterval(0.2f);
            int currentStep = 0;
            PawnMainSprite.sortingOrder = 15;
            for (int i = 0; i < moveSteps; i++)
            {
                Node nextNode = GetNextNodeForPawn();

                if (nextNode)
                {
                    if (CurrentNode)
                    {
                        CurrentNode.RemovePawn(this);
                    }

                    // nextNode.AddPawn(this);
                    CurrentPositionIndex++;
                    CurrentNode = nextNode;
                    Vector3 endPosition = nextNode.transform.position;
                    
                    moveSequence
                        .Append(transform.DOScale(Vector3.one * 1.2f, moveDuration * 0.25f)) // scaling the pawn
                        .Join(transform.DOMove(endPosition, moveDuration * 0.5f)
                            .SetEase(Ease.InOutQuad)) // moving while scaling
                        .Append(transform.DOScale(Vector3.one, moveDuration * 0.25f)); // scaling pawn back down

                    // callback for positioning pawns after each move
                    int stepCopy = currentStep;
                    moveSequence.AppendCallback(() =>
                    {
                        transform.position = endPosition;
                        nextNode.PositionPawns();

                        // checking if this is the last node for given move steps
                        if (stepCopy == moveSteps - 1)
                        {
                            EliminatePawn(lastNodeInMove);
                            lastNodeInMove.AddPawn(this);
                        }
                    });

                    currentStep++;
                }
            }

            // final callback for movement completion
            moveSequence.OnComplete(() =>
            {
                PawnMainSprite.sortingOrder = 10;
                MainPlayer.DisableDiceState(moveSteps);
                MainPlayer.DimDiceSprite();
                MainPlayer.OnPawnMoveComplete();
            });

            moveSequence.Play();
        }

        // calling this in an ui button through editor
        public void MovePawnToIndex()
        {
            if (CurrentPositionIndex == MovePawnToThisIndex) return;
            if (CurrentPositionIndex != MovePawnToThisIndex)
            {
                MainPlayer.PawnPath[MovePawnToThisIndex].AddPawn(this);
                if (CurrentPositionIndex == -1)
                {
                    MainPlayer.HomePawns.Remove(this);
                    MainPlayer.EnteredPawns.Add(this);
                }

                if (CurrentPositionIndex != -1)
                {
                    MainPlayer.PawnPath[CurrentPositionIndex].RemovePawn(this);
                }

                transform.position = MainPlayer.PawnPath[MovePawnToThisIndex].transform.position;
                CurrentPositionIndex = MovePawnToThisIndex;

                MainPlayer.PawnsInPlay = MainPlayer.EnteredPawns.Count;

                MainPlayer.PawnPath[MovePawnToThisIndex].PositionPawns();
                MainPlayer.CheckForVictory();
            }
        }

        // checking if this pawn can move the given steps forward
        public bool CanPawnMove(int steps)
        {
            if (CurrentPositionIndex == -1)
                CurrentPositionIndex = 0;

            int targetIndex = CurrentPositionIndex + steps;
            int stepsToVictory = MainPlayer.PawnPath.Count - CurrentPositionIndex - 1;

            if (targetIndex > MainPlayer.PawnPath.Count - 1) return false;

            // check if each node in the path to the target index can be entered
            for (int i = CurrentPositionIndex + 1; i <= targetIndex; i++)
            {
                if (!MainPlayer.PawnPath[i].CanPawnEnter(this))
                {
                    return false;
                }
            }

            if (steps > stepsToVictory) return false;

            return true;
        }

        // gets next node for pawn to move
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

        public void EliminatePawn(Node node)
        {
            if (node.IsStarNode) return;

            foreach (Pawn p in new List<Pawn>(node.PawnsOnNode))
            {
                if (p.MainPlayer != MainPlayer)
                {
                    p.ResetToHomePosition();
                    MainPlayer.OtherPawnKillCount++;
                    if (MainPlayer.OtherPawnKillCount > 0)
                    {
                        MainPlayer.HasBonusMove = true;
                        MainPlayer.BonusMove = 20;
                    }

                    if (p.MainPlayer.PawnsInPlay < 0)
                    {
                        p.MainPlayer.PawnsInPlay = 0;
                    }
                    else
                    {
                        p.MainPlayer.PawnsInPlay--;
                    }
                }
            }
        }

        public void EnableCollider()
        {
            PawnCollider2D.enabled = true;
        }

        public void DisableCollider()
        {
            PawnCollider2D.enabled = false;
        }

        public void ResetToHomePosition()
        {
            if (CurrentPositionIndex == -1) return;

            CurrentPositionIndex = -1;

            // removing the pawn from its current node and updating its state
            CurrentNode?.RemovePawn(this);
            CurrentNode = null;
            MainPlayer.EnteredPawns.Remove(this);
            MainPlayer.HomePawns.Add(this);
            
            transform.DOMove(HomePosition.position, 0.5f) // Adjust duration as needed
                .SetEase(Ease.InOutQuad) // Use a smooth easing function
                .OnComplete(() =>
                {
                    // Any additional logic to execute after the animation completes
                    Debug.Log("Pawn reset to home position.");
                });
        }
    }
}