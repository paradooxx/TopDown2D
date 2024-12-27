using System.Collections;
using System.Collections.Generic;
using _Scripts.Enums;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager INSTANCE;
        
        public Player.Player[] Players;
        public Player.Player CurrentPlayer;
        [SerializeField] private List<Player.Player> StartingPlayers;

        private readonly Transform[] _playerDicePositions = new Transform[4];

        private int _currentPlayerIndex;

        public Transform MainDice;
        
        [SerializeField] private DiceManager DiceManager;

        [SerializeField] private float TurnChangeTime = 1.5f;

        private void Awake()
        {
            if(INSTANCE == null)
            {
                INSTANCE = this;
            }
            else
            {
                Destroy(gameObject);
            }
            InitializeGame();
        }

        private void OnEnable()
        {
            DiceManager.OnDiceRollFinished += EnablePlayerPlay;
        }

        private void OnDisable()
        {
            DiceManager.OnDiceRollFinished -= EnablePlayerPlay;
        }

        private void InitializeGame()
        {
            _currentPlayerIndex = 0;
            StartingPlayers = new List<Player.Player>();

            for (int i = 0; i < Players.Length; i++)
            {
                if (Players[i].PlayerType == PlayerType.PLAYER || Players[i].PlayerType == PlayerType.BOT)
                {
                    StartingPlayers.Add(Players[i]);
                }
                
            }

            for (int i = 0; i < StartingPlayers.Count; i++)
            {
                StartingPlayers[i].IsMyTurn = i == _currentPlayerIndex;
                StartingPlayers[i].gameObject.SetActive(true);
            }
            
            CurrentPlayer = StartingPlayers[_currentPlayerIndex];
            SetDicePosition(CurrentPlayer);
        }

        public void RollDice()
        {
            CurrentPlayer.PlayerDiceResults.Clear();
            DiceManager.RollDice((dice1Result, dice2Result) => 
            {
                CurrentPlayer.PlayerDiceResults.Add(dice1Result);
                CurrentPlayer.PlayerDiceResults.Add(dice2Result);

                EnablePlayerPlay();
            });
        }

        

        private void GetDiceResult()
        {
            CurrentPlayer.PlayerDiceResults.Add(DiceManager.Dice1Result);
            CurrentPlayer.PlayerDiceResults.Add(DiceManager.Dice2Result);
        }

        // setting position of the dice based on current player
        private void SetDicePosition(Player.Player currentPlayer)
        {
            if (currentPlayer == StartingPlayers[_currentPlayerIndex])
            {
                MainDice.position = currentPlayer.MyDicePosition.position;
            }
        }

        private void EnablePlayerPlay()
        {
            if (CurrentPlayer._enteredPawns.Count < 4)
            {
                CurrentPlayer.MakePawnEnterBoard();
                CurrentPlayer._pawnsInPlay = CurrentPlayer._enteredPawns.Count;
            }
            else
            {
                if (CurrentPlayer.PlayerDiceResults[0] == CurrentPlayer.PlayerDiceResults[1])
                {
                    CurrentPlayer.ChangeTurn = false;
                    Debug.Log(CurrentPlayer.ChangeTurn);
                    Debug.Log(CurrentPlayer.ResetDice);
                }
                else
                {
                    CurrentPlayer.ChangeTurn = true;
                    Debug.Log(CurrentPlayer.ChangeTurn);
                    Debug.Log(CurrentPlayer.ResetDice);
                }
                CurrentPlayer.MakePawnPlayTwoSteps(CurrentPlayer.PlayerDiceResults[0], CurrentPlayer.PlayerDiceResults[1]);
            }
        }

        public void ReRollDice()
        {
            RepeatTurn = true;
            StartCoroutine(ChangeTurnCoroutine(RepeatTurn, true));
        }

        public void ChangeTurn(bool changeTurn, bool resetDice = true)
        {
            RepeatTurn = changeTurn;
            StartCoroutine(ChangeTurnCoroutine(RepeatTurn, resetDice));
        }
        

        public bool RepeatTurn;
        private IEnumerator ChangeTurnCoroutine(bool changeTurn, bool resetDice)
        {
            yield return new WaitForSeconds(TurnChangeTime);
            // checking for victory
            CheckForVictory();
            
            // changing the current player
            if (changeTurn)
            {
                CurrentPlayer.PlayerDiceResults.Clear();
                foreach (Pawn p in CurrentPlayer._enteredPawns)
                {
                    p.HidePawnOption();
                    p.DisableCollider();
                }
                _currentPlayerIndex = (_currentPlayerIndex + 1) % StartingPlayers.Count;
            }

            RepeatTurn = false;

            CurrentPlayer = StartingPlayers[_currentPlayerIndex];
            
            for (int i = 0; i < StartingPlayers.Count; i++)
            {
                StartingPlayers[i].IsMyTurn = i == _currentPlayerIndex;
            }
            
            SetDicePosition(CurrentPlayer);
            DiceManager.ResetDice();
        }

        public void CheckForVictory()
        {
            // if last pawn node of current player has 4 paws current player wins
            if (CurrentPlayer.PawnPath[^1].PawnsOnNode.Count != 4)
            {
                return;
            }
            else if (CurrentPlayer.PawnPath[^1].PawnsOnNode.Count == 4)
            {
                // currentPlayer is 1st
                // remove currentPlayer from starting players
                // continue the game
                // Debug.Log("Victory");
                CurrentPlayer.DisableMyPawns();
                StartingPlayers.Remove(CurrentPlayer);
                _currentPlayerIndex = -1;
                //remove these lines later
                _currentPlayerIndex = (_currentPlayerIndex + 1) % StartingPlayers.Count;
                CurrentPlayer = StartingPlayers[_currentPlayerIndex];
                for (int i = 0; i < StartingPlayers.Count; i++)
                {
                    StartingPlayers[i].IsMyTurn = i == _currentPlayerIndex;
                }
            
                SetDicePosition(CurrentPlayer);
                DiceManager.ResetDice();
            }
        }
    }
}