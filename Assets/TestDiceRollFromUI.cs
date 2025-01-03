using _Scripts.Managers;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class TestDiceRollFromUI : MonoBehaviour
{
    [SerializeField] private Button[] _test1Buttons;
    [SerializeField] private Button[] _test2Buttons;

    [SerializeField] private GameManager _gameManager;

    private TMP_Text[] _test1ButtonText;
    private TMP_Text[] _test2ButtonText;
    
    [SerializeField] private TMP_Text dice1ResultText;
    [SerializeField] private TMP_Text dice2ResultText;
    
    private int _dice1Result = 3;
    private int _dice2Result = 4;
    private void Start()
    {
        _test1ButtonText = new TMP_Text[_test1Buttons.Length];
        _test2ButtonText = new TMP_Text[_test2Buttons.Length];
        for (int i = 0; i < _test1Buttons.Length; i++)
        {
            
            _test1ButtonText[i] = _test1Buttons[i].GetComponentInChildren<TMP_Text>();
            _test2ButtonText[i] = _test2Buttons[i].GetComponentInChildren<TMP_Text>();
        }

        OnDiceButtonClick();
        
    }
    private void OnDiceButtonClick()
    {
        for (int i = 0; i < _test1Buttons.Length; i++)
        {
            int index = i;
            _test1Buttons[i].onClick.AddListener(() => DiceClickFunction1(index));
            _test2Buttons[i].onClick.AddListener(() => DiceClickFunction2(index));
        }
    }

    private void DiceClickFunction1(int index)
    {
        _gameManager.CurrentPlayer.DiceManager.SetDice1Result(index + 1);
        dice1ResultText.text = (index + 1).ToString();
        _dice1Result = index;
    }

    private void DiceClickFunction2(int index)
    {
        _gameManager.CurrentPlayer.DiceManager.SetDice2Result(index + 1);
        dice2ResultText.text = (index + 1).ToString();
        _dice2Result = index;
    }
}
