using System.Collections.Generic;
using _Scripts.Managers;
using UnityEngine;

namespace _Scripts.Board
{
    public class BoardStateManager
    {
        private const string SaveKey = "BoardStateSave";

        public void SaveBoardState(BoardState state)
        {
            SaveData saveData = new SaveData
            {
                PlayerStates = state.PlayerStates,
                CurrentPlayerIndex = state.currentPlayerIndex
            };

            string json = JsonUtility.ToJson(saveData, true);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();

            Debug.Log($"Saved Board State:\n{json}");
        }
        
        // initializing the default board state
        public BoardState LoadDefaultBoardState(List<Player.Player> players, GameManager gameManager)
        {
            DeleteSavedState();
            return new BoardState(players, gameManager);
        }

        public BoardState LoadBoardState()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                Debug.LogWarning("No saved board state found!");
                return null;
            }

            string json = PlayerPrefs.GetString(SaveKey);
            SaveData loadedData = JsonUtility.FromJson<SaveData>(json);

            Debug.Log($"Loaded Board State:\n{json}");

            return new BoardState(loadedData);
        }

        public static bool HasSavedState()
        {
            return PlayerPrefs.HasKey(SaveKey);
        }

        public void DeleteSavedState()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
            Debug.Log("Save state deleted");
        }
    }
}