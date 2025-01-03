using System.Collections;
using System.Collections.Generic;
using _Scripts.Enums;
using _Scripts.Player;
using _Scripts.UI;
using UnityEngine;

namespace _Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject MainGameBoard;
        private static GameManager _instance;

        public static GameManager GetInstance()
        {
            return _instance == null ? FindObjectOfType<GameManager>() : _instance;
        }
        
        //yellow, green, red, blue in this order
        public Player.Player[] Players;
        public Player.Player CurrentPlayer;
        [SerializeField] private List<Player.Player> StartingPlayers;

        private int _currentPlayerIndex;
        
        private int _selectedPlayerIndex = 0;

        [SerializeField] private float TurnChangeTime = 1.5f;

        private void Awake()
        {
            if(_instance == null)
            {
                
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            // InitializeGame();
        }
        
        public void StartGame()
        {
            for (int i = 0; i < Players.Length; i++)
            {
                Players[i].PlayerType = Util.GetPlayerEnumFromId(StaticDatas.PLAYERS_DATA_TYPES.playerDataList[i].TypeId);
                Debug.Log(Players[i].PlayerType);
            }
            
            for (int i = 0; i < Players.Length; i++)
            {
                Players[i].MyIndex = i;
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
            
            _currentPlayerIndex = 0;
            CurrentPlayer = StartingPlayers[_currentPlayerIndex];
            CurrentPlayer.ActivateDice();
            SetDicePosition(CurrentPlayer);
            
            if (CurrentPlayer.PlayerType == PlayerType.BOT)
            {
                CurrentPlayer.DiceManager.DisableDiceCollider();
                CurrentPlayer.StartRollDice();
            }
            
            GameStateManager.Instance.SetState(GameState.MAIN_GAME);
        }

        public void InitializeBoardPositions(int selectedPlayerIndex)
        {
            Quaternion originalRotation = MainGameBoard.transform.rotation;
            
            if (selectedPlayerIndex < 0)
            {
                selectedPlayerIndex = 0;
            }
            
            _selectedPlayerIndex = selectedPlayerIndex;
            Debug.Log(_selectedPlayerIndex);
    
            Debug.Log($"Rotating board for player {selectedPlayerIndex}");
    
            switch (selectedPlayerIndex)
            {
                case 0:
                    MainGameBoard.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    _currentPlayerIndex = StartingPlayers.Count - 1;
                    break;
                case 1:
                    MainGameBoard.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                    _currentPlayerIndex = StartingPlayers.Count - 1;
                    break;
                case 2:
                    MainGameBoard.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                    _currentPlayerIndex = StartingPlayers.Count - 1;
                    break;
                case 3:
                    MainGameBoard.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                    _currentPlayerIndex = StartingPlayers.Count - 1;
                    break;
            }
    
            Debug.Log($"Rotation changed from {originalRotation.eulerAngles} to {MainGameBoard.transform.rotation.eulerAngles}");
        }

        // initializing game
        // wont be used once UI is completed
        private void InitializeGame()
        {
            _currentPlayerIndex = 0;
            StartingPlayers = new List<Player.Player>();

            for (int i = 0; i < Players.Length; i++)
            {
                Players[i].MyIndex = i;
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
            
            if (CurrentPlayer.PlayerType == PlayerType.BOT)
            {
                CurrentPlayer.DiceManager.DisableDiceCollider();
                CurrentPlayer.StartRollDice();
            }
        }
        
        // setting position of the dice based on current player
        private void SetDicePosition(Player.Player currentPlayer)
        {
            if (currentPlayer == StartingPlayers[_currentPlayerIndex])
            {
                CurrentPlayer.DiceManager.gameObject.transform.position = currentPlayer.MyDicePosition.position;
            }
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
            CheckForVictory();
            
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

            if (CurrentPlayer.PlayerType == PlayerType.BOT)
            {
                CurrentPlayer.DiceManager.DisableDiceCollider();
                CurrentPlayer.StartRollDice();
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

                if (StartingPlayers.Count == 1)
                {
                    Debug.Log("Game Finished");
                    // return to home
                    // show end screen
                }
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