using System;
using System.Collections.Generic;
using _Scripts.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Board
{
    [Serializable]
    public class BoardState
    {
        public List<PlayerState> PlayerStates { get; private set; }
        public int currentPlayerIndex;

        public BoardState(List<Player.Player> players, GameManager gameManager)
        {
            PlayerStates = new List<PlayerState>();
            currentPlayerIndex = gameManager._currentPlayerIndex;
            foreach (var player in players)
            {
                PlayerState playerState = new PlayerState();

                playerState.MyIndex = player.MyIndex;
                playerState.IsMyTurn = player.IsMyTurn;
                playerState.HasBonusMoves = player.HasBonusMove;
                playerState.BonusMoves = player.BonusMove;

                foreach (var pawn in player._allPawns)
                {
                    PawnState p = new PawnState();
                    p.Position = pawn.CurrentPositionIndex;
                    playerState.PawnStates.Add(p);
                }

                DiceState d = new DiceState
                {
                    DiceResult1 = player.DiceManager.DiceResults[0],
                    DiceResult2 = player.DiceManager.DiceResults[1],
                    DiceState1 = true,
                    DiceState2 = true
                };
                playerState.DiceStates = d;
                PlayerStates.Add(playerState);
            }
        }


        public BoardState(SaveData saveData)
        {
            PlayerStates = saveData.PlayerStates;
            currentPlayerIndex = saveData.CurrentPlayerIndex;
        }
    }


    [Serializable]
    public class PawnState
    {
        public int Position;
    }

    [Serializable]
    public class PlayerState
    {
        public List<PawnState> PawnStates = new List<PawnState>();
        public DiceState DiceStates;
        public bool IsMyTurn;
        public int MyIndex;
        public int WinPosition;

        public bool HasBonusMoves;
        public int BonusMoves;
    }

    [Serializable]
    public class DiceState
    {
        public int DiceResult1;
        public int DiceResult2;
        public bool DiceState1;
        public bool DiceState2;
        // public DiceState(int value, bool state)
        // {
        //     DiceResult = value;
        //     diceState = state;
        // }
    }

    [Serializable]
    public class SaveData
    {
        public List<PlayerState> PlayerStates = new List<PlayerState>();
        public int CurrentPlayerIndex;
    }
}