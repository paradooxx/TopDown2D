using System;
using System.Collections;
using System.Collections.Generic;
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
            yield return new WaitForSeconds(1f);
            MyPlayer.RollDice();
        }

        public void MakeBotPlay(int moveStep)
        {
            BestPawn(moveStep).MovePawn(moveStep);
        }

        public void MakeBotPlayTwoSteps()
        {
            Pawn bestPawn = null;
            int bestMove = 1;
            int bestScore = Int32.MinValue;

            foreach (Pawn p in MyPlayer.EnteredPawns)
            {
                if (p.CanPawnMove(MyPlayer.DiceStates[0].Value) && p.CanPawnMove(MyPlayer.DiceStates[1].Value))
                {
                    p.BotMoveScore = TotalScore(p, MyPlayer.DiceStates[0].Value, MyPlayer.DiceStates[1].Value);

                    if (p.CanPawnMove(MyPlayer.DiceStates[0].Value + MyPlayer.DiceStates[1].Value))
                    {
                        p.BotTwoMovesScore = TotalScore(p, MyPlayer.DiceStates[0].Value + MyPlayer.DiceStates[1].Value);
                    }

                    // find the best move and pawn
                    if (p.BotMoveScore > bestScore)
                    {
                        bestPawn = p;
                        bestMove = 1;
                        bestScore = p.BotMoveScore;
                    }

                    if (p.BotTwoMovesScore > bestScore)
                    {
                        bestPawn = p;
                        bestMove = 2;
                        bestScore = p.BotTwoMovesScore;
                    }
                }
            }

            // Perform the best move
            if (bestMove == 1)
            {
                Debug.Log("Best Pawn Move 1 : " + bestPawn.TakeStep[0]);
                Debug.Log("Best Pawn Move 2 : " + bestPawn.TakeStep[1]);
                if (bestPawn.TakeStep[0] > bestPawn.TakeStep[1])
                {
                    bestPawn.MovePawn(MyPlayer.DiceStates[0].Value);
                }
                else if (bestPawn.TakeStep[0] > bestPawn.TakeStep[1])
                {
                    bestPawn.MovePawn(MyPlayer.DiceStates[1].Value);
                }
                else
                {
                    if (MyPlayer.DiceStates[0].Value >= MyPlayer.DiceStates[1].Value)
                    {
                        bestPawn.MovePawn(MyPlayer.DiceStates[0].Value);
                    }
                    else
                    {
                        bestPawn.MovePawn(MyPlayer.DiceStates[1].Value);
                    }
                }
            }
            else if (bestMove == 2)
            {
                MyPlayer.ResetDiceStates();
                bestPawn.MovePawn(MyPlayer.DiceStates[0].Value + MyPlayer.DiceStates[1].Value);
            }

            foreach (Pawn p in MyPlayer.EnteredPawns)
            {
                p.TakeStep[0] = 0;
                p.TakeStep[1] = 0;
            }
        }

        // best pawn to move for a given move
        public Pawn BestPawn(int moveStep)
        {
            Pawn bestPawn = null;
            List<Pawn> canMovePawns = new List<Pawn>();
            foreach (Pawn p in MyPlayer.EnteredPawns)
            {
                if (p.CanPawnMove(moveStep))
                {
                    canMovePawns.Add(p);
                    p.BotMoveScore = TotalScore(p, moveStep);
                }
            }

            if (canMovePawns.Count > 0)
            {
                bestPawn = canMovePawns[0];
                for (int i = 1; i < canMovePawns.Count; i++)
                {
                    if (canMovePawns[i].BotMoveScore > bestPawn.BotMoveScore)
                    {
                        bestPawn = canMovePawns[i];
                    }
                }
            }

            return bestPawn;
        }

        public int FindPreviousPathsNearestEnemy(Pawn p, int startIndex)
        {
            int nearestPlayerDistance = p.CurrentPositionIndex - startIndex;
            if (nearestPlayerDistance <= 0)
            {
                return 71;
            }

            for (int i = nearestPlayerDistance; i > 0; i--)
            {
                if (p.MainPlayer.PawnPath[i].PawnsOnNode.Count == 1 &&
                    p.MainPlayer != MyPlayer.PawnPath[i].PawnsOnNode[0].MainPlayer)
                {
                    nearestPlayerDistance = p.CurrentPositionIndex - i;
                    break;
                }
            }

            return nearestPlayerDistance <= 0 ? 71 : nearestPlayerDistance;
        }

        public int FindNearestEnemy(Pawn p, int startIndex)
        {
            int nearestPlayerDistance = 71 - p.CurrentPositionIndex;
            for (int i = p.CurrentPositionIndex + startIndex; i <= 71; i++)
            {
                if (p.MainPlayer.PawnPath[i].PawnsOnNode.Count == 1 &&
                    p.MainPlayer != MyPlayer.PawnPath[i].PawnsOnNode[0].MainPlayer)
                {
                    nearestPlayerDistance = i - p.CurrentPositionIndex;
                    break;
                }
            }

            return nearestPlayerDistance <= 0 ? 71 : nearestPlayerDistance;
        }

        public int FindNearestSafePosition(Pawn p, int startIndex)
        {
            int nearestSafePositionDistance = 63 - p.CurrentPositionIndex;
            for (int i = p.CurrentPositionIndex + startIndex; i < 64; i++)
            {
                if (MyPlayer.PawnPath[i].IsStarNode ||
                    (p.MainPlayer.PawnPath[i].IsStartNode &&
                     p.MainPlayer.PawnPath[i].StartNodePlayer ==
                     p.MainPlayer))
                {
                    nearestSafePositionDistance = i - p.CurrentPositionIndex + 1;
                    break;
                }
            }

            return nearestSafePositionDistance <= 0 ? 1 : nearestSafePositionDistance;
        }

        public int FindVictoryDistance(Pawn p, int index)
        {
            return (71 - p.CurrentPositionIndex - index) <= 0 ? 1 : (71 - p.CurrentPositionIndex - index);
        }

        private int PawnKillScore(Pawn p, int step1, int step2 = 0)
        {
            int killScore1 = 0;
            int killScore2 = 0;
            int nearestEnemyAheadFromCurrentPos = FindNearestEnemy(p, 1);

            if (step2 == 0)
            {
                killScore1 = SafePlaceWeight * step1 / nearestEnemyAheadFromCurrentPos;
                if (nearestEnemyAheadFromCurrentPos == step1)
                {
                    killScore1 += (int)Mathf.Pow(KillPawnWeight, 4);
                }
                return killScore1;
            }

            int firstMoveToPlay;
            int nextMoveToPlay;
            int nextNearestEnemyDistance;
            int nearestEnemyAheadWhenMovedStep1 = FindNearestEnemy(p, step1);
            int nearestEnemyAheadWhenMovedStep2 = FindNearestEnemy(p, step2);

            {
                firstMoveToPlay = step1;
                nextMoveToPlay = step2;
                nextNearestEnemyDistance = FindNearestEnemy(p, firstMoveToPlay + nextMoveToPlay);
                killScore1 += KillPawnWeight * (nearestEnemyAheadFromCurrentPos - nearestEnemyAheadWhenMovedStep1) / nearestEnemyAheadFromCurrentPos;
                killScore1 += KillPawnWeight * firstMoveToPlay / nearestEnemyAheadFromCurrentPos;
                killScore1 += KillPawnWeight * (nextNearestEnemyDistance - nextMoveToPlay) / nextNearestEnemyDistance;
                if (nearestEnemyAheadFromCurrentPos == firstMoveToPlay)
                {
                    killScore1 += (int)Mathf.Pow(KillPawnWeight, 4);
                    p.TakeStep[0]++;
                }
            }

            {
                firstMoveToPlay = step2;
                nextMoveToPlay = step1;
                nextNearestEnemyDistance = FindNearestEnemy(p, firstMoveToPlay + nextMoveToPlay);
                killScore1 += KillPawnWeight * (nearestEnemyAheadFromCurrentPos - nearestEnemyAheadWhenMovedStep2) / nearestEnemyAheadFromCurrentPos;
                killScore2 += KillPawnWeight * firstMoveToPlay / nearestEnemyAheadFromCurrentPos;
                killScore2 += KillPawnWeight * (nextNearestEnemyDistance - nextMoveToPlay) / nextNearestEnemyDistance;
                if (nearestEnemyAheadFromCurrentPos == firstMoveToPlay)
                {
                    killScore2 += (int)Mathf.Pow(KillPawnWeight, 4);
                    p.TakeStep[1]++;
                }
            }

            if (killScore1 > killScore2) p.TakeStep[0]++;
            else p.TakeStep[1]++;

            return killScore1 > killScore2 ? killScore1 : killScore2;
        }

        private int PawnSafeScore(Pawn p, int step1, int step2 = 0)
        {
            int safeScore1 = 0;
            int safeScore2 = 0;

            int nearestSafePlaceFromCurrentPos = FindNearestSafePosition(p, 1);

            // single step case
            // when (moving step1 + step2)
            if (step2 == 0)
            {
                safeScore1 = SafePlaceWeight * step1 / nearestSafePlaceFromCurrentPos;
                return safeScore1;
            }

            int firstMoveToPlay;
            int nextMoveToPlay;
            int nextNearestSafePlace;
            int nearestSafePlaceOnStep1 = FindNearestSafePosition(p, step1);
            int nearestSafePlaceOnStep2 = FindNearestSafePosition(p, step2);

            {
                firstMoveToPlay = step1;
                nextMoveToPlay = step2;
                nextNearestSafePlace = FindNearestSafePosition(p, firstMoveToPlay + nextMoveToPlay);
                safeScore1 += SafePlaceWeight * (nearestSafePlaceFromCurrentPos - nearestSafePlaceOnStep1) / nearestSafePlaceFromCurrentPos;
                safeScore1 += SafePlaceWeight * (nearestSafePlaceFromCurrentPos - firstMoveToPlay) / nearestSafePlaceFromCurrentPos;
                safeScore1 += SafePlaceWeight * (nextNearestSafePlace - nextMoveToPlay) / nextNearestSafePlace;
                if (nearestSafePlaceFromCurrentPos == firstMoveToPlay)
                {
                    safeScore1 += SafePlaceWeight * SafePlaceWeight;
                    p.TakeStep[0]++;
                }
            }

            {
                firstMoveToPlay = step2;
                nextMoveToPlay = step1;
                nextNearestSafePlace = FindNearestSafePosition(p, firstMoveToPlay + nextMoveToPlay);
                safeScore1 += SafePlaceWeight * (nearestSafePlaceFromCurrentPos - nearestSafePlaceOnStep2) / nearestSafePlaceFromCurrentPos;
                safeScore2 += SafePlaceWeight * (nearestSafePlaceFromCurrentPos - firstMoveToPlay) / nearestSafePlaceFromCurrentPos;
                safeScore2 += SafePlaceWeight * (nextNearestSafePlace - nextMoveToPlay) / nextNearestSafePlace;
                if (nearestSafePlaceFromCurrentPos == firstMoveToPlay)
                {
                    safeScore2 += SafePlaceWeight * SafePlaceWeight;
                    p.TakeStep[1]++;
                }
            }

            if (safeScore1 > safeScore2) p.TakeStep[0]++;
            else p.TakeStep[1]++;

            return safeScore1 > safeScore2 ? safeScore1 : safeScore2;
        }

        private int PawnRunScore(Pawn p, int step1, int step2 = 0)
        {
            int runScore1 = 0;
            int runScore2 = 0;
            int nearestEnemyPosOnPreviousPath = FindPreviousPathsNearestEnemy(p, 1);

            // single step case
            // when (moving step1 + step2)
            if (step2 == 0)
            {
                runScore1 = SafePlaceWeight * (nearestEnemyPosOnPreviousPath + step1) / nearestEnemyPosOnPreviousPath;
                return runScore1;
            }

            int firstMoveToPlay;
            int nextMoveToPlay;
            int nextNearestEnemyOnPreviousPath;
            int nearestEnemyOnPreviousPathOnMovedStep1 = FindPreviousPathsNearestEnemy(p, step1);
            int nearestEnemyOnPreviousPathOnMovedStep2 = FindPreviousPathsNearestEnemy(p, step2);

            {
                firstMoveToPlay = step1;
                nextMoveToPlay = step2;
                nextNearestEnemyOnPreviousPath = FindPreviousPathsNearestEnemy(p, firstMoveToPlay + nextMoveToPlay);
                runScore1 += RunFromPawnWeight * (nearestEnemyPosOnPreviousPath - nearestEnemyOnPreviousPathOnMovedStep1) / nearestEnemyPosOnPreviousPath;
                runScore1 += RunFromPawnWeight * firstMoveToPlay / nearestEnemyPosOnPreviousPath;
                runScore1 += RunFromPawnWeight * (nextMoveToPlay - nextNearestEnemyOnPreviousPath) / nextMoveToPlay;
            }

            {
                firstMoveToPlay = step2;
                nextMoveToPlay = step1;
                nextNearestEnemyOnPreviousPath = FindPreviousPathsNearestEnemy(p, firstMoveToPlay + nextMoveToPlay);
                runScore2 += RunFromPawnWeight * (nearestEnemyPosOnPreviousPath - nearestEnemyOnPreviousPathOnMovedStep2) / nearestEnemyPosOnPreviousPath;
                runScore2 += RunFromPawnWeight * firstMoveToPlay / nearestEnemyPosOnPreviousPath;
                runScore2 += RunFromPawnWeight * (nextMoveToPlay - nextNearestEnemyOnPreviousPath) / nextMoveToPlay;
            }

            if (runScore1 > runScore2) p.TakeStep[0]++;
            else p.TakeStep[1]++;

            return runScore1 > runScore2 ? runScore1 : runScore2;
        }

        private int VictoryScore(Pawn p, int step1, int step2 = 0)
        {
            int victoryScore1 = 0;
            int victoryScore2 = 0;
            int finishDistanceFromCurrentPos = FindVictoryDistance(p, 0);

            if (step2 == 0)
            {
                victoryScore1 = SafePlaceWeight * step1 / finishDistanceFromCurrentPos;
                if (finishDistanceFromCurrentPos == step1)
                {
                    victoryScore1 += (int)Mathf.Pow(VictoryWeight, 4);
                    p.TakeStep[0]++;
                }
                return victoryScore1;
            }

            int firstNearestDistance;
            int nextNearestDistance;
            int finishDistanceWhenMovedStep1 = FindVictoryDistance(p, step1);
            int finishDistanceWhenMovedStep2 = FindVictoryDistance(p, step2);

            {
                firstNearestDistance = finishDistanceWhenMovedStep1;
                nextNearestDistance = finishDistanceWhenMovedStep2;
                victoryScore1 += VictoryWeight * firstNearestDistance / finishDistanceFromCurrentPos;
                victoryScore1 += VictoryWeight * (firstNearestDistance - nextNearestDistance) / firstNearestDistance;
                if (finishDistanceFromCurrentPos == step1)
                {
                    victoryScore1 += (int)Mathf.Pow(VictoryWeight, 4);
                    p.TakeStep[0]++;
                }
            }

            {
                firstNearestDistance = finishDistanceWhenMovedStep2;
                nextNearestDistance = finishDistanceWhenMovedStep1;
                victoryScore2 += VictoryWeight * firstNearestDistance / finishDistanceFromCurrentPos;
                victoryScore2 += VictoryWeight * (firstNearestDistance - nextNearestDistance) / firstNearestDistance;
                if (finishDistanceFromCurrentPos == step2)
                {
                    victoryScore2 += (int)Mathf.Pow(VictoryWeight, 4);
                    p.TakeStep[1]++;
                }
            }

            if (victoryScore1 > victoryScore2) p.TakeStep[0]++;
            else p.TakeStep[1]++;

            return victoryScore1 > victoryScore2 ? victoryScore1 : victoryScore2;
        }

        public int TotalScore(Pawn p, int step1, int step2 = 0)
        {
            int totalScore = 0;

            if (step2 == 0)
            {
                totalScore = PawnRunScore(p, step1) + VictoryScore(p, step1) + PawnSafeScore(p, step2) +
                             PawnKillScore(p, step1);
                return totalScore;
            }

            totalScore = PawnRunScore(p, step1, step2) + VictoryScore(p, step1, step2) +
                         PawnSafeScore(p, step2, step1) + PawnKillScore(p, step1, step2);
            return totalScore;
        }
    }
}