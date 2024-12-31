using System.Collections;
using System.Collections.Generic;
using _Scripts.Enums;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject MainGameBoard;
        public static GameManager INSTANCE;
        
        //yellow, green, red, blue in this order
        public Player.Player[] Players;
        public Player.Player CurrentPlayer;
        [SerializeField] private List<Player.Player> StartingPlayers;

        private int _currentPlayerIndex;
        
        private int _selectedPlayerIndex = 0;

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

    

        // selects how many players will be in game
        // called in UI button
        public void InitializeNumOfPlayers(int numberOfPlayers)
        {
            if (numberOfPlayers <= 0)
            {
                numberOfPlayers = 2;
            }
            switch (numberOfPlayers)
            {
                case 2:
                    OnNumOfPlayersTwo();
                    break;
                case 3:
                    OnNumOfPlayersThree();
                    break;
                case 4:
                    Players[0].PlayerType = PlayerType.PLAYER;
                    Players[1].PlayerType = PlayerType.PLAYER;
                    Players[2].PlayerType = PlayerType.PLAYER;
                    Players[3].PlayerType = PlayerType.PLAYER;
                    break;
            }
        }
        public void StartGame()
        {
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
            _currentPlayerIndex = _selectedPlayerIndex;
            CurrentPlayer = StartingPlayers[_currentPlayerIndex];
            SetDicePosition(CurrentPlayer);
        }
        
        // finding which player's place will be empty based on selected player by player
        // called when 2 players are in game
        private void OnNumOfPlayersTwo()
        {
            switch (_selectedPlayerIndex)
            {
                case 0: // yellow selected
                case 2: // red selected
                    Players[0].PlayerType = PlayerType.PLAYER;
                    Players[1].PlayerType = PlayerType.NONE;
                    Players[2].PlayerType = PlayerType.PLAYER;
                    Players[3].PlayerType = PlayerType.NONE;
                    break;
                case 1: // green selected
                case 3: // blue selected
                    Players[0].PlayerType = PlayerType.NONE;
                    Players[1].PlayerType = PlayerType.PLAYER;
                    Players[2].PlayerType = PlayerType.NONE;
                    Players[3].PlayerType = PlayerType.PLAYER;
                    break;
            }
        }

        // finding which player's place will be empty based on selected player by player
        // called when 3 players are in game
        private void OnNumOfPlayersThree()
        {
            switch (_selectedPlayerIndex)
            {
                case 0: // yellow seleted
                    Players[0].PlayerType = PlayerType.PLAYER;
                    Players[1].PlayerType = PlayerType.PLAYER;
                    Players[2].PlayerType = PlayerType.PLAYER;
                    Players[3].PlayerType = PlayerType.NONE;
                    break;
                case 1: // green selected
                    Players[0].PlayerType = PlayerType.NONE;
                    Players[1].PlayerType = PlayerType.PLAYER;
                    Players[2].PlayerType = PlayerType.PLAYER;
                    Players[3].PlayerType = PlayerType.PLAYER;
                    break;
                case 2: // red selected
                    Players[0].PlayerType = PlayerType.PLAYER;
                    Players[1].PlayerType = PlayerType.NONE;
                    Players[2].PlayerType = PlayerType.PLAYER;
                    Players[3].PlayerType = PlayerType.PLAYER;
                    break;
                case 3: // blue selected
                    Players[0].PlayerType = PlayerType.PLAYER;
                    Players[1].PlayerType = PlayerType.PLAYER;
                    Players[2].PlayerType = PlayerType.NONE;
                    Players[3].PlayerType = PlayerType.PLAYER;
                    break;
            }
        }

        // initialize board rotations based on the color of player selected by player
        // call in UI button
        public void InitializeBoardPositions(int? selectedPlayerIndex)
        {
            _selectedPlayerIndex = selectedPlayerIndex ?? 0;
            switch (_selectedPlayerIndex)
            {
                case 0:
                    MainGameBoard.transform.rotation = Quaternion.identity;
                    break;
                case 1:
                    MainGameBoard.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                    break;
                case 2:
                    MainGameBoard.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                    break;
                case 3:
                    MainGameBoard.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                    break;
            }
        }

        // initializing game
        // wont be used once UI is completed
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

                if (StartingPlayers[i].PlayerType == PlayerType.BOT)
                {
                    StartingPlayers[i].DiceManager.DisableDiceCollider();
                    StartingPlayers[i].DisableMyPawns();
                }
            }
            
            CurrentPlayer = StartingPlayers[_currentPlayerIndex];
            CurrentPlayer.ActivateDice();
            SetDicePosition(CurrentPlayer);
        }
        
        // setting position of the dice based on current player
        private void SetDicePosition(Player.Player currentPlayer)
        {
            if (currentPlayer == StartingPlayers[_currentPlayerIndex])
            {
                CurrentPlayer.DiceManager.gameObject.transform.position = currentPlayer.MyDicePosition.position;
            }
        }

        public void ResetDice()
        {
            StartCoroutine(ResetDiceCo());
        }
        
        private IEnumerator ResetDiceCo()
        {
            yield return new WaitForSeconds(TurnChangeTime);
            CurrentPlayer.DiceManager.ResetDiceImage();
        }

        public void UIButtonChangeTurn()
        {
            ChangeTurn();
        }
        
        public void ChangeTurn()
        {
            StartCoroutine(ChangeTurnCoroutine());
        }
        
        private IEnumerator ChangeTurnCoroutine()
        {
            yield return new WaitForSeconds(TurnChangeTime);
            // checking for victory
            Player.Player oldPlayer = CurrentPlayer;
            // changing the current player
            if (CurrentPlayer.ShouldChangeTurn)
            {
                CurrentPlayer.PlayerDiceResults.Clear();
                foreach (Pawn p in CurrentPlayer._enteredPawns)
                {
                    p.HidePawnOption();
                    p.DisableCollider();
                    p.IsPawnMovable = false;
                }
                _currentPlayerIndex = (_currentPlayerIndex + 1) % StartingPlayers.Count;
            }

            CurrentPlayer = StartingPlayers[_currentPlayerIndex];
            
            for (int i = 0; i < StartingPlayers.Count; i++)
            {
                StartingPlayers[i].IsMyTurn = i == _currentPlayerIndex;
            }

            oldPlayer.DeactivateTurnUI();
            oldPlayer.DeactivateDice();
            CurrentPlayer.ActivateTurnUI();
            CurrentPlayer.ActivateDice();

            CurrentPlayer.StartRollDice();
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