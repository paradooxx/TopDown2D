using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace _Scripts.Managers
{
    public class DiceManager : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer Dice1Sprite;
        [SerializeField] private SpriteRenderer Dice2Sprite;

        [SerializeField] private Sprite[] DiceAnimationSprites;
        [SerializeField] private Sprite ResetDiceSprite;

        public int Dice1Result;
        public int Dice2Result;

        [SerializeField] private float TotalAnimationTime = 2.0f;
        [SerializeField] private float DiceSpriteChangeTime = 0.01f;
        
        private Collider2D _diceCollider;
        public static event Action OnDiceRollFinished;
        public UnityEvent OnDiceButtonClicked;

        private void Start() => _diceCollider = GetComponent<Collider2D>();

        private void OnMouseDown()
        {
            OnDiceButtonClicked?.Invoke();
        }

        public int[] RollDice(System.Action<int, int> onDiceRolled)
        {
            StartCoroutine(RollDiceTask(onDiceRolled));

            return new int[] {Dice1Result, Dice2Result};
        }

        // animating and deciding the dice roll result
        private IEnumerator RollDiceTask(System.Action<int, int> onDiceRolled)
        {
            float elapsedTime = 0.0f;

            while (elapsedTime < TotalAnimationTime)
            {
                Dice1Sprite.sprite = DiceAnimationSprites[Random.Range(0, DiceAnimationSprites.Length)];
                Dice2Sprite.sprite = DiceAnimationSprites[Random.Range(0, DiceAnimationSprites.Length)];

                yield return new WaitForSeconds(DiceSpriteChangeTime); // Task.Delay takes milliseconds

                elapsedTime += DiceSpriteChangeTime;
            }

            Dice1Result = Random.Range(1, 7);
            Dice2Result = Random.Range(1, 7);

            Dice1Sprite.sprite = DiceAnimationSprites[Dice1Result - 1];
            Dice2Sprite.sprite = DiceAnimationSprites[Dice2Result - 1];

            onDiceRolled?.Invoke(Dice1Result, Dice2Result);
        }

        public void DisableDiceCollider()
        {
            _diceCollider.enabled = false;
        }

        public void EnableDiceCollider()
        {
            _diceCollider.enabled = true;
        }

        public void ResetDice()
        {
            Dice1Sprite.sprite = ResetDiceSprite;
            Dice2Sprite.sprite = ResetDiceSprite;
        }
    }
}