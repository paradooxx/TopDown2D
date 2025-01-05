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

                foreach (var diceResult in player.PlayerDiceResults)
                {
                    DiceState d = new DiceState
                    {
                        DiceStates = new Dictionary<int, bool>()
                    };
                    d.DiceStates[diceResult] = true;
                    playerState.DiceStates.Add(d);
                }

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
        public List<DiceState> DiceStates = new List<DiceState>();
        public bool IsMyTurn;
        public int MyIndex;
        public int WinPosition;

        public bool HasBonusMoves;
        public int BonusMoves;
    }
    
    [Serializable]
    public class DiceState
    {
        public Dictionary<int, bool> DiceStates;
        public int DiceResult;
        public bool diceState;

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