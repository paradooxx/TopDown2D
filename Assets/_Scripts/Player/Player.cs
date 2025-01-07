using System.Collections.Generic;
using _Scripts.Board;
using _Scripts.Enums;
using _Scripts.Managers;
using _Scripts.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Player
{
    public class Player : MonoBehaviour
    {
        public PlayerType PlayerType;
        public List<Node> PawnPath; // fixed path where this player's pawn can move
        public List<Transform> HomePosition; // home position's for this player's pawns
        private readonly GameObject[] _instantiatedPawns = new GameObject[4]; // list of this player's pawn

        public List<Pawn> _allPawns = new List<Pawn>();
        public List<Pawn> _myPawns = new List<Pawn>();
        public List<Pawn> _enteredPawns = new List<Pawn>();
        public List<Collider2D> _myPawnsColliders = new List<Collider2D>();

        [SerializeField] private GameObject PawnPrefab;
        [SerializeField] private Sprite PawnSprite;

        public Sprite[] DiceResultSprites;

        public PawnManager PawnManager;
        public DiceManager DiceManager;
        public Transform MyDicePosition;
        public GameManager GameManager;
        public PlayerCanvaManager PlayerCanvaManager;

        public List<int> PlayerDiceResults = new List<int>(2);
        public bool IsMyTurn;
        public bool HasBonusMove;

        public int PawnsInPlay;
        public int OtherPawnKillCount = 0;
        public int BonusMove;
        
        public PlayerBot MyPlayerBot;

        public int MyIndex;
        
        public PlayerStateManager PlayerStateManager;
        public bool ShouldChangeTurn;
        
        public int WinPosition;
        
        [SerializeField] private GameObject WinScreen;
        [SerializeField] private TMP_Text RankText;

        [ContextMenu("Add Pawn Reference")]
        void AddPawnReference()
        {
            PawnManager = GetComponent<PawnManager>();
        }

        [ContextMenu("Add PlayerStateManager Reference")]
        private void AddPlayerStateManagerReference()
        {
            PlayerStateManager = GetComponent<PlayerStateManager>();
        }
        
        public void StartGame()
        {
            InitializeMyPawns();
            PlayerStateManager.LoadGameState(this);
        }

        public void NewGameStart()
        {
            InitializeMyPawns();
        }

        // setting this player's pawns in the respective home positions
        private void InitializeMyPawns()
        {
            for (int i = 0; i < 4; i++)
            {
                _instantiatedPawns[i] = Instantiate(PawnPrefab, HomePosition[i].position, Quaternion.identity);
                _allPawns.Add(_instantiatedPawns[i].GetComponent<Pawn>());
                _myPawns.Add(_instantiatedPawns[i].GetComponent<Pawn>());
                _myPawns[i].HomePosition = HomePosition[i];
                _myPawns[i].PawnMainSprite.sprite = PawnSprite;
                _myPawns[i].MainPlayer = this;
                _myPawnsColliders.Add(_myPawns[i].GetComponent<Collider2D>());
                _myPawnsColliders[i].enabled = false;
            }
            PlayerCanvaManager.AddRemovePlayer();
        }

 
        #region PawnEnterRegion

        public void MakePawnEnterBoard()
        {
            switch (PawnsInPlay)
            {
                case 0:
                    Debug.Log("change case 0");
                    PawnEnterCaseZero();
                    break;
                case 1:
                    Debug.Log("change case 1");

                    PawnEnterCaseOne();
                    break;
                case 2:
                    Debug.Log("change case 2");

                    PawnEnterCaseTwo();
                    break;
                case 3:
                    Debug.Log("change case 3");

                    PawnEnterCaseThree();
                    break;
                default:
                    Debug.Log("change case default");

                    Debug.Log("Unexpected _pawnsInPlay value: " +PawnsInPlay);
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
                EnterPawnOnBoard(2);
            }
            else if (PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                PlayerDiceResults.Clear();
                EnterPawnOnBoard(1);
            }
            else if (PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                PlayerDiceResults.Remove(5);
                EnterPawnOnBoard(1);
            }
            else
            {
                PlayerDiceResults.Clear();
                GameManager.ChangeTurn();
            }
        }

        // entry case when 1 play is in play
        private void PawnEnterCaseOne()
        {
            if (PlayerDiceResults[0] == 5 && PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count == 0)
                {
                    // PlayerDiceResults.Clear();
                    EnterPawnOnBoard(2);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 1)
                {
                    // PlayerDiceResults.Remove(5);
                    EnterPawnOnBoard(1);
                    // MakePawnPlay(PlayerDiceResults[0]);
                }
            }
            else if (PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                // PlayerDiceResults.Remove(5);
                EnterPawnOnBoard(1);
                // MakePawnPlay(PlayerDiceResults[0]);
            }
            else if (PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                PlayerDiceResults.Clear();
                EnterPawnOnBoard(1);
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
                    // PlayerDiceResults.Clear();
                    EnterPawnOnBoard(2);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 1)
                {
                    // PlayerDiceResults.RemoveAt(1);
                    EnterPawnOnBoard(1);
                    // MakePawnPlay(5);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    // PlayerDiceResults.RemoveAt(1);
                    MoveActivePawn(_enteredPawns[0], 5);
                    EnterPawnOnBoard(1);
                }
            }
            else if (PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    // PlayerDiceResults.Remove(5);
                    EnterPawnOnBoard(1);
                    // MakePawnPlay(PlayerDiceResults[0]);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    int nonFiveDiceResult = PlayerDiceResults[0] == 5 ? PlayerDiceResults[1] : PlayerDiceResults[0];

                    // moving the active pawn using the non-5 dice result
                    MoveActivePawn(_enteredPawns[0], nonFiveDiceResult);
                    // PlayerDiceResults.Remove(nonFiveDiceResult);
                    // PlayerDiceResults.Clear();      //here is the issue
                    EnterPawnOnBoard(1);
                }
            }
            else if (PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    PlayerDiceResults.Clear();
                    EnterPawnOnBoard(1);
                }
                else
                {
                    MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
                }
            }
            else
            {
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
                    // PlayerDiceResults.RemoveAt(0);
                    EnterPawnOnBoard(1);
                    // MakePawnPlay(PlayerDiceResults[0]);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    // PlayerDiceResults.RemoveAt(0);
                    MoveActivePawn(_enteredPawns[0], 5);
                    EnterPawnOnBoard(1);
                }
                else
                {
                    // PlayerDiceResults.RemoveAt(0);
                    EnterPawnOnBoard(1);
                }
            }
            else if (PlayerDiceResults[0] == 5 || PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    // PlayerDiceResults.Remove(5);
                    EnterPawnOnBoard(1);
                    // MakePawnPlay(PlayerDiceResults[0]);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    int nonFiveDiceResult = PlayerDiceResults[0] == 5 ? PlayerDiceResults[1] : PlayerDiceResults[0];

                    // moving the active pawn using the non-5 dice result
                    // if three pawns are already in game and two pawns are at start move one of the pawns
                    MoveActivePawn(PawnPath[0].PawnsOnNode[1], nonFiveDiceResult);
                    // PlayerDiceResults.Remove(nonFiveDiceResult);
                    // PlayerDiceResults.Clear();
                    EnterPawnOnBoard(1);
                }
            }
            else if (PlayerDiceResults[0] + PlayerDiceResults[1] == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    PlayerDiceResults.Clear();
                    EnterPawnOnBoard(1);
                }
                else
                {
                    MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
                }
            }
            else
            {
                MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
            }
        }

        #endregion

        // checks if pawn can make play and enables pawn accordingly
        // for single move
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
                GameManager.ChangeTurn();
                return;
            }

            // if only one pawn is movable move it directly
            if (movablePawnCount == 1)
            {
                firstMovablePawn?.MovePawn(moveStep);
            }
            else
            {
                // Show options for all movable pawns
                foreach (Pawn pawn in _enteredPawns)
                {
                    if (pawn.IsPawnMovable)
                    {
                        if (PlayerType == PlayerType.BOT)
                        {
                            MyPlayerBot.MakeBotPlay(moveStep);
                            return;
                        }
                        pawn.ShowPawnOption();
                        pawn.EnableCollider();
                        pawn.AvailableMovesText.text = moveStep.ToString();
                    }
                }
            }
        }

        // when two moves are available, use this to determine how to move the pawns
        public void MakePawnPlayTwoSteps(int moveStep1, int moveStep2)
        {
            Pawn firstMovablePawn = null;
            int movablePawnCount = 0;

            // Single pass through pawns to check movement possibilities and count
            foreach (Pawn pawn in _enteredPawns)
            {
                if (pawn.CanPawnMove(moveStep1) || pawn.CanPawnMove(moveStep2))
                {
                    if (pawn.CanPawnMove(moveStep1)) pawn.AvailableMovesText.text = moveStep1.ToString();
                    else if (pawn.CanPawnMove(moveStep2)) pawn.AvailableMovesText.text = moveStep2.ToString();

                    pawn.IsPawnMovable = true;
                    firstMovablePawn = firstMovablePawn ?? pawn;
                    movablePawnCount++;
                }
            }

            // handling no movable pawns
            
            
            if (movablePawnCount == 0)
            {
                GameManager.ChangeTurn();
                return;
            }

            if (movablePawnCount == 1)
            {
                // trying bigger step first
                if (moveStep1 >= moveStep2)
                {
                    firstMovablePawn.MovePawn(firstMovablePawn.CanPawnMove(moveStep1) ? moveStep1 : moveStep2);
                }
                else
                {
                    firstMovablePawn.MovePawn(firstMovablePawn.CanPawnMove(moveStep2) ? moveStep2 : moveStep1);
                }

                return;
            }

            // handling multiple movable pawns
            foreach (Pawn pawn in _enteredPawns)
            {
                if (PlayerType == PlayerType.BOT)
                {
                    MyPlayerBot.MakeBotPlayTwoSteps(moveStep1, moveStep2);
                    return;
                }
                
                if (pawn.CanPawnMove(moveStep1) && pawn.CanPawnMove(moveStep2))
                {
                    pawn.AvailableMovesText.text = moveStep1 + "," + moveStep2;
                }

                if (pawn.IsPawnMovable)
                {
                    pawn.ShowPawnOption();
                    pawn.EnableCollider();
                }
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
            foreach (var t in _enteredPawns)
            {
                t.enabled = false;
            }
        }

        // mostly used in pawn entry cases to move a pawn directly
        public void MoveActivePawn(Pawn pawn, int steps)
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
        private void EnterPawnOnBoard(int numberOfPawns)
        {
            while (numberOfPawns > 0)
            {
                numberOfPawns--;

                _myPawns[numberOfPawns].EnterBoard();
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

            if (movablePawnCount == 0)
            {
                // no pawns can move, just change turn
                Debug.Log("No Pawns can move : bonus move");
                GameManager.ChangeTurn();
                HasBonusMove = false;
                BonusMove = 0;
            }
            else if (movablePawnCount == 1)
            {
                // only one pawn can move so move it directly
                MoveActivePawn(movablePawn, bonusMoveStep);
                HasBonusMove = false;
                BonusMove = 0;
            }
            else
            {
                // multiple pawns can move, show options
                foreach (Pawn p in _enteredPawns)
                {
                    if (p.CanPawnMove(bonusMoveStep))
                    {
                        if (PlayerType == PlayerType.BOT)
                        {
                            HasBonusMove = false;
                            MyPlayerBot.MakeBotPlay(bonusMoveStep);
                            BonusMove = 0;
                            return;
                        }
                        p.EnableCollider();
                        p.ShowPawnOption();
                        p.AvailableMovesText.text = bonusMoveStep.ToString();
                    }
                }
            }
        }

        public void ActivateDice()
        {
            DiceManager.ActivateDice();
            DiceManager.EnableDiceTouch();
            DiceManager.ResetDiceImage();
            //check condition for player or bot also update the UIs 
        }

        public void DeactivateDice()
        {
            DiceManager.DeactivateDice();
            DiceManager.DisableDiceTouch();
        }

        public void DeactivateTurnUI()
        {
            //update ui for removing turn 
            //deactivate dices
        }

        public void ActivateTurnUI()
        {
            //update ui for gettting turn
            //active dices 
        }

        public void StartRollDice()
        {
            if (PlayerType == PlayerType.BOT)
            {
                StartCoroutine(MyPlayerBot.BotRollDiceCo());
            }
        }

        // called when player's dice is clicked
        public void RollDice()
        {
            PlayerDiceResults.Clear();
            DiceManager.RollDice((dice1Result, dice2Result) =>
            {
                ShouldChangeTurn = dice1Result != dice2Result;

                PlayerDiceResults.Add(dice1Result);
                PlayerDiceResults.Add(dice2Result);
                PlayerStateManager.SaveGameState(GameManager.Players, GameManager);
                EnablePlayerPlay();
            });
        }

        private void EnablePlayerPlay()
        {
            // makes sure a pawn enters when 5
            // is drawn if it is possible
            Debug.Log("change player enabled to play");
            // if possible pawn must enter the board always
            // if there is a move to enter the pawn on board, it will be used to take a pawn into game
            if (_enteredPawns.Count < 4)
            {
                MakePawnEnterBoard();
            }
            else
            {
                MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
            }
        }

        public void OnPawnMoveComplete()
        {
            CheckForVictory();
            if (PlayerDiceResults.Count == 0)
            {
                if (HasBonusMove)
                {
                    BonusMovePlay(BonusMove);
                }
                else
                {
                    foreach (Pawn p in _enteredPawns)
                    {
                        p.DisableCollider();
                        p.HidePawnOption();
                        p.AvailableMovesText.text = "";
                        p.IsPawnClicked = false;
                    }
                    GameManager.ChangeTurn();
                }
            }
            else if (PlayerDiceResults.Count == 1)
            {
                if (_enteredPawns.Count > 1)
                {
                    MakePawnPlay(PlayerDiceResults[0]);
                }
                else if (_enteredPawns.Count == 1)
                {
                    MoveActivePawn(_enteredPawns[0], PlayerDiceResults[0]);
                }
            }
            
            else if (PlayerDiceResults.Count == 2)
            {
                ShouldChangeTurn = PlayerDiceResults[0] != PlayerDiceResults[1];
                MakePawnPlayTwoSteps(PlayerDiceResults[0], PlayerDiceResults[1]);
            }

            foreach (Pawn p in _enteredPawns)
            {
                p.IsPawnClicked = false;
            }
            PlayerStateManager.SaveGameState(GameManager.Players, GameManager);
        }

        public void CheckForVictory()
        {
            if (PawnPath[^1].PawnsOnNode.Count == 4)
            {
                GameManager.PlayerFinishedGame(this);
                int rank = GameManager.FinishedPlayers.Count;

                if (rank == 1)
                {
                    GameStateManager.Instance.SetState(GameState.GAME_FINISHED);
                }
                VictoryScreen(rank);
            }

            if (GameManager.StartingPlayers.Count == 1)
            {
                // game finish
                GameStateManager.Instance.SetState(GameState.GAME_FINISHED);
            }
        }

        public void VictoryScreen(int rank)
        {
            WinScreen.SetActive(true);
            switch (rank)
            {
                case 1:
                    RankText.text = "1st";
                    break;
                case 2:
                    RankText.text = "2nd";
                    break;
                case 3:
                    RankText.text = "3rd";
                    break;
                default:
                    RankText.text = "4th";
                    break;
            }
        }
    }
}