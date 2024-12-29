using System.Collections;
using System.Collections.Generic;
using _Scripts.Enums;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Board
{
    public class Node : MonoBehaviour
    {
        public bool IsStarNode;
        public bool IsStartNode;
        public bool IsFinishNode;

        public Player.Player StartNodePlayer;

        private const int MaxPlayersPerNormalNode = 2;

        private int MaxPawnsAllowed => IsFinishNode ? 4 : MaxPlayersPerNormalNode;
    
        public List<Pawn> PawnsOnNode = new List<Pawn>();
        
        [SerializeField] private PawnArrangeType ArrangeType;

        // checking if the pawn can enter a given node / path
        public bool CanPawnEnter(Pawn pawn)
        {
            if (PawnsOnNode.Count == MaxPawnsAllowed)
            {
                return false;
            }
            
            if (PawnsOnNode.Count < MaxPawnsAllowed)
            {
                if (IsStartNode && StartNodePlayer == pawn.MainPlayer)
                {
                    return true;
                }
                
                foreach (Pawn p in PawnsOnNode)
                {
                    if (IsStartNode && StartNodePlayer != pawn.MainPlayer)
                    {
                        if (StartNodePlayer != p.MainPlayer)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    
                    if (p.MainPlayer == pawn.MainPlayer)
                    {
                        return true;
                    }
                    if (IsStarNode && p.MainPlayer != pawn.MainPlayer)
                    {
                        return false;
                    }
                    if (IsStarNode && p.MainPlayer != pawn.MainPlayer)
                    {
                        return true;
                    }
                }
            }
            return true;
        }

        // add pawn to this Node
        public void AddPawn(Pawn pawn)
        {
            if (!CanPawnEnter(pawn))
            {
                return;
            }
            PawnsOnNode.Add(pawn);
            pawn.CurrentNode = this;
            StartCoroutine(PositionPawnsCo());

            if (IsFinishNode)
            {
                pawn.MainPlayer.HasBonusMove = true;
                pawn.MainPlayer.BonusMove = 10;
            }
        }

        // remove pawn from this node
        public void RemovePawn(Pawn pawn)
        {
            if (PawnsOnNode.Contains(pawn))
            {
                PawnsOnNode.Remove(pawn);
            }
            PositionPawns();
        }
        
        private IEnumerator PositionPawnsCo()
        {
            yield return new WaitForSeconds(0.01f);
            PositionPawns();
        }
        
        public void PositionPawns()
        {
            float offset = 0.2f;
            float changeScale = 0.7f;
            if (PawnsOnNode.Count == 0)
            {
                return;
            }
            
            if (PawnsOnNode.Count == 2)
            {
                if (ArrangeType == PawnArrangeType.HORIZONTAL)
                {
                    PawnsOnNode[0].transform.position = transform.position + new Vector3(-offset, 0, 0);
                    PawnsOnNode[1].transform.position = transform.position + new Vector3(offset, 0, 0);
                    PawnsOnNode[0].transform.localScale = Vector3.one * changeScale;
                    PawnsOnNode[1].transform.localScale = Vector3.one * changeScale;
                }
                else if (ArrangeType == PawnArrangeType.VERTICAL)
                {
                    PawnsOnNode[0].transform.position = transform.position + new Vector3(0, -offset, 0);
                    PawnsOnNode[1].transform.position = transform.position + new Vector3(0, offset, 0);
                    PawnsOnNode[0].transform.localScale = Vector3.one * changeScale;
                    PawnsOnNode[1].transform.localScale = Vector3.one * changeScale;
                }
                else
                {
                    PawnsOnNode[0].transform.position = transform.position + new Vector3(0, -offset, 0);
                    PawnsOnNode[1].transform.position = transform.position + new Vector3(0, offset, 0);
                    PawnsOnNode[0].transform.localScale = Vector3.one * changeScale;
                    PawnsOnNode[1].transform.localScale = Vector3.one * changeScale;
                }
            }
            else if (PawnsOnNode.Count == 1)
            {
                PawnsOnNode[0].transform.position = transform.position;
                PawnsOnNode[0].transform.localScale = Vector3.one;
            }
            else if (PawnsOnNode.Count == 3)
            {
                PawnsOnNode[0].transform.position = transform.position + new Vector3(-offset, 0, 0);
                PawnsOnNode[1].transform.position = transform.position + new Vector3(offset, 0, 0);
                PawnsOnNode[2].transform.position = transform.position + new Vector3(0, offset, 0);
                PawnsOnNode[0].transform.localScale = Vector3.one * changeScale;
                PawnsOnNode[1].transform.localScale = Vector3.one * changeScale;
                PawnsOnNode[2].transform.localScale = Vector3.one * changeScale;
            }
            else if (PawnsOnNode.Count == 4)
            {
                PawnsOnNode[0].transform.position = transform.position + new Vector3(-offset, 0, 0);
                PawnsOnNode[1].transform.position = transform.position + new Vector3(offset, 0, 0);
                PawnsOnNode[2].transform.position = transform.position + new Vector3(0, offset, 0);
                PawnsOnNode[3].transform.position = transform.position + new Vector3(0, -offset, 0);
                PawnsOnNode[0].transform.localScale = Vector3.one * changeScale;
                PawnsOnNode[1].transform.localScale = Vector3.one * changeScale;
                PawnsOnNode[2].transform.localScale = Vector3.one * changeScale;
                PawnsOnNode[3].transform.localScale = Vector3.one * changeScale;
            }
        }

        public void EliminatePawn(Pawn pawn)
        {
            if(IsStarNode) return;
            
            foreach(Pawn p in new List<Pawn>(PawnsOnNode))
            {
                if(p.MainPlayer != pawn.MainPlayer)
                {
                    p.ResetToHomePosition();
                    pawn.MainPlayer.OtherPawnKillCount++;
                    if (pawn.MainPlayer.OtherPawnKillCount > 0)
                    {
                        pawn.MainPlayer.HasBonusMove = true;
                        pawn.MainPlayer.BonusMove = 20;
                    }
                    if (p.MainPlayer._pawnsInPlay < 0)
                    {
                        p.MainPlayer._pawnsInPlay = 0;
                    }
                    else
                    {
                        p.MainPlayer._pawnsInPlay--;
                    }
                    // Debug.Log($"Pawn {p.name} eliminated by {pawn.name}.");
                }
                
                // if(p.MainPlayer.PawnPath[0])
            }
        }
    }
}
