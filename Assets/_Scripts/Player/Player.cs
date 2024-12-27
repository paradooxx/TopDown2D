using System.Collections;
using System.Collections.Generic;
using _Scripts.Board;
using _Scripts.Enums;
using _Scripts.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Player
{
    public class Player : MonoBehaviour
    {
        public PlayerType PlayerType;
        public List<Node> PawnPath; // fixed path where this player's pawn can move
        public List<Transform> HomePosition; // home position's for this player's pawns
        private readonly GameObject[] _instantiatedPawns = new GameObject[4]; // list of this player's pawn

        public List<Pawn> _myPawns = new List<Pawn>();
        public List<Pawn> _enteredPawns = new List<Pawn>();
        public List<Collider2D> _myPawnsColliders = new List<Collider2D>();

        [SerializeField] private GameObject PawnPrefab;
        [SerializeField] private DiceManager DiceManager;
        [SerializeField] private Sprite PawnSprite;

        public Sprite[] DiceResultSprites;

        public Transform MyDicePosition;

        public List<int> PlayerDiceResults = new List<int>(2);
        public bool IsMyTurn;
        public bool RepeatTurn = false;
        public bool ChangeTurn;
        public bool ResetDice => PlayerDiceResults.Count == 0;

        public int _pawnsInPlay;
        public int bonusMove;

        private void Start()
        {
            InitializeMyPawns();
        }
        
        private void OnEnable()
        {
            Pawn.OnPawnMoveCompleted += OnPawnMoveComplete;
        }

        private void OnDisable()
        {
            Pawn.OnPawnMoveCompleted -= OnPawnMoveComplete;
        }

        // setting this player's pawns in the respective home positions
        private void InitializeMyPawns()
        {
            for (int i = 0; i < 4; i++)
            {
                _instantiatedPawns[i] = Instantiate(PawnPrefab, HomePosition[i].position, Quaternion.identity);
                _myPawns.Add(_instantiatedPawns[i].GetComponent<Pawn>());
                _myPawns[i].HomePosition = HomePosition[i];
                _myPawns[i].PawnMainSprite.sprite = PawnSprite;
                _myPawns[i].MainPlayer = this;
                _myPawnsColliders.Add(_myPawns[i].GetComponent<Collider2D>());
                _myPawnsColliders[i].enabled = false;
            }
        }

        public void MakePawnEnterBoard()
        {
            switch (_pawnsInPlay)
            {
                case 0:
                    PawnEnterCaseZero();
                    break;
                case 1:
                    PawnEnterCaseOne();
                    break;
                case 2:
                    PawnEnterCaseTwo();
                    break;
                case 3:
                    PawnEnterCaseThree();
                    break;
                default:
                    Debug.Log("Unexpected _pawnsInPlay value: " + _pawnsInPlay);
                    break;

            }
        }

        // pawn entry cases when there is/arenot pawns already
        // entry case when no pawn is in play
        private void PawnEnterCaseZero()
        {
            if (PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                EnterPawnOnBoard(_myPawns[0]);
                PlayerDiceResults.Clear();
                GameManager.INSTANCE.ChangeTurn(false, ResetDice);
                ;
            }
            else if (PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                PlayerDiceResults.Clear();
                GameManager.INSTANCE.ChangeTurn(true, ResetDice);
            }
            else if (PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                PlayerDiceResults.Remove(5);
                MoveActivePawn(_enteredPawns[0], PlayerDiceResults[0]);
                GameManager.INSTANCE.ChangeTurn(true, ResetDice);
            }
            else if (PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                PlayerDiceResults.Clear();
                GameManager.INSTANCE.ChangeTurn(false, ResetDice);
            }
            else
            {
                PlayerDiceResults.Clear();
                GameManager.INSTANCE.ChangeTurn(true, ResetDice);
            }
        }

        // entry case when 1 play is in play
        private void PawnEnterCaseOne()
        {
            if (PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                PlayerDiceResults.RemoveAt(1);
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.RemoveAt(0);
                }
                else
                {
                    MakePawnPlay(PlayerDiceResults[0]);
                }
                GameManager.INSTANCE.ChangeTurn(false, ResetDice);
            }
            else if (PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                PlayerDiceResults.Remove(5);
                MakePawnPlay(PlayerDiceResults[0]);
            }
            else if (PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                PlayerDiceResults.Clear();
                GameManager.INSTANCE.ChangeTurn(true, ResetDice);
            }
            else if (PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                MoveActivePawn(_enteredPawns[0], PlayerDiceResults[0] + PlayerDiceResults[1]);
                GameManager.INSTANCE.ChangeTurn(false, ResetDice);
            }
            else
            {
                MoveActivePawn(_enteredPawns[0], PlayerDiceResults[0]);
                MoveActivePawn(_enteredPawns[0], PlayerDiceResults[1]);
                // MoveActivePawn(_enteredPawns[0], PlayerDiceResults[0] + PlayerDiceResults[1]);
                GameManager.INSTANCE.ChangeTurn(true, ResetDice);
            }
        }

        // entry case when 2 pawns are in play
        private void PawnEnterCaseTwo()
        {
            if (PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count == 0)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.Clear();
                    GameManager.INSTANCE.ChangeTurn(false, ResetDice);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 1)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.RemoveAt(1);
                    MakePawnPlay(5);
                    ChangeTurn = false;
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    MoveActivePawn(_enteredPawns[0], 5);
                    EnterPawnOnBoard(_myPawns[0]);
                    GameManager.INSTANCE.ChangeTurn(false, ResetDice);
                    ;
                }
                // GameManager.INSTANCE.ChangeTurn(false);;
            }
            else if (PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.Remove(5);
                    MakePawnPlay(PlayerDiceResults[0]);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    int nonFiveDiceResult = PlayerDiceResults[0] == 5 ? PlayerDiceResults[1] : PlayerDiceResults[0];

                    // moving the active pawn using the non-5 dice result
                    MoveActivePawn(_enteredPawns[0], nonFiveDiceResult);
                    PlayerDiceResults.Remove(nonFiveDiceResult);
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.Clear();
                    GameManager.INSTANCE.ChangeTurn(true, ResetDice);
                }
            }
            else if (PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.Clear();
                    GameManager.INSTANCE.ChangeTurn(true, ResetDice);
                }
                else
                {
                    MakePawnPlay(PlayerDiceResults[0] + PlayerDiceResults[1]);
                }
            }
            else if (PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                MakePawnPlay(PlayerDiceResults[0] + PlayerDiceResults[1]);
                MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
                GameManager.INSTANCE.ChangeTurn(false, ResetDice);
                ;
            }
            else
            {
                // MakePawnPlay(PlayerDiceResults[0] + PlayerDiceResults[1]);
                MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
                // GameManager.INSTANCE.ChangeTurn();
            }
        }

        // entry case when 3 pawns are in play
        private void PawnEnterCaseThree()
        {
            if (PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.Remove(5);
                    MakePawnPlay(PlayerDiceResults[0]);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    MoveActivePawn(PawnPath[0].PawnsOnNode[0], 5);
                    EnterPawnOnBoard(_myPawns[0]);
                }
                else
                {
                    MakePawnPlay(PlayerDiceResults[0] + PlayerDiceResults[1]);
                    MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
                }

                GameManager.INSTANCE.ChangeTurn(false, ResetDice);
            }
            else if (PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.Remove(5);
                    MakePawnPlay(PlayerDiceResults[0]);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    int nonFiveDiceResult = PlayerDiceResults[0] == 5 ? PlayerDiceResults[1] : PlayerDiceResults[0];

                    // moving the active pawn using the non-5 dice result
                    // if three pawns are already in game and two pawns are at start move one of the pawns
                    MoveActivePawn(PawnPath[0].PawnsOnNode[1], nonFiveDiceResult);
                    PlayerDiceResults.Remove(nonFiveDiceResult);
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.Clear();
                    GameManager.INSTANCE.ChangeTurn(true, ResetDice);
                }
            }
            else if (PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.Clear();
                }
                else
                {
                    MakePawnPlay(PlayerDiceResults[0] + PlayerDiceResults[1]);
                    MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
                }
            }
            else if (PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                MakePawnPlay(PlayerDiceResults[0] + PlayerDiceResults[1]);
                MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
                GameManager.INSTANCE.ChangeTurn(false, ResetDice);
                ;
            }
            else
            {
                MakePawnPlay(PlayerDiceResults[0] + PlayerDiceResults[1]);
                MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
                // GameManager.INSTANCE.ChangeTurn();
            }
        }

        public void MakePawnPlay(int moveStep)
        {
            if (PlayerDiceResults.Count == 0 && !HasBonusMove())
            {
                GameManager.INSTANCE.ChangeTurn(true, ResetDice);
                return;
            }

            for (int i = 0; i < _enteredPawns.Count; i++)
            {
                if (_enteredPawns[i].CanPawnMove(moveStep))
                {
                    _enteredPawns[i].ShowPawnOption();
                    _enteredPawns[i].EnableCollider();
                }
            }
        }

        public void MakePawnPlayTwoSteps(int moveStep1, int moveStep2)
        {
            if (PlayerDiceResults.Count == 0 && !HasBonusMove())
            {
                GameManager.INSTANCE.ChangeTurn(true, ResetDice);
                return;
            }

            if (moveStep1 == moveStep2)
            {
            }

            if (moveStep1 > moveStep2)
            {
                MakePawnPlay(moveStep2);
            }
            else if (moveStep1 < moveStep2)
            {
                MakePawnPlay(moveStep1);
            }
            else
            {
                MakePawnPlay(moveStep1);
                MakePawnPlay(moveStep2);
            }
        }
        
        public void OnPawnMoveComplete()
        {
            if (PlayerDiceResults.Count == 1)
            {
                MakePawnPlay(PlayerDiceResults[0]);
            }
            else if (PlayerDiceResults.Count == 2)
            {
                MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
            }
            else
            {
                foreach (Pawn p in _enteredPawns)
                {
                    p.DisableCollider();
                    p.HidePawnOption();
                }
            }

            Debug.Log("Invoked");
        }

        // enabling colliders of this player's pawns
        public void EnableMyPawns()
        {
            for (int i = 0; i < _myPawns.Count; i++)
            {
                if (_myPawns[i].IsInPlay)
                    _myPawnsColliders[i].enabled = true;
            }
        }

        public void DisableMyPawns()
        {
            for (int i = 0; i < _enteredPawns.Count; i++)
            {
                _enteredPawns[i].enabled = false;
            }
        }

        private void MoveActivePawn(Pawn pawn, int steps)
        {
            if (pawn.CanPawnMove(steps))
            {
                pawn.MovePawn(steps, ChangeTurn);
                PlayerDiceResults.Remove(steps);
            }
            else
            {
                MakePawnPlay(steps);
            }
        }

        //enters the pawn into enteredpawns list and removes it from mypawns list
        private void EnterPawnOnBoard(Pawn pawn)
        {
            pawn.EnterBoard();
            _enteredPawns.Add(pawn);
            _myPawns.Remove(pawn);
        }

        // get next location to move for this player's pawns
        public Node GetNextNodeForPawn(Pawn pawn)
        {
            int currentIndex = pawn.CurrentPositionIndex;

            if (currentIndex + 1 < pawn.MainPlayer.PawnPath.Count)
            {
                Node nextNode = pawn.MainPlayer.PawnPath[currentIndex + 1];
                return nextNode;
            }
            else
            {
                return null;
            }
        }

        public Node GetPreviousNodeForPawn(Pawn pawn)
        {
            int currentIndex = pawn.CurrentPositionIndex;

            if (currentIndex - 1 > -1)
            {
                Node previousNode = pawn.MainPlayer.PawnPath[currentIndex - 1];
                return previousNode;
            }
            else
            {
                return null;
            }
        }

        public bool HasBonusMove()
        {
            return false;
        }

        public void BonusMove(int bonusMoveStep)
        {
            // if (!HasBonusMove()) return;

            if (_enteredPawns.Count == 1)
            {
                // if the active pawn can move bonus step
                // the check if implemented but has errors
                MoveActivePawn(_enteredPawns[0], bonusMoveStep);
                //else break the loop and return
            }
            else if (_enteredPawns.Count > 1)
            {
                foreach (Pawn p in _enteredPawns)
                {
                    if (p.CanPawnMove(bonusMoveStep))
                    {
                        p.EnableCollider();
                        p.ShowPawnOption();
                    }
                }
            }
        }

        private bool MyPawnKilledOtherPawn()
        {
            return false;
        }
    }
}