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
        public void SaveGameState(List<Player> players, GameManager gameManager)
        {
            _currentBoardState = new BoardState(players, gameManager);
            _boardStateManager.SaveBoardState(_currentBoardState);
            Debug.Log("Game state saved!");
        }

        // call this for loading a saved game state
        public void LoadGameState(Player players)
        {
            BoardState loadedState = _boardStateManager.LoadBoardState();
            if (loadedState != null)
            {
                ApplyBoardState(loadedState, players);
                Debug.Log("Game state loaded and applied!");
            }
        }

        // for starting a game with default state
        public void LoadDefaultBoardState(List<Player> players, GameManager gameManager)
        {
            BoardState loadedState = _boardStateManager.LoadDefaultBoardState(players, gameManager);
            for (int i = 0; i < players.Count; i++)
            {
                // ApplyBoardState(loadedState, Player, diceManager);
                for (int j = 0; j < players[i]._allPawns.Count; j++)
                {
                    players[i]._allPawns[j].CurrentPositionIndex = loadedState.PlayerStates[i].PawnStates[j].Position;
                }

                players[i].IsMyTurn = loadedState.PlayerStates[i].IsMyTurn;
                players[i].MyIndex = loadedState.PlayerStates[i].MyIndex;
                players[i].WinPosition = loadedState.PlayerStates[i].WinPosition;
                players[i].HasBonusMove = loadedState.PlayerStates[i].HasBonusMoves;
                players[i].BonusMove = loadedState.PlayerStates[i].BonusMoves;
            }

            _boardStateManager.SaveBoardState(loadedState);
        }

        private void ApplyBoardState(BoardState state, Player players)
        {
            ApplyPlayerState(players, state.PlayerStates[players.MyIndex]);
            Debug.Log("Current Player Index: " + state.currentPlayerIndex);
            GameManager.GetInstance()._currentPlayerIndex = state.currentPlayerIndex;
            // players.PlayerDiceResults = state.DiceStates.DiceResults;
        }

        public void DisableDiceState(Player player, int value)
        {
            BoardState boardState = _boardStateManager.LoadBoardState();
            PlayerState playerState = boardState.PlayerStates[player.MyIndex];

            int diceResult1 = playerState.DiceStates.DiceResult1;
            int diceResult2 = playerState.DiceStates.DiceResult2;

            if (diceResult1 == value)
            {
                Debug.Log("Called 1");
                playerState.DiceStates.DiceState1 = false;
            }
            else if (diceResult2 == value)
            {
                Debug.Log("Called 2");
                playerState.DiceStates.DiceState2 = false;
            }

            _boardStateManager.SaveBoardState(boardState);
        }

        public void AddNewPlayer(Player player, int index)
        {
            BoardState state = _boardStateManager.LoadBoardState();

            GameManager.GetInstance().StartingPlayers.Insert(index, player);
            player.StartGame();

            _boardStateManager.SaveBoardState(state);
        }

        public void RemovePlayer(Player player)
        {
            BoardState state = _boardStateManager.LoadBoardState();
            foreach (Pawn p in player._allPawns)
            {
                Destroy(p.gameObject);
            }

            player._myPawns.Clear();
            player._allPawns.Clear();
            player._enteredPawns.Clear();
            player._myPawnsColliders.Clear();
            GameManager.GetInstance().StartingPlayers.Remove(player);
            _boardStateManager.SaveBoardState(state);
        }

        private void ApplyPlayerState(Player player, PlayerState state)
        {
            Debug.Log("PawnStates: " + state.PawnStates.Count);
            if (state.PawnStates != null)
            {
                for (int i = 0; i < player._allPawns.Count; i++)
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
            }

            if (state.DiceStates != null)
            {
                player.DiceManager.SetDiceSprite(state.DiceStates.DiceResult1, state.DiceStates.DiceResult2);
                if (state.DiceStates.DiceState1 == true)
                {
                    player.PlayerDiceResults.Add(state.DiceStates.DiceResult1);
                }

                if (state.DiceStates.DiceState2 == true)
                {
                    player.PlayerDiceResults.Add(state.DiceStates.DiceResult2);
                }

                Debug.Log("Dice Collider for current player disabled");
                player.DiceManager.DisableDiceCollider();
            }


            player.IsMyTurn = state.IsMyTurn;
            player.WinPosition = state.WinPosition;
            player.MyIndex = state.MyIndex;
            player.HasBonusMove = state.HasBonusMoves;
            player.BonusMove = state.BonusMoves;
        }
    }
}