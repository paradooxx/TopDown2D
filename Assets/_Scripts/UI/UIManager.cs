using System.Collections.Generic;
using _Scripts.Enums;
using _Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject MainMenuPanel;
        [SerializeField] private GameObject NumOfPlayersSelectPanel;
        [SerializeField] private GameObject PlayerColorSelectPanel;
        [SerializeField] private GameObject GameFinishedPanel;

        [SerializeField] private GameObject MainBg;
        
        [SerializeField] private List<Button> PlayerSelectButtons;

        [SerializeField] private TMP_Text WinText;

        private GameObject _activePanel;
        private List<GameObject> _unactivePanels;

        private void OnEnable() => GameStateManager.OnStateChange += ManagePanels;

        private void OnDisable() => GameStateManager.OnStateChange -= ManagePanels;

        private void Awake()
        {
            _unactivePanels = new List<GameObject> { MainMenuPanel, NumOfPlayersSelectPanel, PlayerColorSelectPanel, GameFinishedPanel };
            HideAllPanels();
            MainBg.SetActive(true);
        }

        private void Start()
        {
            foreach (var button in PlayerSelectButtons)
            {
                button.onClick.AddListener(() => HighlightButton(button));
            }
            HighlightButton(PlayerSelectButtons[0]);
        }

        private void ManagePanels(GameState state)
        {
            switch(state)
            {
                case GameState.MAIN_MENU:
                    SetActivePanel(MainMenuPanel);
                    break;
                case GameState.PLAYER_SELECT_MENU:
                    SetActivePanel(PlayerColorSelectPanel);
                    break;
                case GameState.GAME_FINISHED:
                    SetActivePanel(GameFinishedPanel);
                    WinText.text = "Winner: " + GameManager.INSTANCE.CurrentPlayer.ToString();
                    break;
                case GameState.MAIN_GAME:
                    HideAllPanels();
                    break;
            }
        }

        private void DisableActivePanel()
        {
            if(_activePanel == null) return;
            _unactivePanels.Add(_activePanel);
            _activePanel.SetActive(false);
            _activePanel = null;
        }

        private void SetActivePanel(GameObject panel)
        {
            DisableActivePanel();
            _unactivePanels.Remove(panel);
            _activePanel = panel;
            panel.SetActive(true);
        }

        private void HideAllPanels()
        {
            DisableActivePanel();
            foreach (GameObject panel in _unactivePanels)
            {
                panel.SetActive(false);
            }
        }

        public void SelectPlayerColor()
        {
            SetActivePanel(PlayerColorSelectPanel);
        }

        public void SelectNumberOfPlayers()
        {
            SetActivePanel(NumOfPlayersSelectPanel);
        }


        //use in UI to get to main menu
        public void CloseButton()
        {
            SetActivePanel(MainMenuPanel);
        }
        
        private void HighlightButton(Button clickedButton)
        {
            foreach (var button in PlayerSelectButtons)
            {
                button.image.color = button.colors.normalColor;
            }
            // Highlight the clicked button with the unique color
            clickedButton.image.color = Color.yellow;
        }
    }
}