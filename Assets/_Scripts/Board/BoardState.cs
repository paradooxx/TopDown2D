using System;
using System.Collections.Generic;
using _Scripts.Enums;
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
            currentPlayerIndex = gameManager.CurrentPlayerIndex;
            foreach (var player in players)
            {
                PlayerState playerState = new PlayerState();

                playerState.MyIndex = player.MyIndex;
                playerState.IsMyTurn = player.IsMyTurn;
                playerState.HasBonusMoves = player.HasBonusMove;
                playerState.BonusMoves = player.BonusMove;
                playerState.PlayerType = Util.GetPlayerIdFromEnum(player.PlayerType);

                foreach (var pawn in player.AllPawns)
                {
                    PawnState p = new PawnState();
                    p.Position = pawn.CurrentPositionIndex;
                    playerState.PawnStates.Add(p);
                }
                
                playerState.DiceStates = player.DiceStates;
                
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
        public List<DiceState> DiceStates =new List<DiceState>();
        public bool IsMyTurn;
        public int MyIndex;
        public int WinPosition;

        public bool HasBonusMoves;
        public int BonusMoves;

        public bool ShouldChangeTurn;
        
        public int PlayerType;
    }

    [Serializable]
    public class DiceState
    {
        public int Value;
        public bool diceState = true;

        public DiceState(int value, bool state)
        {
            Value = value;
            diceState = state;
        }
    }

    [Serializable]
    public class SaveData
    {
        public List<PlayerState> PlayerStates = new List<PlayerState>();
        public int CurrentPlayerIndex;
    }
}