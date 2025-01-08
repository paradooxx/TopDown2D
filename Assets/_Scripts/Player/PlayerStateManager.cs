using System;
using System.Collections.Generic;
using _Scripts.Board;
using _Scripts.Enums;
using _Scripts.Managers;
using UnityEngine;

namespace _Scripts.Player
{
    public class PlayerStateManager : MonoBehaviour
    {
        private BoardStateManager _boardStateManager;
        private BoardState _currentBoardState;

        private void Awake()
        {
            _boardStateManager = new BoardStateManager();
        }

        // call this when saving game state
        public void SaveGameState(List<Player> players, GameManager gameManager)
        {
            _currentBoardState = new BoardState(players, gameManager);
            _boardStateManager.SaveBoardState(_currentBoardState);
        }

        // call this for loading a saved game state
        public void LoadGameState(Player players)
        {
            BoardState loadedState = _boardStateManager.LoadBoardState();
            if (loadedState != null)
            {
                ApplyBoardState(loadedState, players);
            }
        }

        // for starting a game with default state
        public void LoadDefaultBoardState(List<Player> players, GameManager gameManager)
        {
            BoardState loadedState = _boardStateManager.LoadDefaultBoardState(players, gameManager);
            for (int i = 0; i < players.Count; i++)
            {
                // ApplyBoardState(loadedState, Player, diceManager);
                for (int j = 0; j < players[i].AllPawns.Count; j++)
                {
                    players[i].AllPawns[j].CurrentPositionIndex = loadedState.PlayerStates[i].PawnStates[j].Position;
                }

                players[i].IsMyTurn = loadedState.PlayerStates[i].IsMyTurn;
                players[i].MyIndex = loadedState.PlayerStates[i].MyIndex;
                players[i].WinPosition = loadedState.PlayerStates[i].WinPosition;
                players[i].HasBonusMove = loadedState.PlayerStates[i].HasBonusMoves;
                players[i].BonusMove = loadedState.PlayerStates[i].BonusMoves;
                players[i].PlayerType = Util.GetPlayerEnumFromId(StaticDatas.PLAYERS_DATA_TYPES.playerDataList[i].TypeId);
            }

            _boardStateManager.SaveBoardState(loadedState);
        }

        private void ApplyBoardState(BoardState state, Player players)
        {
            if (players.MyIndex > state.PlayerStates.Count)
            {
                return;
            }
            
            ApplyPlayerState(players, state.PlayerStates[players.MyIndex]);
            Debug.Log("Current Player Index: " + state.currentPlayerIndex);
            GameManager.GetInstance().CurrentPlayerIndex = state.currentPlayerIndex;
            // players.PlayerDiceResults = state.DiceStates.DiceResults;
        }

        public void AddNewPlayer(Player player, int index)
        {
            BoardState state = _boardStateManager.LoadBoardState();

            if (index > GameManager.GetInstance().StartingPlayers.Count)
            {
                index--;
            }

            GameManager.GetInstance().StartingPlayers.Insert(index, player);
            StaticDatas.PLAYERS_DATA_TYPES.playerDataList[player.MyIndex].TypeId = Util.GetPlayerIdFromEnum(player.PlayerType);
            player.PlayerType = Util.GetPlayerEnumFromId(StaticDatas.PLAYERS_DATA_TYPES.playerDataList[player.MyIndex].TypeId);
            PlayerSelectionController.GetInstance().SavePlayerDataInput();
            
            player.StartGame();
            _boardStateManager.SaveBoardState(state);
            Debug.Log("New Player Added: " + player);
        }

        public void RemovePlayer(Player player)
        {
            BoardState state = _boardStateManager.LoadBoardState();
            foreach (Pawn p in player.AllPawns)
            {
                Destroy(p.gameObject);
            }

            player.HomePawns.Clear();
            player.AllPawns.Clear();
            player.EnteredPawns.Clear();
            player.HomePawnsColliders.Clear();
            
            state.PlayerStates[player.MyIndex].PawnStates.Clear();
            GameManager.GetInstance().StartingPlayers.Remove(player);
            _boardStateManager.SaveBoardState(state);
        }

        private void ApplyPlayerState(Player player, PlayerState state)
        {
            Debug.Log("PawnStates: " + state.PawnStates.Count);
            player.IsMyTurn = state.IsMyTurn;
            player.WinPosition = state.WinPosition;
            player.MyIndex = state.MyIndex;
            player.HasBonusMove = state.HasBonusMoves;
            player.BonusMove = state.BonusMoves;
            player.PlayerType = Util.GetPlayerEnumFromId(StaticDatas.PLAYERS_DATA_TYPES.playerDataList[player.MyIndex].TypeId);


            if (player.MyIndex > state.PawnStates.Count)
            {
                return;
            }
            
            if (state.PawnStates[player.MyIndex] != null)
            {
                for (int i = 0; i < player.AllPawns.Count; i++)
                {
                    if (state.PawnStates[i].Position != -1)
                    {
                        player.PawnPath[state.PawnStates[i].Position].AddPawn(player.AllPawns[i]);
                        if (!player.EnteredPawns.Contains(player.AllPawns[i]))
                        {
                            player.EnteredPawns.Add(player.AllPawns[i]);
                            player.HomePawns.Remove(player.AllPawns[i]);
                        }

                        player.PawnsInPlay = player.EnteredPawns.Count;
                    }
                    player.AllPawns[i].CurrentPositionIndex = state.PawnStates[i].Position;
                }
            }

            if (state.DiceStates != null)
            {
                bool useSavedDataForDiceImage = false;
                player.DiceStates = state.DiceStates;
                Debug.Log("Dice Collider for current player disabled");
                
                foreach (var t in state.DiceStates)
                {
                    if (t.diceState)
                    {
                        useSavedDataForDiceImage = true;
                    }
                }
                
                if (state.IsMyTurn)
                {
                    player.DiceManager.ResetDiceImage();
                    player.DiceManager.MakeSpriteNormalColor();
                    if (useSavedDataForDiceImage)
                    {
                        Debug.Log("Chaaaaaaaange Dice Image on Loooaaad");
                        Debug.Log(state.DiceStates[0].Value);
                        Debug.Log(state.DiceStates[1].Value);
                        player.DiceManager.ActivateDice();
                        player.DiceManager.SetDiceSpriteToResult(state.DiceStates[0].Value, state.DiceStates[1].Value);
                        player.DiceManager.DisableDiceCollider();
                        player.DimDiceSprite();
                    }
                    else
                    {
                        player.DiceManager.ActivateDice();
                        player.DiceManager.MakeSpriteNormalColor();
                    }
                }
                else
                {
                    player.DiceManager.DeactivateDice();
                }
            }
            
        }
    }
}