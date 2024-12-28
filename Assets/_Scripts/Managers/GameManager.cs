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

        private int _currentPlayerIndex;
        public bool ChangeTurnBool;
        public bool ResetDiceBool;
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
            Pawn.OnPawnMoveCompleted += OnPawnMoveComplete;
        }

        private void OnDisable()
        {
            DiceManager.OnDiceRollFinished -= EnablePlayerPlay;
            Pawn.OnPawnMoveCompleted -= OnPawnMoveComplete;
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
                    ChangeTurnBool = false;
                    ResetDiceBool = true;
                }
                else
                {
                    ChangeTurnBool = true;
                }
                CurrentPlayer.MakePawnPlayTwoSteps(CurrentPlayer.PlayerDiceResults[0], CurrentPlayer.PlayerDiceResults[1]);
            }
        }
        
        public void OnPawnMoveComplete()
        {
            if (CurrentPlayer.PlayerDiceResults.Count == 0)
            {
                if (CurrentPlayer.HasBonusMove)
                {
                    CurrentPlayer.BonusMovePlay(CurrentPlayer.BonusMove);
                }
                else
                {
                    foreach (Pawn p in CurrentPlayer._enteredPawns)
                    {
                        p.DisableCollider();
                        p.HidePawnOption();
                        p.IsPawnClicked = false;
                        Debug.Log("reached here");
                        ResetDiceBool = true;
                        ChangeTurn();
                    }
                }
            }
            else if (CurrentPlayer.PlayerDiceResults.Count == 1)
            {
                CurrentPlayer.MakePawnPlay(CurrentPlayer.PlayerDiceResults[0]);
            }

            foreach (Pawn p in CurrentPlayer._enteredPawns)
            {
                p.IsPawnClicked = false;
            }
        }

        public void ResetDice()
        {
            StartCoroutine(ResetDiceCo());
        }
        
        private IEnumerator ResetDiceCo()
        {
            yield return new WaitForSeconds(TurnChangeTime);
            DiceManager.ResetDice();
            ChangeTurnBool = false;
            ResetDiceBool = false;
        }
        public void ChangeTurn()
        {
            StartCoroutine(ChangeTurnCoroutine());
        }
        
        private IEnumerator ChangeTurnCoroutine()
        {
            yield return new WaitForSeconds(TurnChangeTime);
            // checking for victory
            CheckForVictory();
            
            // changing the current player
            if (ChangeTurnBool)
            {
                CurrentPlayer.PlayerDiceResults.Clear();
                foreach (Pawn p in CurrentPlayer._enteredPawns)
                {
                    p.HidePawnOption();
                    p.DisableCollider();
                    p.IsPawnMovable = false;
                }
                _currentPlayerIndex = (_currentPlayerIndex + 1) % StartingPlayers.Count;
                ChangeTurnBool = false;
            }

            CurrentPlayer = StartingPlayers[_currentPlayerIndex];
            
            for (int i = 0; i < StartingPlayers.Count; i++)
            {
                StartingPlayers[i].IsMyTurn = i == _currentPlayerIndex;
            }
            
            SetDicePosition(CurrentPlayer);

            if (ResetDiceBool)
            {
                DiceManager.ResetDice();
                ResetDiceBool = false;
            }
        }

        public void CheckForVictory()
        {
            // check if the current player has completed all pawns
            if (CurrentPlayer.PawnPath[^1].PawnsOnNode.Count == 4)
            {
                CurrentPlayer.DisableMyPawns();
                StartingPlayers.Remove(CurrentPlayer);

                // checking if the game should end
                if (StartingPlayers.Count == 1)
                {
                    // game over logic
                    return;
                }

                // update turn for remaining players
                _currentPlayerIndex = _currentPlayerIndex % StartingPlayers.Count;
                CurrentPlayer = StartingPlayers[_currentPlayerIndex];

                for (int i = 0; i < StartingPlayers.Count; i++)
                {
                    StartingPlayers[i].IsMyTurn = i == _currentPlayerIndex;
                }

                SetDicePosition(CurrentPlayer);
            }
        }

    }
}