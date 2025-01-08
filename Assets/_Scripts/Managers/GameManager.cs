using System.Collections;
using System.Collections.Generic;
using _Scripts.Enums;
using _Scripts.Player;
using _Scripts.UI;
using UnityEngine;
using UnityEngine.Serialization;

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
        public List<Player.Player> Players;
        public Player.Player CurrentPlayer;
        public List<Player.Player> StartingPlayers;
        public List<Player.Player> FinishedPlayers;

        public int CurrentPlayerIndex;

        // private int _selectedPlayerIndex = 0;

        [SerializeField] private float TurnChangeTime = 1.5f;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartGame()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].PlayerType =
                    Util.GetPlayerEnumFromId(StaticDatas.PLAYERS_DATA_TYPES.playerDataList[i].TypeId);
                Debug.Log(Players[i].PlayerType);
            }

            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].PlayerType == PlayerType.PLAYER || Players[i].PlayerType == PlayerType.BOT)
                {
                    StartingPlayers.Add(Players[i]);
                }
                Players[i].PlayerCanvaManager.AddRemovePlayer();
            }

            for (int i = 0; i < StartingPlayers.Count; i++)
            {
                StartingPlayers[i].IsMyTurn = i == CurrentPlayerIndex;
                StartingPlayers[i].gameObject.SetActive(true);

                if (StartingPlayers[i].PlayerType == PlayerType.BOT)
                {
                    StartingPlayers[i].DiceManager.DisableDiceCollider();
                    StartingPlayers[i].DisableHomePawns();
                }
                // StartingPlayers[i].StartGame();
                StartingPlayers[i].NewGameStart();
            }

            // _currentPlayerIndex = 0;
            CurrentPlayer = StartingPlayers[CurrentPlayerIndex];
            CurrentPlayer.PlayerStateManager.LoadDefaultBoardState(Players, this);
            CurrentPlayer.ActivateDice();
            
            if (CurrentPlayer.PlayerType == PlayerType.BOT)
            {
                CurrentPlayer.DiceManager.DisableDiceCollider();
                CurrentPlayer.StartRollDice();
            }

            GameStateManager.Instance.SetState(GameState.MAIN_GAME);
        }

        public void UIButtonChangeTurn()
        {
            CurrentPlayer.ShouldChangeTurn = true;
            Debug.Log("Invoked: " + CurrentPlayer.ShouldChangeTurn);
            ChangeTurn();
            CurrentPlayer.ShouldChangeTurn = false;
        }

        public void ChangeTurn()
        {
            StartCoroutine(ChangeTurnCoroutine());
        }

        private IEnumerator ChangeTurnCoroutine()
        {
            yield return new WaitForSeconds(TurnChangeTime);

            Player.Player oldPlayer = CurrentPlayer;
            // changing the current player
            if (CurrentPlayer.ShouldChangeTurn)
            {
                // CurrentPlayer.PlayerDiceResults.Clear();
                foreach (Pawn p in CurrentPlayer.EnteredPawns)
                {
                    p.HidePawnOption();
                    p.DisableCollider();
                    p.IsPawnMovable = false;
                }
                CurrentPlayerIndex = (CurrentPlayerIndex + 1) % StartingPlayers.Count;
            }
            
            CurrentPlayer = StartingPlayers[CurrentPlayerIndex];
            CurrentPlayer.DiceManager.MakeSpriteNormalColor();
            for (int i = 0; i < StartingPlayers.Count; i++)
            {
                StartingPlayers[i].IsMyTurn = i == CurrentPlayerIndex;
            }
            CurrentPlayer.PlayerStateManager.SaveGameState(Players, this);

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
        
        public void PlayerFinishedGame(Player.Player player)
        {
            FinishedPlayers.Add(player);
            StartingPlayers.Remove(player);
        }

        public void LoadSaveGame()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].PlayerStateManager.LoadGameState(Players[i]);
                // Players[i].PlayerType =
                //     Util.GetPlayerEnumFromId(StaticDatas.PLAYERS_DATA_TYPES.playerDataList[i].TypeId);
                // Debug.Log(Players[i].PlayerType);
                //
                if (Players[i].PlayerType == PlayerType.PLAYER || Players[i].PlayerType == PlayerType.BOT)
                {
                    StartingPlayers.Add(Players[i]);
                }
                Players[i].PlayerCanvaManager.AddRemovePlayer();
            }

            for (int i = 0; i < StartingPlayers.Count; i++)
            {
                StartingPlayers[i].gameObject.SetActive(true);

                if (StartingPlayers[i].PlayerType == PlayerType.BOT)
                {
                    StartingPlayers[i].DiceManager.DisableDiceCollider();
                    StartingPlayers[i].DisableHomePawns();
                }
                StartingPlayers[i].StartGame();
            }

            // _currentPlayerIndex = 0;
            CurrentPlayer = StartingPlayers[CurrentPlayerIndex];
            // CurrentPlayer.ActivateDice();
            Debug.Log("On Pawn Move Complete Called");
            CurrentPlayer.OnPawnMoveComplete();
            
            if (CurrentPlayer.PlayerType == PlayerType.BOT)
            {
                CurrentPlayer.DiceManager.DisableDiceCollider();
                CurrentPlayer.StartRollDice();
            }

            GameStateManager.Instance.SetState(GameState.MAIN_GAME);
        }
    }
}