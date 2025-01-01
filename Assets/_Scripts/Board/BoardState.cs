using System.Collections.Generic;
using _Scripts.Managers;
using UnityEngine;

namespace _Scripts.Board
{
    public class BoardState : MonoBehaviour
    {
        List<PlayerState> playerStates = new List<PlayerState>();
        private DiceState diceStates = new DiceState();

        public BoardState(List<Player.Player> players, DiceManager dices)
        {
            playerStates = new List<PlayerState>();
            foreach (var player in players)
            {
                PlayerState playerState = new PlayerState();
                playerState.MyIndex = player.MyIndex;

                foreach (var pawn in player._myPawns)
                {
                    PawnState p = new PawnState();
                    p.Position = pawn.CurrentPositionIndex;
                    p.IsHome = pawn.isHome;
                    p.IsPlaying = pawn.IsInPlay;
                    p.ParentIndex = pawn.MainPlayer.MyIndex;
                    playerState.PawnStates.Add(p);
                }

                playerStates.Add(playerState);
            }

            diceStates = new DiceState(dices.Dice1Result, dices.Dice2Result);
        }
    }

    class PawnState
    {
        public int Position;
        public bool IsPlaying;
        public bool IsHome;
        public int ParentIndex;
    }

    class PlayerState
    {
        public List<PawnState> PawnStates = new List<PawnState>();
        public int MyIndex;
    }

    class DiceState
    {
        private int _dice1Value, _dice2Value;

        public DiceState() {}

        public DiceState(int dice1, int dice2)
        {
            _dice1Value = dice1;
            _dice2Value = dice2;
        }
    }
}