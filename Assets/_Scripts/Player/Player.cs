using System.Collections.Generic;
using System.Linq;
using _Scripts.Board;
using _Scripts.Enums;
using _Scripts.Managers;
using _Scripts.UI;
using TMPro;
using UnityEngine;

namespace _Scripts.Player
{
    public class Player : MonoBehaviour
    {
        public PlayerType PlayerType;
        public List<Node> PawnPath; // fixed path where this player's pawn can move
        public List<Transform> HomePosition; // home position's for this player's pawns
        private readonly GameObject[] _instantiatedPawns = new GameObject[4]; // list of this player's pawn
        public List<DiceState> DiceStates = new List<DiceState>();

        public List<Pawn> AllPawns = new List<Pawn>();
        public List<Pawn> HomePawns = new List<Pawn>();
        public List<Pawn> EnteredPawns = new List<Pawn>();
        public List<Collider2D> HomePawnsColliders = new List<Collider2D>();

        [SerializeField] private GameObject PawnPrefab;
        [SerializeField] private Sprite PawnSprite;

        public Sprite[] DiceResultSprites;

        public PawnManager PawnManager;
        public DiceManager DiceManager;
        public Transform MyDicePosition;
        public GameManager GameManager;
        public PlayerCanvaManager PlayerCanvaManager;
        
        public readonly Queue<int> PreviousDiceSum = new Queue<int>();

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
            InitializeHomePawns();
            PlayerStateManager.LoadGameState(this);
        }

        public void NewGameStart()
        {
            InitializeHomePawns();
        }

        // setting this player's pawns in the respective home positions
        private void InitializeHomePawns()
        {
            for (int i = 0; i < 4; i++)
            {
                _instantiatedPawns[i] = Instantiate(PawnPrefab, HomePosition[i].position, Quaternion.identity);
                AllPawns.Add(_instantiatedPawns[i].GetComponent<Pawn>());
                HomePawns.Add(_instantiatedPawns[i].GetComponent<Pawn>());
                HomePawns[i].HomePosition = HomePosition[i];
                HomePawns[i].PawnMainSprite.sprite = PawnSprite;
                HomePawns[i].MainPlayer = this;
                HomePawnsColliders.Add(HomePawns[i].GetComponent<Collider2D>());
                HomePawnsColliders[i].enabled = false;
            }
        }

 
        #region PawnEnterRegion

        public void MakePawnEnterBoard()
        {
            switch (PawnsInPlay)
            {
                case 0:
                    Debug.Log("pawn entry case 0");
                    PawnEnterCaseZero();
                    break;
                case 1:
                    Debug.Log("pawn entry case 1");
                    PawnEnterCaseOne();
                    break;
                case 2:
                    Debug.Log("pawn entry case 2");
                    PawnEnterCaseTwo();
                    break;
                case 3:
                    Debug.Log("pawn entry case 3");
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
            if (DiceStates[0].Value == 5 && DiceStates[1].Value == 5)
            {
                ResetDiceStates();
                EnterPawnOnBoard(2);
            }
            else if (DiceStates[0].Value + DiceStates[1].Value == 5)
            {
                ResetDiceStates();
                EnterPawnOnBoard(1);
            }
            else if (DiceStates[0].Value == 5 || DiceStates[1].Value == 5)
            {
                EnterPawnOnBoard(1);
            }
            else
            {
                ResetDiceStates();
                GameManager.ChangeTurn();
            }
        }

        // entry case when 1 play is in play
        private void PawnEnterCaseOne()
        {
            if (DiceStates[0].Value == 5 && DiceStates[1].Value == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count == 0)
                {
                    EnterPawnOnBoard(2);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 1)
                {
                    EnterPawnOnBoard(1);
                }
            }
            else if (DiceStates[0].Value == 5 || DiceStates[1].Value == 5)
            {
                EnterPawnOnBoard(1);
            }
            else if (DiceStates[0].Value + DiceStates[1].Value == 5)
            {
                ResetDiceStates();
                EnterPawnOnBoard(1);
            }
            else
            {
                MakePawnPlayTwoSteps();
            }
        }

        // entry case when 2 pawns are in play
        private void PawnEnterCaseTwo()
        {
            if (DiceStates[0].Value == 5 && DiceStates[1].Value == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count == 0)
                {
                    EnterPawnOnBoard(2);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 1)
                {
                    EnterPawnOnBoard(1);
                    // MakePawnPlay(5);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    MoveActivePawn(EnteredPawns[0], 5);
                    EnterPawnOnBoard(1);
                }
            }
            else if (DiceStates[0].Value == 5 || DiceStates[1].Value == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    EnterPawnOnBoard(1);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    int nonFiveDiceResult = DiceStates[0].Value == 5 ? DiceStates[1].Value : DiceStates[0].Value;

                    // moving the active pawn using the non-5 dice result
                    MoveActivePawn(EnteredPawns[0], nonFiveDiceResult);
                    EnterPawnOnBoard(1);
                }
            }
            else if (DiceStates[0].Value + DiceStates[1].Value == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    ResetDiceStates();
                    EnterPawnOnBoard(1);
                }
                else
                {
                    MakePawnPlayTwoSteps();
                }
            }
            else
            {
                MakePawnPlayTwoSteps();
            }
        }

        // entry case when 3 pawns are in play
        private void PawnEnterCaseThree()
        {
            if (DiceStates[0].Value == 5 && DiceStates[1].Value == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count == 1)
                {
                    EnterPawnOnBoard(1);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    MoveActivePawn(EnteredPawns[0], 5);
                    EnterPawnOnBoard(1);
                }
                else
                {
                    EnterPawnOnBoard(1);
                }
            }
            else if (DiceStates[0].Value == 5 || DiceStates[1].Value == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    EnterPawnOnBoard(1);
                }
                else if (PawnPath[0].PawnsOnNode.Count == 2)
                {
                    int nonFiveDiceResult = DiceStates[0].Value == 5 ? DiceStates[1].Value : DiceStates[0].Value;

                    // moving the active pawn using the non-5 dice result
                    // if three pawns are already in game and two pawns are at start move one of the pawns
                    MoveActivePawn(PawnPath[0].PawnsOnNode[1], nonFiveDiceResult);
                    EnterPawnOnBoard(1);
                }
            }
            else if (DiceStates[0].Value + DiceStates[1].Value == 5)
            {
                if (PawnPath[0].PawnsOnNode.Count != 2)
                {
                    ResetDiceStates();
                    EnterPawnOnBoard(1);
                }
                else
                {
                    MakePawnPlayTwoSteps();
                }
            }
            else
            {
                MakePawnPlayTwoSteps();
            }
        }

        #endregion

        // checks if pawn can make play and enables pawn accordingly
        // for single move
        public void MakePawnPlay()
        {
            int moveStep = DiceStates.Find(d => d.diceState == true).Value;
            // track movable pawns
            bool anyPawnCanMove = false;
            int movablePawnCount = 0;
            Pawn firstMovablePawn = null;

            // checking all entered pawns if they can move
            foreach (Pawn pawn in EnteredPawns)
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
                foreach (Pawn pawn in EnteredPawns)
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
        public void MakePawnPlayTwoSteps()
        {
            Pawn firstMovablePawn = null;
            int movablePawnCount = 0;

            // Single pass through pawns to check movement possibilities and count
            foreach (Pawn pawn in EnteredPawns)
            {
                if (pawn.CanPawnMove(DiceStates[0].Value) || pawn.CanPawnMove(DiceStates[1].Value))
                {
                    if (pawn.CanPawnMove(DiceStates[0].Value)) pawn.AvailableMovesText.text = DiceStates[0].Value.ToString();
                    else if (pawn.CanPawnMove(DiceStates[1].Value)) pawn.AvailableMovesText.text = DiceStates[1].Value.ToString();

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
                if (DiceStates[0].Value >= DiceStates[1].Value)
                {
                    firstMovablePawn.MovePawn(firstMovablePawn.CanPawnMove(DiceStates[0].Value) ? DiceStates[0].Value : DiceStates[1].Value);
                }
                else
                {
                    firstMovablePawn.MovePawn(firstMovablePawn.CanPawnMove(DiceStates[1].Value) ? DiceStates[1].Value : DiceStates[0].Value);
                }

                return;
            }

            // handling multiple movable pawns
            foreach (Pawn pawn in EnteredPawns)
            {
                if (PlayerType == PlayerType.BOT)
                {
                    MyPlayerBot.MakeBotPlayTwoSteps();
                    return;
                }
                
                if (pawn.CanPawnMove(DiceStates[0].Value) && pawn.CanPawnMove(DiceStates[1].Value))
                {
                    pawn.AvailableMovesText.text = DiceStates[0].Value + "," + DiceStates[1].Value;
                }

                if (pawn.IsPawnMovable)
                {
                    pawn.ShowPawnOption();
                    pawn.EnableCollider();
                }
            }
        }

        // enabling colliders of this player's pawns
        public void EnableHomePawns()
        {
            for (int i = 0; i < HomePawns.Count; i++)
            {
                if (HomePawns[i].IsInPlay)
                    HomePawnsColliders[i].enabled = true;
            }
        }

        public void DisableHomePawns()
        {
            foreach (var t in EnteredPawns)
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
                // DiceManager.GetActiveDiceCount().Remove(steps);
            }
            else
            {
                MakePawnPlay();
            }
        }

        //enters the pawn into entered pawns list and removes it from my pawns(initial) list
        private void EnterPawnOnBoard(int numberOfPawns)
        {
            while (numberOfPawns > 0)
            {
                numberOfPawns--;

                HomePawns[numberOfPawns].EnterBoard();
            }
        }

        public void BonusMovePlay(int bonusMoveStep)
        {
            if (!HasBonusMove) return;

            int movablePawnCount = 0;
            Pawn movablePawn = null;

            // checking how many pawns can move
            foreach (Pawn p in EnteredPawns)
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
                foreach (Pawn p in EnteredPawns)
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
            //update ui for getting turn
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
            List<DiceState> unReviewedDices = new List<DiceState>(CreateRandomDice());
            
            // filter and change these unreviewed dice states 
            // do dice manipulation here 
            unReviewedDices = CheckFor3SameSum(unReviewedDices);

            if (unReviewedDices[1].Value == unReviewedDices[0].Value && PawnsInPlay < 2)
            {
                unReviewedDices = CreateUnequalDices();
            }
            
            DiceStates.Clear();
            DiceStates.Add(new DiceState(unReviewedDices[0].Value, true));
            DiceStates.Add(new DiceState(unReviewedDices[1].Value, true));
            
            //set dice states in dice manager 
            ShouldChangeTurn = unReviewedDices[0].Value != unReviewedDices[1].Value;

            PlayerStateManager.SaveGameState(GameManager.Players, GameManager);
            
            StartCoroutine(DiceManager.AnimateDiceTask(() =>
            {
                EnablePlayerPlay();
            }, unReviewedDices));
        }

        private List<DiceState> CreateRandomDice()
        {
            List<DiceState> diceStates = new List<DiceState>();
            diceStates.Add(new DiceState(Random.Range(1, 7), true));
            diceStates.Add(new DiceState(Random.Range(1, 7), true));
            return diceStates;
        }

        private List<DiceState> CreateUnequalDices()
        {
            List<DiceState> diceStates = new List<DiceState>();
            diceStates.Add(new DiceState(Random.Range(1, 7), true));
            int unequalValue = Random.Range(1, 7);
            
            if (unequalValue == diceStates[0].Value)
            {
                unequalValue = diceStates[0].Value + 1;
                if(unequalValue > 6 ) unequalValue = 1;
            }
            
            diceStates.Add(new DiceState(unequalValue, true));
            return diceStates;
        }

        List<DiceState> CheckFor3SameSum(List<DiceState> diceStates)
        {
            int currentSum = diceStates[0].Value + diceStates[1].Value;

            if (PreviousDiceSum.Count >= 2)
            {
                if (PreviousDiceSum.All(sum => sum != currentSum))
                {
                    return CreateRandomDice();
                }
            }
            
            PreviousDiceSum.Enqueue(currentSum);
            if (PreviousDiceSum.Count > 3)
            {
                PreviousDiceSum.Dequeue();
            }
            return diceStates;
        }
        
        public void AddDiceState(DiceState diceState)
        {
            DiceStates.Add(diceState);
        }

        public void DisableDiceState(int result)
        {
            var find = DiceStates.Find(x => x.Value == result);
            if (find != null) find.diceState = false;
        }

        public void DisableAllDiceStates()
        {
            DiceStates.ForEach(x => x.diceState = false);
        }
        
        public int GetActiveDiceCount()
        {
            int activeDiceCount = 0;
            foreach (var dice in DiceStates)
            {
                if (dice.diceState == true)
                {
                    activeDiceCount++;
                }
            }
            Debug.Log("Active Usable Dice Counts: " + activeDiceCount);
            return activeDiceCount;
        }

        public void ResetDiceStates()
        {
            foreach (var state in DiceStates)
            {
                state.diceState = false;
            }
        }

        public void DimDiceSprite()
        {
            for (int i = 0; i < DiceStates.Count; i++)
            {
                if (DiceStates[i].diceState == false)
                {
                    DiceManager.DimSprite(i);
                }
            }
        }
        

        private void EnablePlayerPlay()
        {
            // makes sure a pawn enters when 5
            // is drawn if it is possible
            Debug.Log("change player enabled to play");
            // if possible pawn must enter the board always
            // if there is a move to enter the pawn on board, it will be used to take a pawn into game
            if (EnteredPawns.Count < 4)
            {
                MakePawnEnterBoard();
            }
            else
            {
                MakePawnPlayTwoSteps();
            }
        }

        public void OnPawnMoveComplete()
        {
            CheckForVictory();
            if (GetActiveDiceCount() == 0)
            {
                if (HasBonusMove)
                {
                    BonusMovePlay(BonusMove);
                }
                else
                {
                    foreach (Pawn p in EnteredPawns)
                    {
                        p.DisableCollider();
                        p.HidePawnOption();
                        p.AvailableMovesText.text = "";
                        p.IsPawnClicked = false;
                    }
                    GameManager.ChangeTurn();
                }
            }
            else if (GetActiveDiceCount() == 1)
            {
                int moveSteps = DiceStates.Find(d => d.diceState == true).Value;
                if (EnteredPawns.Count > 1)
                {
                    MakePawnPlay();
                }
                else if (EnteredPawns.Count == 1)
                {
                    MoveActivePawn(EnteredPawns[0], moveSteps);
                }
            }
            
            else if (GetActiveDiceCount() == 2)
            {
                ShouldChangeTurn = DiceStates[0].Value != DiceStates[1].Value;
                MakePawnPlayTwoSteps();
            }

            foreach (Pawn p in EnteredPawns)
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