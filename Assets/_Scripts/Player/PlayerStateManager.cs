using System;
using System.Collections.Generic;
using _Scripts.Board;
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
        public void SaveGameState(List<Player> players, DiceManager diceManager, GameManager gameManager)
        {
            _currentBoardState = new BoardState(players, diceManager, gameManager);
            _boardStateManager.SaveBoardState(_currentBoardState);
            Debug.Log("Game state saved!");
        }

        // call this loading a saved game state
        public void LoadGameState(Player players, DiceManager diceManager)
        {
            BoardState loadedState = _boardStateManager.LoadBoardState();
            if (loadedState != null)
            {
                ApplyBoardState(loadedState, players, diceManager);
                Debug.Log("Game state loaded and applied!");
            }
        }

        public void LoadDefaultBoardState(List<Player> players, DiceManager diceManager, GameManager gameManager)
        {
            BoardState loadedState = _boardStateManager.LoadDefaultBoardState(players, diceManager, gameManager);
            for(int i = 0 ; i < players.Count ; i++)
            {
                // ApplyBoardState(loadedState, Player, diceManager);
                for(int j = 0 ; i < players[i]._allPawns.Count; i++)
                {
                    players[i]._allPawns[j].CurrentPositionIndex = loadedState.PlayerStates[j].PawnStates[j].Position;
                }

                players[i].IsMyTurn = loadedState.PlayerStates[i].IsMyTurn;
                players[i].MyIndex = loadedState.PlayerStates[i].MyIndex;
                players[i].WinPosition = loadedState.PlayerStates[i].WinPosition;
                players[i].HasBonusMove = loadedState.PlayerStates[i].HasBonusMoves;
                players[i].BonusMove = loadedState.PlayerStates[i].BonusMoves;
            }
        }

        private void ApplyBoardState(BoardState state, Player players, DiceManager diceManager)
        {
            ApplyPlayerState(players, state.PlayerStates[players.MyIndex]);
            Debug.Log("Current Player Index: " + state.currentPlayerIndex);
            GameManager.GetInstance()._currentPlayerIndex = state.currentPlayerIndex;
            // players.PlayerDiceResults = state.DiceStates.DiceResults;
        }

        private void ApplyPlayerState(Player player, PlayerState state)
        {
            Debug.Log("PawnStates: " + state.PawnStates.Count);
            for(int i = 0 ; i < player._allPawns.Count; i++)
            {
                if (state.PawnStates[i].Position != -1)
                {
                    player.PawnPath[state.PawnStates[i].Position].AddPawn(player._allPawns[i]);
                    if (!player._enteredPawns.Contains(player._allPawns[i]))
                    {
                        player._enteredPawns.Add(player._allPawns[i]);
                        player._myPawns.Remove(player._allPawns[i]);
                    }
                    player.PawnsInPlay = player._enteredPawns.Count;
                }
                player._allPawns[i].CurrentPositionIndex = state.PawnStates[i].Position;
            }
            player.IsMyTurn = state.IsMyTurn;
            player.WinPosition = state.WinPosition;
            player.MyIndex = state.MyIndex;
            player.HasBonusMove = state.HasBonusMoves;
            player.BonusMove = state.BonusMoves;
        }
    }
}