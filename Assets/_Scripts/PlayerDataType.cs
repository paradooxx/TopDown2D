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
                break;
            case PlayerType.BOT:
                return 1;
                break;
            case PlayerType.NONE:
                return 2;
                break;
            default:
                return 0;
                break;
        }

        return 1;
    }

    public static PlayerType GetPlayerEnumFromId(int playerId)
    {
        switch (playerId)
        {
            case 0:
                return PlayerType.PLAYER;
                break;
            case 1:
                return PlayerType.BOT;
                break;
            case 2:
                return PlayerType.NONE;
                break;
            default:
                return PlayerType.PLAYER;
                break;
        }

        return PlayerType.BOT;
    }
}