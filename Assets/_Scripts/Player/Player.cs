using System.Collections.Generic;
using _Scripts.Board;
using _Scripts.Enums;
using _Scripts.Managers;
using UnityEngine;

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
        public bool HasBonusMove;

        public int _pawnsInPlay;
        public int OtherPawnKillCount = 0;
        public int BonusMove;

        private void Start()
        {
            InitializeMyPawns();
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

        #region MyRegion

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
        
        // pawn entry cases when there is/are not pawns already
        // entry case when no pawn is in play
        private void PawnEnterCaseZero()
        {
            if (PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5)
            {
                PlayerDiceResults.Clear();
                EnterPawnOnBoard(_myPawns[0]);
                EnterPawnOnBoard(_myPawns[0]);
            }
            else if (PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                PlayerDiceResults.Clear();
                EnterPawnOnBoard(_myPawns[0]);
            }
            else if (PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                PlayerDiceResults.Remove(5);
                EnterPawnOnBoard(_myPawns[0]);
                MoveActivePawn(_enteredPawns[0], PlayerDiceResults[0]);
            }
            else if (PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                PlayerDiceResults.Clear();
                GameManager.INSTANCE.ShouldChangeTurn = false;
                GameManager.INSTANCE.ChangeTurn();
            }
            else
            {
                PlayerDiceResults.Clear();
                GameManager.INSTANCE.ShouldChangeTurn = true;
                GameManager.INSTANCE.ChangeTurn();
            }
        }
        
        // entry case when 1 play is in play
        private void PawnEnterCaseOne()
        {
            if (PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5)
            {
                PlayerDiceResults.RemoveAt(1);
                EnterPawnOnBoard(_myPawns[0]);
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    PlayerDiceResults.Clear();
                    EnterPawnOnBoard(_myPawns[0]);
                }
                else
                {
                    MakePawnPlay(PlayerDiceResults[0]);
                }
            }
            else if (PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                PlayerDiceResults.Remove(5);
                EnterPawnOnBoard(_myPawns[0]);
                MakePawnPlay(PlayerDiceResults[0]);
            }
            else if (PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                PlayerDiceResults.Clear();
                EnterPawnOnBoard(_myPawns[0]);
            }
            else if (PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
        
            }
            else
            {
                MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
            }
        }
        
        // entry case when 2 pawns are in play
        private void PawnEnterCaseTwo()
        {
            if (PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count == 0)
                {
                    PlayerDiceResults.Clear();
                    EnterPawnOnBoard(_myPawns[0]);
                    EnterPawnOnBoard(_myPawns[0]);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 1)
                {
                    PlayerDiceResults.RemoveAt(1);
                    EnterPawnOnBoard(_myPawns[0]);
                    MakePawnPlay(5);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    PlayerDiceResults.RemoveAt(1);
                    MoveActivePawn(_enteredPawns[0], 5);
                    EnterPawnOnBoard(_myPawns[0]);
                }
            }
            else if (PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    PlayerDiceResults.Remove(5);
                    EnterPawnOnBoard(_myPawns[0]);
                    MakePawnPlay(PlayerDiceResults[0]);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    int nonFiveDiceResult = PlayerDiceResults[0] == 5 ? PlayerDiceResults[1] : PlayerDiceResults[0];
        
                    // moving the active pawn using the non-5 dice result
                    MoveActivePawn(_enteredPawns[0], nonFiveDiceResult);
                    PlayerDiceResults.Remove(nonFiveDiceResult);
                    PlayerDiceResults.Clear();
                    EnterPawnOnBoard(_myPawns[0]);
                }
            }
            else if (PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    PlayerDiceResults.Clear();
                    EnterPawnOnBoard(_myPawns[0]);
                }
                else
                {
                    MakePawnPlay(PlayerDiceResults[0] + PlayerDiceResults[1]);
                }
            }
            else if (PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
            }
            else
            {
                GameManager.INSTANCE.ShouldChangeTurn = true;
                MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
            }
        }
        
        // entry case when 3 pawns are in play
        private void PawnEnterCaseThree()
        {
            if (PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count == 1)
                {
                    PlayerDiceResults.Remove(5);
                    EnterPawnOnBoard(_myPawns[0]);
                    MakePawnPlay(PlayerDiceResults[0]);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    PlayerDiceResults.Remove(5);
                    MoveActivePawn(PawnPath[0].PawnsOnNode[0], 5);
                    PlayerDiceResults.Clear();
                    EnterPawnOnBoard(_myPawns[0]);
                }
                else
                {
                    MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
                }
            }
            else if (PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    PlayerDiceResults.Remove(5);
                    EnterPawnOnBoard(_myPawns[0]);
                    MakePawnPlay(PlayerDiceResults[0]);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    int nonFiveDiceResult = PlayerDiceResults[0] == 5 ? PlayerDiceResults[1] : PlayerDiceResults[0];
        
                    // moving the active pawn using the non-5 dice result
                    // if three pawns are already in game and two pawns are at start move one of the pawns
                    MoveActivePawn(PawnPath[0].PawnsOnNode[1], nonFiveDiceResult);
                    PlayerDiceResults.Remove(nonFiveDiceResult);
                    PlayerDiceResults.Clear();
                    EnterPawnOnBoard(_myPawns[0]);
                }
            }
            else if (PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    PlayerDiceResults.Clear();
                    EnterPawnOnBoard(_myPawns[0]);
                }
                else
                {
                    MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
                }
            }
            else if (PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
            }
            else
            {
                MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
            }
        }

        #endregion
        
        #region MyRegion2

        // private enum DiceCondition
        // {
        //     DoubleFive,
        //     SingleFive,
        //     SumFive,
        //     Doubles,
        //     Other
        // }
        //
        // public void MakePawnEnterBoard()
        // {
        //     // if (_pawnsInPlay < 0 || _pawnsInPlay > 3)
        //     // {
        //     //     Debug.Log($"Unexpected _pawnsInPlay value: {_pawnsInPlay}");
        //     //     return;
        //     // }
        //
        //     var condition = GetDiceCondition();
        //
        //     switch (condition)
        //     {
        //         case DiceCondition.DoubleFive:
        //             HandleDoubleFive();
        //             break;
        //         case DiceCondition.SingleFive:
        //             HandleSingleFive();
        //             break;
        //         case DiceCondition.SumFive:
        //             HandleSumFive();
        //             break;
        //         case DiceCondition.Doubles:
        //             HandleDoubles();
        //             break;
        //         case DiceCondition.Other:
        //             HandleOtherCase();
        //             break;
        //     }
        // }
        //
        // private DiceCondition GetDiceCondition()
        // {
        //     if (PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5) return DiceCondition.DoubleFive;
        //     if (PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5) return DiceCondition.SingleFive;
        //     if (PlayerDiceResults[0] + PlayerDiceResults[1] == 5) return DiceCondition.SumFive;
        //     if (PlayerDiceResults[0] == PlayerDiceResults[1]) return DiceCondition.Doubles;
        //     return DiceCondition.Other;
        // }
        //
        // private void HandleDoubleFive()
        // {
        //     switch (_pawnsInPlay)
        //     {
        //         case 0:
        //             PlayerDiceResults.Clear();
        //             EnterPawnOnBoard(_myPawns[0]);
        //             EnterPawnOnBoard(_myPawns[0]);
        //             break;
        //
        //         case 1:
        //             PlayerDiceResults.RemoveAt(1);
        //             EnterPawnOnBoard(_myPawns[0]);
        //             if (PawnPath[0].PawnsOnNode.Count != 2)
        //             {
        //                 PlayerDiceResults.Clear();
        //                 EnterPawnOnBoard(_myPawns[0]);
        //             }
        //             else
        //             {
        //                 MakePawnPlay(PlayerDiceResults[0]);
        //             }
        //
        //             break;
        //
        //         case 2:
        //             HandleDoubleFiveWithTwoPawns();
        //             break;
        //
        //         case 3:
        //             HandleDoubleFiveWithThreePawns();
        //             break;
        //     }
        // }
        //
        // private void HandleDoubleFiveWithTwoPawns()
        // {
        //     if (PawnPath[0].PawnsOnNode.Count == 0)
        //     {
        //         PlayerDiceResults.Clear();
        //         EnterPawnOnBoard(_myPawns[0]);
        //         EnterPawnOnBoard(_myPawns[0]);
        //     }
        //     else if (PawnPath[0].PawnsOnNode.Count == 1)
        //     {
        //         PlayerDiceResults.RemoveAt(1);
        //         EnterPawnOnBoard(_myPawns[0]);
        //         MakePawnPlay(5);
        //     }
        //     else if (PawnPath[0].PawnsOnNode.Count == 2)
        //     {
        //         PlayerDiceResults.RemoveAt(1);
        //         MoveActivePawn(_enteredPawns[0], 5);
        //         EnterPawnOnBoard(_myPawns[0]);
        //     }
        // }
        //
        // private void HandleDoubleFiveWithThreePawns()
        // {
        //     if (PawnPath[0].PawnsOnNode.Count == 1)
        //     {
        //         PlayerDiceResults.Remove(5);
        //         EnterPawnOnBoard(_myPawns[0]);
        //         MakePawnPlay(PlayerDiceResults[0]);
        //     }
        //     else if (PawnPath[0].PawnsOnNode.Count == 2)
        //     {
        //         PlayerDiceResults.Remove(5);
        //         MoveActivePawn(PawnPath[0].PawnsOnNode[0], 5);
        //         PlayerDiceResults.Clear();
        //         EnterPawnOnBoard(_myPawns[0]);
        //     }
        //     else
        //     {
        //         MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
        //     }
        // }
        //
        // private void HandleSingleFive()
        // {
        //     int nonFiveDice = PlayerDiceResults[0] == 5 ? PlayerDiceResults[1] : PlayerDiceResults[0];
        //
        //     if (_pawnsInPlay == 0)
        //     {
        //         PlayerDiceResults.Remove(5);
        //         EnterPawnOnBoard(_myPawns[0]);
        //         MoveActivePawn(_enteredPawns[0], PlayerDiceResults[0]);
        //         return;
        //     }
        //
        //     if (PawnPath[0].PawnsOnNode.Count != 2)
        //     {
        //         PlayerDiceResults.Remove(5);
        //         EnterPawnOnBoard(_myPawns[0]);
        //         MakePawnPlay(PlayerDiceResults[0]);
        //     }
        //     else
        //     {
        //         var pawnToMove = _pawnsInPlay == 3 ? PawnPath[0].PawnsOnNode[1] : _enteredPawns[0];
        //         MoveActivePawn(pawnToMove, nonFiveDice);
        //         PlayerDiceResults.Remove(nonFiveDice);
        //         PlayerDiceResults.Clear();
        //         EnterPawnOnBoard(_myPawns[0]);
        //     }
        // }
        //
        // private void HandleSumFive()
        // {
        //     PlayerDiceResults.Clear();
        //     if (_pawnsInPlay == 0 || (_pawnsInPlay >= 1 && PawnPath[0].PawnsOnNode.Count != 2))
        //     {
        //         EnterPawnOnBoard(_myPawns[0]);
        //     }
        //     else
        //     {
        //         MakePawnPlay(PlayerDiceResults[0] + PlayerDiceResults[1]);
        //     }
        // }
        //
        // private void HandleDoubles()
        // {
        //     if (_pawnsInPlay == 0)
        //     {
        //         PlayerDiceResults.Clear();
        //         GameManager.INSTANCE.ShouldChangeTurn = false;
        //         GameManager.INSTANCE.ChangeTurn();
        //     }
        //     else
        //     {
        //         MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
        //     }
        // }
        //
        // private void HandleOtherCase()
        // {
        //     if (_pawnsInPlay == 0)
        //     {
        //         PlayerDiceResults.Clear();
        //         GameManager.INSTANCE.ShouldChangeTurn = true;
        //         GameManager.INSTANCE.ChangeTurn();
        //         return;
        //     }
        //
        //     if (_pawnsInPlay == 2)
        //     {
        //         GameManager.INSTANCE.ShouldChangeTurn = true;
        //     }
        //
        //     MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
        // }
        #endregion

        // checks if pawn can make play and enables pawn accordingly
        public void MakePawnPlay(int moveStep)
        {
            // track movable pawns
            bool anyPawnCanMove = false;
            int movablePawnCount = 0;
            Pawn firstMovablePawn = null;

            // checking all entered pawns if they can move
            foreach (Pawn pawn in _enteredPawns)
            {
                if (pawn.CanPawnMove(moveStep))
                {
                    pawn.IsPawnMovable = true;
                    movablePawnCount++;
                    anyPawnCanMove = true;
                    firstMovablePawn = pawn; // tracking the first movable pawn
                }
            }

            // if no pawn can move change turn and exit
            if (!anyPawnCanMove)
            {
                // GameManager.INSTANCE.ChangeTurn(true);
                return;
            }

            // if only one pawn is movable move it directly
            if (movablePawnCount == 1)
            {
                firstMovablePawn?.MovePawn(moveStep);
                Debug.Log("here");
            }
            else
            {
                // Show options for all movable pawns
                foreach (Pawn pawn in _enteredPawns)
                {
                    if (pawn.IsPawnMovable)
                    {
                        pawn.ShowPawnOption();
                        pawn.EnableCollider();
                    }
                }
            }
        }

        // checks if pawn can play the two dice results and enables pawns accordingly
        // public void MakePawnPlayTwoSteps(int moveStep1, int moveStep2)
        // {
        //     MakePawnPlay(moveStep1);
        //     MakePawnPlay(moveStep2);
        //
        //     bool allPawnsNotMovable = true;
        //
        //     foreach (Pawn p in _enteredPawns)
        //     {
        //         if (p.IsPawnMovable)
        //         {
        //             allPawnsNotMovable = false; // found at least one movable pawn
        //             break;
        //         }
        //     }
        //
        //     if (allPawnsNotMovable)
        //     {
        //         GameManager.INSTANCE.ShouldChangeTurn = true;
        //         GameManager.INSTANCE.ChangeTurn();
        //     }
        // }
        
        public void MakePawnPlayTwoSteps(int moveStep1, int moveStep2)
        {
            Pawn firstMovablePawn = null;
            int movablePawnCount = 0;
    
            // Single pass through pawns to check movement possibilities and count
            foreach (Pawn pawn in _enteredPawns)
            {
                if (pawn.CanPawnMove(moveStep1) || pawn.CanPawnMove(moveStep2))
                {
                    pawn.IsPawnMovable = true;
                    firstMovablePawn = firstMovablePawn ?? pawn;
                    movablePawnCount++;
                }
            }

            // Handle no movable pawns
            if (movablePawnCount == 0)
            {
                GameManager.INSTANCE.ShouldChangeTurn = true;
                GameManager.INSTANCE.ChangeTurn();
                return;
            }

            // Handle single movable pawn
            if (movablePawnCount == 1)
            {
                // Try moveStep1 first, then moveStep2
                if (firstMovablePawn.CanPawnMove(moveStep1))
                {
                    firstMovablePawn.MovePawn(moveStep1);
                }
                else
                {
                    firstMovablePawn.MovePawn(moveStep2);
                }
                return;
            }

            // Handle multiple movable pawns
            foreach (Pawn pawn in _enteredPawns)
            {
                if (pawn.IsPawnMovable)
                {
                    pawn.ShowPawnOption();
                    pawn.EnableCollider();
                }
            }
        }

        private void TwoStepPlay(int move1, int move2)
        {
            foreach (Pawn p in _enteredPawns)
            {
                
            }
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
                pawn.MovePawn(steps);
                // PlayerDiceResults.Remove(steps);
            }
            else
            {
                MakePawnPlay(steps);
            }
        }

        //enters the pawn into entered pawns list and removes it from my pawns(initial) list
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

        // get previous node for the pawn
        // used when pawn is killed and needs to return to home
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

        public void BonusMovePlay(int bonusMoveStep)
        {
            if (!HasBonusMove) return;

            int movablePawnCount = 0;
            Pawn movablePawn = null;

            // checking how many pawns can move
            foreach (Pawn p in _enteredPawns)
            {
                if (p.CanPawnMove(bonusMoveStep))
                {
                    movablePawnCount++;
                    movablePawn = p; // track the last movable pawn
                }
            }

            // if only one pawn is movable move it directly
            if (movablePawnCount == 1)
            {
                Debug.Log("herere");
                GameManager.INSTANCE.ShouldChangeTurn = true;
                MoveActivePawn(movablePawn, bonusMoveStep);
                HasBonusMove = false;
                BonusMove = 0;
            }
            // if more than one pawn is movable show options
            else if (movablePawnCount > 1)
            {
                Debug.Log("else here");
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
    }
}