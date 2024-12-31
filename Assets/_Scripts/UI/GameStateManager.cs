using _Scripts.Enums;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.UI
{
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField] private GameState defaultGameState;

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
        }

        private void Start()
        {
            SetState(defaultGameState);
        }

        public void SetState(GameState gameState)
        {
            CurrentGameState = gameState;
            OnStateChange?.Invoke(gameState);
        }
    }
}
