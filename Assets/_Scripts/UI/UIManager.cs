using System.Collections.Generic;
using _Scripts.Enums;
using _Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject MainMenuPanel;
        [SerializeField] private GameObject NumOfPlayersSelectPanel;
        [SerializeField] private GameObject PlayerColorSelectPanel;
        [SerializeField] private GameObject GameFinishedPanel;

        [SerializeField] private GameObject MainBg;

        [SerializeField] private TMP_Text WinText;

        [SerializeField] private Player.Player[] Players;

        private GameObject _activePanel;
        private List<GameObject> _unactivePanels;

        private void OnEnable() => GameStateManager.OnStateChange += ManagePanels;

        private void OnDisable() => GameStateManager.OnStateChange -= ManagePanels;

        private void Awake()
        {
            _unactivePanels = new List<GameObject> { MainMenuPanel, GameFinishedPanel };
            HideAllPanels();
            // MainBg.SetActive(true);
        }

        private void ManagePanels(GameState state)
        {
            switch (state)
            {
                case GameState.MAIN_MENU:
                    Time.timeScale = 1;
                    SetActivePanel(MainMenuPanel);
                    break;
                case GameState.GAME_FINISHED:
                    SetActivePanel(GameFinishedPanel);
                    Time.timeScale = 0;
                    WinText.text = "Winner: " + GameManager.GetInstance().FinishedPlayers[0];
                    break;
                case GameState.MAIN_GAME:
                    HideAllPanels();
                    break;
                case GameState.CONTINUE_GAME:
                    Time.timeScale = 0;
                    break;
            }
        }

        private void DisableActivePanel()
        {
            if (_activePanel == null) return;
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

        //use in UI to get to main menu
        public void CloseButton()
        {
            SetActivePanel(MainMenuPanel);
        }

        // private void HighlightButton(Button clickedButton)
        // {
        //     foreach (var button in PlayerSelectButtons)
        //     {
        //         button.image.color = new Color32(130, 130, 130, 255);
        //     }
        //     // Highlight the clicked button with the unique color
        //     clickedButton.image.color = Color.black;
        // }

        public void ReloadScene()
        {
            SceneManager.LoadScene(0);
        }
    }
}