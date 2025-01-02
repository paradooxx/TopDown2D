using System.Collections;
using System.Collections.Generic;
using _Scripts;
using _Scripts.Enums;
using _Scripts.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyModeManager : MonoBehaviour
{
    private List<PlayerType> playerTypes = new List<PlayerType>();
    [SerializeField] private Button TypeButton;
    [SerializeField] private TMP_Text Text;
    [SerializeField] Player Player;
    [SerializeField] private int MyIndex;

    private PlayerType _currentType;

    // Start is called before the first frame update

    [ContextMenu("set reference from child")]
    private void SetReferenceFromChild()
    {
        TypeButton = GetComponentInChildren<Button>();
        Text = GetComponentInChildren<TMP_Text>();
    }

    void Start()
    {
        playerTypes.Add(PlayerType.BOT);
        playerTypes.Add(PlayerType.PLAYER);
        playerTypes.Add(PlayerType.NONE);
        
        TypeButton.onClick.AddListener(() => { ChangeMode(); });
        
        SetUi();
    }

    private void ChangeMode()
    {
        switch (_currentType)
        {
            case PlayerType.BOT:
                SetType(PlayerType.PLAYER);
                break;
            case PlayerType.PLAYER:
                SetType(PlayerType.NONE);
                break;
            case PlayerType.NONE:
                SetType(PlayerType.BOT);
                break;
            default:
                break;
        }
    }
    
    private void SetType(PlayerType type)
    {
        _currentType = type;
        Text.text = type.ToString();
        StaticDatas.PLAYERS_DATA_TYPES.playerDataList[MyIndex].TypeId = Util.GetPlayerIdFromEnum(type);
        PlayerSelectionController.GetInstance().SavePlayerDataInput();
        //save to json is you want to change json each time
    }

    private void SetUi()
    {
        //get from json 
        PlayerType type = Util.GetPlayerEnumFromId(StaticDatas.PLAYERS_DATA_TYPES.playerDataList[MyIndex].TypeId);
        _currentType = type;
        Text.text = type.ToString();
    }
}