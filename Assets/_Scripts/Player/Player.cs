using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Board;
using _Scripts.Enums;
using _Scripts.Managers;
using Unity.VisualScripting;
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

        private int _pawnsInPlay;

        public event Action OnPawnMoveComplete;

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

        public int PawnInPlayCount()
        {
            foreach (var pawn in _myPawns)
            {
                if (pawn.IsInPlay)
                {
                    _pawnsInPlay++;
                }
            }
            return _pawnsInPlay;
        }

        public void MakePawnEnterBoard()
        {
            switch(PawnInPlayCount())
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

        private void PawnEnterCaseZero()
        {
            if(PlayerDiceResults[0] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                MoveActivePawn(_enteredPawns[0], PlayerDiceResults[1]);
            }
            else if(PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                MoveActivePawn(_enteredPawns[0], PlayerDiceResults[0]);
            }
            else if(PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                PlayerDiceResults.Clear();
            }
            else if(PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                EnterPawnOnBoard(_myPawns[0]);
                //ReRoll();
            }
            else if(PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                //ReRoll();
            }
            else
            {
                //ChangeTurn();
            }
        }

        private void PawnEnterCaseOne()
        {
            if(PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                MakePawnPlay();
            }
            else if(PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                EnterPawnOnBoard(_myPawns[0]);
                PlayerDiceResults.Clear();
            }
            else if(PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5)
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
                //ReRoll();
            }
            else if(PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                MoveActivePawn(_enteredPawns[0], PlayerDiceResults[0] + PlayerDiceResults[1]);
                //ReRoll();
            }
            else
            {
                MoveActivePawn(_enteredPawns[0], PlayerDiceResults[0] + PlayerDiceResults[1]);
                //ChangeTurn();
            }
        }

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
                //ReRoll();
            }
            else if(PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                MakePawnPlay();
                //ReRoll();
            }
            else
            {
                MakePawnPlay();
                //ChangeTurn();
            }
        }

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
                //ReRoll();
            }
            else if(PlayerDiceResults[0] == PlayerDiceResults[1])
            {
                MakePawnPlay();
                //ReRoll();
            }
            else
            {
                MakePawnPlay();
                //ChangeTurn();
            }
        }

        private void PawnPlay()
        {
            StartCoroutine(PawnPlayCo());
        }

        private IEnumerator PawnPlayCo()
        {
            yield return new WaitForSeconds(1f);
            if (PlayerDiceResults.Count == 1 && _enteredPawns.Count == 1)
            {
                MoveActivePawn(_enteredPawns[0], PlayerDiceResults[0]);
            }
            else if (PlayerDiceResults.Count == 1 && _enteredPawns.Count > 1)
            {
                foreach (Pawn p in _enteredPawns)
                {
                    p.EnableCollider();
                }
            }
            else
            {
                TurnComplete();
            }
        }

        public void MakePawnPlay()
        {
            foreach (Pawn p in _enteredPawns)
            {
                p.ShowPawnOption();
                p.EnableCollider();
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
            for (int i = 0; i < _myPawns.Count; i++)
            {
                _myPawnsColliders[i].enabled = false;
            }
        }

        private void MoveActivePawn(Pawn pawn, int steps)
        {
            pawn.MovePawn(steps);
            // if(pawn.CanPawnMove(steps))
            //     pawn.MovePawn(steps);
            // else
            // {
            //     Debug.Log("Cannot move pawn");
            //     // change turn or something
            // }
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

        private void BonusMove()
        {
            // if kill other player, 20
            // if a pawn reaches finish, 10
        }

        private void TurnComplete()
        {
            StartCoroutine(ReRollDice());
        }

        private void ShowPawnOption()
        {
            foreach(Pawn p in _enteredPawns)
            {
                p.ShowPawnOption();
                p.EnableCollider();
            }
        }

        private IEnumerator ReRollDice()
        {
            yield return new WaitForSeconds(1f);
            GameManager.INSTANCE.ChangeTurn();
        }

        private bool MyPawnKilledOtherPawn()
        {
            return false;
        }
    }
}