using System;
using System.Collections.Generic;
using _Scripts.Enums;
using _Scripts.Player;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerDataType
{
    public int TypeId;
    
    public PlayerDataType(int id)
    {
        this.TypeId = id;
    }
}

[Serializable]
public class ListOfPlayerData
{
    public List<PlayerDataType> playerDataList = new List<PlayerDataType>();
}

public static class Util
{
    public static int GetPlayerIdFromEnum(PlayerType pType)
    {
        switch (pType)
        {
            case PlayerType.PLAYER:
                return 0;
            case PlayerType.BOT:
                return 1;
            case PlayerType.NONE:
                return 2;
            default:
                return 0;
        }
    }

    public static PlayerType GetPlayerEnumFromId(int playerId)
    {
        switch (playerId)
        {
            case 0:
                return PlayerType.PLAYER;
            case 1:
                return PlayerType.BOT;
            case 2:
                return PlayerType.NONE;
            default:
                return PlayerType.PLAYER;
        }
    }
}