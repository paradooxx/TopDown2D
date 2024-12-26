using System.Collections;
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
        public bool RepeatTurn = false;

        public int _pawnsInPlay;
        public int bonusMove;

        private void Start()
        {
            InitializeMyPawns();
        }

        private void OnEnable()
        {
            foreach (Pawn p in _myPawns)
            {
                p.OnPawnMoveComplete += CheckForDiceResults;
            }
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
        
        public int PawnInPlayCount()
        {
            foreach (var pawn in _enteredPawns)
            {
                _pawnsInPlay++;
            }
            return _pawnsInPlay;
        }

        private void CheckForDiceResults()
        {
            if (PlayerDiceResults.Count == 0)
            {
                if (HasBonusMove())
                {
                    // check for bonus and movable pawns
                }
                else
                {
                    GameManager.INSTANCE.RepeatTurn = false;
                    GameManager.INSTANCE.ChangeTurn();
                }
            }
            else if (PlayerDiceResults.Count == 1)
            {
                foreach (Pawn p in _enteredPawns)
                {
                    if (p.CanPawnMove(PlayerDiceResults[0]))
                    {
                        p.EnableCollider();
                        p.ShowPawnOption();
                    }
                }
            }
        }

        public void MakePawnEnterBoard()
        {
            // _pawnsInPlay = _enteredPawns.Count;
            switch(_pawnsInPlay)
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
                GameManager.INSTANCE.RepeatTurn = true;
                GameManager.INSTANCE.ChangeTurn();
            }
            else if(PlayerDiceResults[0] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                MoveActivePawn(_enteredPawns[0], PlayerDiceResults[1]);
                GameManager.INSTANCE.RepeatTurn = false;
                GameManager.INSTANCE.ChangeTurn();
            }
            else if(PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                MoveActivePawn(_enteredPawns[0], PlayerDiceResults[0]);
                GameManager.INSTANCE.RepeatTurn = false;
                GameManager.INSTANCE.ChangeTurn();
            }
            else if(PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                PlayerDiceResults.Clear();
                GameManager.INSTANCE.RepeatTurn = false;
                GameManager.INSTANCE.ChangeTurn();
            }
            else if(PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                PlayerDiceResults.Clear();
                
                GameManager.INSTANCE.RepeatTurn = true;
                GameManager.INSTANCE.ChangeTurn();
            }
            else
            {
                GameManager.INSTANCE.ChangeTurn();
            }
        }

        // entry case when 1 play is in play
        private void PawnEnterCaseOne()
        {
            if(PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                if(PawnPath[0].PawnsOnNode.Count != 2)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                }
                else
                {
                    MakePawnPlay();
                }
                GameManager.INSTANCE.RepeatTurn = true;
                GameManager.INSTANCE.ChangeTurn();;//recet dice has problem in this class
            }
            else if(PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                PlayerDiceResults.Remove(5);
                MakePawnPlay();
            }
            else if(PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                PlayerDiceResults.Clear();
                GameManager.INSTANCE.RepeatTurn = false;
                GameManager.INSTANCE.ChangeTurn();
            }
            else if(PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                MoveActivePawn(_enteredPawns[0], PlayerDiceResults[0] + PlayerDiceResults[1]);
                GameManager.INSTANCE.RepeatTurn = true;
                GameManager.INSTANCE.ChangeTurn();
            }
            else
            {
                MoveActivePawn(_enteredPawns[0], PlayerDiceResults[0] + PlayerDiceResults[1]);
                GameManager.INSTANCE.RepeatTurn = false;
                GameManager.INSTANCE.ChangeTurn();
            }
        }

        // entry case when 2 pawns are in play
        private void PawnEnterCaseTwo()
        {
            if(PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                if(PawnPath[0].PawnsOnNode.Count != 2)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.Remove(5);
                    MakePawnPlay();
                }
                else if(PawnPath[0].PawnsOnNode.Count == 2)
                {
                    MakePawnPlay();
                }
            }
            else if(PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                if(PawnPath[0].PawnsOnNode.Count != 2)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.Clear();
                    GameManager.INSTANCE.RepeatTurn = false;
                    GameManager.INSTANCE.ChangeTurn();
                }
                else
                {
                    MakePawnPlay();
                }
            }
            else if(PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5)
            {
                if(PawnPath[0].PawnsOnNode.Count == 0)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.Clear();
                }
                else if(PawnPath[0].PawnsOnNode.Count == 1)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.RemoveAt(1);
                    MakePawnPlay();
                }
                else
                {
                    MakePawnPlay();
                }
                GameManager.INSTANCE.RepeatTurn = true;
                GameManager.INSTANCE.ChangeTurn();
            }
            else if(PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                MakePawnPlay();
                GameManager.INSTANCE.RepeatTurn = true;
                GameManager.INSTANCE.ChangeTurn();
            }
            else
            {
                MakePawnPlay();
                // GameManager.INSTANCE.ChangeTurn();
            }
        }

        // entry case when 3 pawns are in play
        private void PawnEnterCaseThree()
        {
            if(PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                if(PawnPath[0].PawnsOnNode.Count != 2)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.Remove(5);
                    MakePawnPlay();
                }
                else if(PawnPath[0].PawnsOnNode.Count == 2)
                {
                    MakePawnPlay();
                }
            }
            else if(PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                if(PawnPath[0].PawnsOnNode.Count != 2)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.Clear();
                }
                else
                {
                    MakePawnPlay();
                }
            }
            else if(PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5)
            {
                if(PawnPath[0].PawnsOnNode.Count != 2)
                {
                    EnterPawnOnBoard(_myPawns[0]);
                    PlayerDiceResults.Remove(5);
                    MakePawnPlay();
                }
                else
                {
                    MakePawnPlay();
                }
                GameManager.INSTANCE.RepeatTurn = true;
                GameManager.INSTANCE.ChangeTurn();
            }
            else if(PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                MakePawnPlay();
                GameManager.INSTANCE.RepeatTurn = true;
                GameManager.INSTANCE.ChangeTurn();
            }
            else
            {
                MakePawnPlay();
                GameManager.INSTANCE.RepeatTurn = false;
                GameManager.INSTANCE.ChangeTurn();
            }
        }

        public void MakePawnPlay()
        {
            if (PlayerDiceResults.Count == 0 && !HasBonusMove())
            {
                GameManager.INSTANCE.RepeatTurn = false;
                GameManager.INSTANCE.ChangeTurn();
                return;
            }
            
            // else if (PlayerDiceResults.Count == 1)
            
            foreach (Pawn p in _enteredPawns)
            {
                p.ShowPawnOption();
                p.EnableCollider();
            }
        }

        private void MoveActivePawn(Pawn pawn, int steps)
        {
            if(pawn.CanPawnMove(steps))
                pawn.MovePawn(steps);
            
            // pawn.MovePawn(steps);
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