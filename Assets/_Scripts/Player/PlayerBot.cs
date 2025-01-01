using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Managers;
using UnityEngine;

namespace _Scripts.Player
{
    public class PlayerBot : MonoBehaviour
    {
        public Player MyPlayer;

        public int SafePlaceWeight = 20;
        public int KillPawnWeight = 40;
        public int RunFromPawnWeight = 40;
        public int VictoryWeight = 20;

        public IEnumerator BotRollDiceCo()
        {
            yield return new WaitForSeconds(0.5f);
            MyPlayer.RollDice();
        }

        public void MakeBotPlay(int moveStep)
        {
            BestPawn(moveStep).MovePawn(moveStep);
        }

        public void MakeBotPlayTwoSteps(int moveStep1, int moveStep2)
        {
            Pawn bestPawn = null;
            int bestMove = 0;
            int bestScore = int.MinValue;

            foreach (Pawn p in MyPlayer._enteredPawns)
            {
                if (p.CanPawnMove(moveStep1) && p.CanPawnMove(moveStep2))
                {
                    p.BotMove1Score = CheckForKill(p, moveStep1) + CheckBeforeToRun(p) +
                                      CheckAheadForSafePosition(p, moveStep1) + CheckForVictory(p);
                    p.BotMove2Score = CheckForKill(p, moveStep2) + CheckBeforeToRun(p) +
                                      CheckAheadForSafePosition(p, moveStep2) + CheckForVictory(p);
                    p.BotMove1and2Score = CheckForKill(p, moveStep1 + moveStep2) + CheckBeforeToRun(p) +
                                          CheckAheadForSafePosition(p, moveStep1 + moveStep2) + CheckForVictory(p);

                    // Find the best move and pawn
                    if (p.BotMove1Score > bestScore)
                    {
                        bestPawn = p;
                        bestMove = 1;
                        bestScore = p.BotMove1Score;
                    }
                    else if (p.BotMove2Score > bestScore)
                    {
                        bestPawn = p;
                        bestMove = 2;
                        bestScore = p.BotMove2Score;
                    }
                    else if (p.BotMove1and2Score > bestScore)
                    {
                        bestPawn = p;
                        bestMove = 3;
                        bestScore = p.BotMove1and2Score;
                    }
                }
            }

            // Perform the best move
            if (bestMove == 1)
            {
                bestPawn.MovePawn(moveStep1);
            }
            else if (bestMove == 2)
            {
                bestPawn.MovePawn(moveStep2);
            }
            else if (bestMove == 3)
            {
                MyPlayer.PlayerDiceResults.Clear();
                bestPawn.MovePawn(moveStep1 + moveStep2);
            }
        }


        public void BotBonusMovePlay(int bonusMoveStep)
        {
            BestPawn(bonusMoveStep).MovePawn(bonusMoveStep);
        }

        // best pawn to move for a given move
        public Pawn BestPawn(int moveStep)
        {
            Pawn bestPawn = null;
            List<Pawn> canMovePawns = new List<Pawn>();
            foreach (Pawn p in MyPlayer._enteredPawns)
            {
                if (p.CanPawnMove(moveStep))
                {
                    canMovePawns.Add(p);
                    p.BotMove1Score = CheckForKill(p, moveStep) + CheckBeforeToRun(p) +
                                     CheckAheadForSafePosition(p, moveStep) + CheckForVictory(p);
                }
            }

            if (canMovePawns.Count > 0)
            {
                bestPawn = canMovePawns[0];
                for (int i = 1; i < canMovePawns.Count; i++)
                {
                    if (canMovePawns[i].BotMove1Score > bestPawn.BotMove1Score)
                    {
                        bestPawn = canMovePawns[i];
                    }
                }
            }

            return bestPawn;
        }

        // checks moves ahead of current position
        public int CheckForKill(Pawn p, int step)
        {
            int nearestKillDistance = p.CurrentPositionIndex + 1;
            int targetIndex = p.CurrentPositionIndex + step;
            for (int i = p.CurrentPositionIndex + 1;
                 i <= targetIndex && i <= 71;
                 i++) // 71 is the last victory position
            {
                if (p.MainPlayer.PawnPath[i].PawnsOnNode.Count == 1 &&
                    p.MainPlayer != MyPlayer.PawnPath[i].PawnsOnNode[0].MainPlayer)
                {
                    nearestKillDistance = Math.Max(nearestKillDistance, i);
                }
            }

            return KillPawnWeight * (13 - nearestKillDistance) / 13 - step;
        }

        public int CheckAheadForSafePosition(Pawn p, int step)
        {
            int nearestSafestDistance = p.CurrentPositionIndex + 1;
            int targetIndex = p.CurrentPositionIndex + step;
            for (int i = nearestSafestDistance; i <= targetIndex && i <= 71; i++)
            {
                if (MyPlayer.PawnPath[i].IsStarNode || i > 63 ||
                    (p.MainPlayer.PawnPath[i].IsStartNode &&
                     p.MainPlayer.PawnPath[i].StartNodePlayer ==
                     p.MainPlayer)) // >63 is index for players path to home positions
                {
                    nearestSafestDistance = Math.Max(nearestSafestDistance, i);
                }
            }

            return SafePlaceWeight * (13 - nearestSafestDistance) / (13 - step);
        }

        // checks moves before current position to see if there are other players
        public int CheckBeforeToRun(Pawn p)
        {
            int nearestPlayerDistance = p.CurrentPositionIndex - 1;
            for (int i = p.CurrentPositionIndex - 1; i >= p.CurrentPositionIndex - 12 && i >= 0; i--)
            {
                if (MyPlayer.PawnPath[i].PawnsOnNode.Count > 0)
                {
                    nearestPlayerDistance = Math.Min(nearestPlayerDistance, i);
                }
            }

            return RunFromPawnWeight * (13 - nearestPlayerDistance) / 13;
        }

        public int CheckForVictory(Pawn p)
        {
            int victoryDistance = 71 - p.CurrentPositionIndex;
            return VictoryWeight * (71 - victoryDistance) / 71;
        }
    }
}