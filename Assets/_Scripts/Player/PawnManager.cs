using System;
using System.Collections.Generic;
using _Scripts.Player;
using UnityEngine;

public class PawnManager : MonoBehaviour
{
    public List<Pawn> MyPlayerPawns = new List<Pawn>();
    public Player MyPlayer;


    [ContextMenu("Add player reference")]
    void AddPlayerReference()
    {
        MyPlayer = GetComponent<Player>();
    }

    public List<Pawn> GetEnteredPawns()
    {
        List<Pawn> enteredPawns = new List<Pawn>();
        foreach (var pawn in MyPlayerPawns)
        {
            if (pawn.IsInPlay)
            {
                enteredPawns.Add(pawn);
            }
        }
        return enteredPawns;
    }

    public List<Pawn> GetNotStartedPawns()
    {
        List<Pawn> notStartedPawns = new List<Pawn>();
        foreach (var pawn in MyPlayer._myPawns)
        {
            if (!pawn.IsInPlay && !pawn.IsHome) notStartedPawns.Add(pawn);
        }

        return notStartedPawns;
    }
}