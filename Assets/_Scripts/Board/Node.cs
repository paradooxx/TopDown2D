using System;
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
            if (PawnsOnNode.Count == 0) return;

            float offset = 0.2f;
            float changeScale = 0.9f;
            var positions = GetPawnPositions(PawnsOnNode.Count, offset);

            // apply positions and scaling to all pawns
            for (int i = 0; i < PawnsOnNode.Count; i++)
            {
                PawnsOnNode[i].transform.position = transform.position + positions[i];
                PawnsOnNode[i].transform.localScale = Vector3.one * (i == 0 && PawnsOnNode.Count == 1 ? 1f : changeScale);
                
                SpriteRenderer spriteRenderer = PawnsOnNode[i].PawnMainSprite;

                if (spriteRenderer)
                {
                    spriteRenderer.sortingOrder = PawnsOnNode.Count == 1 ? 10 : 10 + i;
                }
            }
        }

        private Vector3[] GetPawnPositions(int pawnCount, float offset)
        {
            switch (pawnCount)
            {
                case 1:
                    return new[] { Vector3.zero };
                
                case 2:
                    return ArrangeType switch
                    {
                        PawnArrangeType.HORIZONTAL => new[] {
                            new Vector3(-offset, 0, 0),
                            new Vector3(offset, 0, 0)
                        },
                        _ => new[] {
                            new Vector3(0, -offset, 0),
                            new Vector3(0, offset, 0)
                        }
                    };
                
                case 3:
                    return new[] {
                        new Vector3(-offset, 0, 0),
                        new Vector3(offset, 0, 0),
                        new Vector3(0, offset, 0)
                    };
                
                case 4:
                    return new[] {
                        new Vector3(-offset, 0, 0),
                        new Vector3(offset, 0, 0),
                        new Vector3(0, offset, 0),
                        new Vector3(0, -offset, 0)
                    };
                
                default:
                    return Array.Empty<Vector3>();
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
                }
            }
        }
    }
}
