using _Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Player
{
    public class PlayerCanvaManager : MonoBehaviour
    {
        [SerializeField] private Player MyPlayer;
        [SerializeField] private int AddIndex;
        [SerializeField] private Button AddButton;
        [SerializeField] private Button RemoveButton;


        public void AddRemovePlayer()
        {
            if (GameManager.GetInstance().StartingPlayers.Contains(MyPlayer))
            {
                AddButton.gameObject.SetActive(false);
                RemoveButton.gameObject.SetActive(true);
            }
            else
            {
                AddButton.gameObject.SetActive(true);
                RemoveButton.gameObject.SetActive(false);
            }
            
            AddButton.onClick.AddListener(() => AddButtonClicked());
            RemoveButton.onClick.AddListener(() => RemoveButtonClicked());
        }

        private void AddButtonClicked()
        {
            MyPlayer.PlayerStateManager.AddNewPlayer(MyPlayer, AddIndex);
            AddButton.gameObject.SetActive(false);
            RemoveButton.gameObject.SetActive(true);
        }

        private void RemoveButtonClicked()
        {
            MyPlayer.PlayerStateManager.RemovePlayer(MyPlayer);
            RemoveButton.gameObject.SetActive(false);
            AddButton.gameObject.SetActive(true);
        }
    }
}
