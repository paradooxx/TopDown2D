using System.Collections;
using System.Collections.Generic;
using _Scripts.Enums;
using UnityEngine;
using UnityEngine.Serialization;

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
                _playerDicePositions[i] = StartingPlayers[i].MyDicePosition;
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
                MainDice.position = _playerDicePositions[_currentPlayerIndex].position;
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
                CurrentPlayer.MakePawnPlay();
            }
        }

        public void ChangeTurn()
        {
            StartCoroutine(ChangeTurnCoroutine());
        }

        public bool RepeatTurn;
        private IEnumerator ChangeTurnCoroutine()
        {
            yield return new WaitForSeconds(TurnChangeTime);
            // clearing current player Dice results
            
            
            // changing the current player
            if (!RepeatTurn)
            {
                CurrentPlayer.PlayerDiceResults.Clear();
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
    }
}