using System.Collections.Generic;
using _Scripts.Enums;
using _Scripts.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Board
{
    public class Node : MonoBehaviour
    {
        public bool IsStarNode;
        public bool IsStartNode;
        public bool IsFinishNode;

        private const int MaxPlayersPerNormalNode = 2;
        
        public int MaxPawnsAllowed => IsFinishNode ? 4 : MaxPlayersPerNormalNode;
    
        public List<Pawn> PawnsOnNode = new List<Pawn>();
        
        [SerializeField] private PawnArrangeType ArrangeType;

        // checking if the pawn can enter a given node / path
        public bool CanPawnEnter(Pawn pawn)
        {
            int maxPawnsAllowed = IsFinishNode ? 4 : MaxPlayersPerNormalNode;
            
            if (PawnsOnNode.Count == MaxPawnsAllowed)
            {
                return false;
            }
            if (PawnsOnNode.Count < MaxPawnsAllowed)
            {
                foreach (Pawn p in PawnsOnNode)
                {
                    if (p.MainPlayer == pawn.MainPlayer)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
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
        }

        // remove pawn from this node
        public void RemovePawn(Pawn pawn)
        {
            if(PawnsOnNode.Contains(pawn))
                PawnsOnNode.Remove(pawn);

            PositionPawns();
        }
        
        public void PositionPawns()
        {
            float offset = 0.3f;
            if (PawnsOnNode.Count == 2)
            {
                if (ArrangeType == PawnArrangeType.HORIZONTAL)
                {
                    PawnsOnNode[0].transform.position = transform.position + new Vector3(-offset, 0, 0);
                    PawnsOnNode[1].transform.position = transform.position + new Vector3(offset, 0, 0);
                }
                else if (ArrangeType == PawnArrangeType.VERTICAL)
                {
                    PawnsOnNode[0].transform.position = transform.position + new Vector3(0, -offset, 0);
                    PawnsOnNode[1].transform.position = transform.position + new Vector3(0, offset, 0);
                }
                
            }
            else if (PawnsOnNode.Count == 3)
            {
                PawnsOnNode[0].transform.position = transform.position + new Vector3(-offset, 0, 0);
                PawnsOnNode[1].transform.position = transform.position + new Vector3(offset, 0, 0);
                PawnsOnNode[2].transform.position = transform.position + new Vector3(0, offset, 0);
            }
            else if (PawnsOnNode.Count == 4)
            {
                PawnsOnNode[0].transform.position = transform.position + new Vector3(-offset, 0, 0);
                PawnsOnNode[1].transform.position = transform.position + new Vector3(offset, 0, 0);
                PawnsOnNode[2].transform.position = transform.position + new Vector3(0, offset, 0);
                PawnsOnNode[3].transform.position = transform.position + new Vector3(0, -offset, 0);
            }
        }

        public void EliminatePawn(Pawn pawn)
        {
            if(IsStarNode) return;
            
            foreach(Pawn p in new List<Pawn>(PawnsOnNode))
            {
                if(p.MainPlayer != pawn.MainPlayer)
                {
                    // p.ResetToHomePosition();
                    Debug.Log($"Pawn {p.name} eliminated by {pawn.name}.");
                }
            }
        }
    }
}