using _Scripts.Enums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace _Scripts.UI
{
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField] private GameState DefaultGameState;

        public static GameStateManager Instance { get; private set; }
        public static GameState CurrentGameState { get; private set; }

        public static event UnityAction<GameState> OnStateChange;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            SetState(DefaultGameState);
        }

        public void SetState(GameState gameState)
        {
            CurrentGameState = gameState;
            OnStateChange?.Invoke(gameState);
        }
    }
}
