using System.Collections;
using _Scripts.Managers;
using UnityEngine;

namespace _Scripts.Player
{
    public class PlayerBot : MonoBehaviour
    {
        public Player MyPlayer;
        
        public IEnumerator BotRollDiceCo()
        {
            yield return new WaitForSeconds(0.5f);
            MyPlayer.RollDice();
        }
        
        public void MakeBotPlay(int moveStep)
        {
            // track movable pawns
            bool anyPawnCanMove = false;
            int movablePawnCount = 0;
            Pawn firstMovablePawn = null;

            // checking all entered pawns if they can move
            foreach (Pawn pawn in MyPlayer._enteredPawns)
            {
                if (pawn.CanPawnMove(moveStep))
                {
                    pawn.IsPawnMovable = true;
                    movablePawnCount++;
                    anyPawnCanMove = true;
                    firstMovablePawn = pawn; // tracking the first movable pawn
                }
            }

            // if no pawn can move change turn and exit
            if (!anyPawnCanMove)
            {
                Debug.Log("1");
                GameManager.INSTANCE.ChangeTurn();
                return;
            }

            // if only one pawn is movable move it directly
            if (movablePawnCount == 1)
            {
                firstMovablePawn?.MovePawn(moveStep);
                Debug.Log("here");
            }
            else
            {
                Debug.Log("here");
                // Show options for all movable pawns
                foreach (Pawn pawn in MyPlayer._enteredPawns)
                {
                    if (pawn.IsPawnMovable)
                    {
                        FindBestPawnToMove().MovePawn(moveStep);
                        pawn.AvailableMovesText.text = moveStep.ToString();
                    }
                }
            }
        }
        
        public void MakeBotPlayTwoSteps(int moveStep1, int moveStep2)
        {
            Pawn firstMovablePawn = null;
            int movablePawnCount = 0;

            // Single pass through pawns to check movement possibilities and count
            foreach (Pawn pawn in MyPlayer._enteredPawns)
            {
                if (pawn.CanPawnMove(moveStep1) || pawn.CanPawnMove(moveStep2))
                {
                    if (pawn.CanPawnMove(moveStep1)) pawn.AvailableMovesText.text = moveStep1.ToString();
                    else if (pawn.CanPawnMove(moveStep2)) pawn.AvailableMovesText.text = moveStep2.ToString();

                    pawn.IsPawnMovable = true;
                    firstMovablePawn = firstMovablePawn ?? pawn;
                    movablePawnCount++;
                }
            }

            // Handle no movable pawns
            if (movablePawnCount == 0)
            {
                Debug.Log("2");
                GameManager.INSTANCE.ChangeTurn();
                return;
            }

            if (movablePawnCount == 1)
            {
                // trying bigger step first
                if (moveStep1 >= moveStep2)
                {
                    if (firstMovablePawn.CanPawnMove(moveStep1))
                    {
                        firstMovablePawn.MovePawn(moveStep1);
                    }
                    else
                    {
                        firstMovablePawn.MovePawn(moveStep2);
                    }
                }
                else
                {
                    if (firstMovablePawn.CanPawnMove(moveStep2))
                    {
                        firstMovablePawn.MovePawn(moveStep2);
                    }
                    else
                    {
                        firstMovablePawn.MovePawn(moveStep1);
                    }
                }

                return;
            }

            // Handle multiple movable pawns
            foreach (Pawn pawn in MyPlayer._enteredPawns)
            {
                if (pawn.CanPawnMove(moveStep1) && pawn.CanPawnMove(moveStep2))
                {
                    pawn.AvailableMovesText.text = moveStep1 + "," + moveStep2;
                }

                if (pawn.IsPawnMovable)
                {
                    GetBestPawnToMove(moveStep1, moveStep2);
                }
            }
        }

        public void GetBestPawnToMove(int moveStep1, int moveStep2)
        {
            FindBestPawnToMove().MovePawn(moveStep1);
            FindBestPawnToMove().MovePawn(moveStep2);
        }

        public Pawn FindBestPawnToMove()
        {
            Pawn bestPawn = null;
            int randomPawnNumber = Random.Range(0, MyPlayer._enteredPawns.Count);
            bestPawn = MyPlayer._enteredPawns[randomPawnNumber];
            return bestPawn;
        }
    }
}
