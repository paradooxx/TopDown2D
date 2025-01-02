using System.Collections.Generic;
using _Scripts;
using _Scripts.Enums;
using _Scripts.Managers;
using _Scripts.Player;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectionController : MonoBehaviour
{
    private static PlayerSelectionController _instance;

    // Start is called before the first frame update
    List<MyModeManager> _modes = new List<MyModeManager>();

    [SerializeField] private GameManager GameManager;
    [SerializeField] private Player[] Player;
    [SerializeField] private Button[] PlayerSelectButtons;

    public static PlayerSelectionController GetInstance()
    {
        return _instance == null ? FindObjectOfType<PlayerSelectionController>() : _instance;
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        if (PlayerPrefs.HasKey("PlayerDataType"))
        {
            StaticDatas.PLAYERS_DATA_TYPES = JsonUtility
                .FromJson<ListOfPlayerData>(PlayerPrefs.GetString("PlayerDataType"));
            SavePlayerDataInput();
        }
        else
        {
            ListOfPlayerData data = new ListOfPlayerData();
            data.playerDataList = new List<PlayerDataType>();
            data.playerDataList.Add(new PlayerDataType(0));
            data.playerDataList.Add(new PlayerDataType(0));
            data.playerDataList.Add(new PlayerDataType(0));
            data.playerDataList.Add(new PlayerDataType(0));
            StaticDatas.PLAYERS_DATA_TYPES = data;
            SavePlayerDataInput();
        }
    }

    public void StartGame()
    {
        SavePlayerDataInput();
    }

    public void SavePlayerDataInput()
    {
        string dataType = JsonUtility.ToJson(StaticDatas.PLAYERS_DATA_TYPES);
        PlayerPrefs.SetString("PlayerDataType", dataType);
        Debug.Log(dataType);
    }
}