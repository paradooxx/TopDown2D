using System;
using System.Collections;
using TMPro;
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

        [SerializeField] private Collider2D DiceCollider;
        public UnityEvent OnDiceButtonClicked;

        public bool CustomDiceResult = true;
        public int CustomDice1Value = 5, CustomDice2Value = 2;

        [SerializeField] private TMP_Text CustomDice1Text;
        [SerializeField] private TMP_Text CustomDice2Text;

        private void Start()
        {
            ResetDiceImage();
        }

        private void OnMouseDown()
        {
            OnDiceButtonClicked?.Invoke();
        }

        public int[] RollDice(Action<int, int> onDiceRolled)
        {
            StartCoroutine(RollDiceTask(onDiceRolled));

            return new int[] { Dice1Result, Dice2Result };
        }

        // animating and deciding the dice roll result
        private IEnumerator RollDiceTask(Action<int, int> onDiceRolled)
        {
            float elapsedTime = 0.0f;

            while (elapsedTime < TotalAnimationTime)
            {
                Dice1Sprite.sprite = DiceAnimationSprites[Random.Range(0, DiceAnimationSprites.Length)];
                Dice2Sprite.sprite = DiceAnimationSprites[Random.Range(0, DiceAnimationSprites.Length)];

                yield return new WaitForSeconds(DiceSpriteChangeTime); // Task.Delay takes milliseconds

                elapsedTime += DiceSpriteChangeTime;
            }

            if (CustomDiceResult)
            {
                Dice1Result = CustomDice1Value;
                Dice2Result = CustomDice2Value;

                Dice1Sprite.sprite = DiceAnimationSprites[Dice1Result - 1];
                Dice2Sprite.sprite = DiceAnimationSprites[Dice2Result - 1];

                onDiceRolled?.Invoke(Dice1Result, Dice2Result);
            }
            else
            {
                Dice1Result = Random.Range(1, 7);
                Dice2Result = Random.Range(1, 7);

                Dice1Sprite.sprite = DiceAnimationSprites[Dice1Result - 1];
                Dice2Sprite.sprite = DiceAnimationSprites[Dice2Result - 1];

                onDiceRolled?.Invoke(Dice1Result, Dice2Result);
            }
        }

        public void DisableDiceCollider()
        {
            DiceCollider.enabled = false;
        }

        public void EnableDiceCollider()
        {
            DiceCollider.enabled = true;
        }

        public void ResetDiceImage(bool resetDice = true)
        {
            if (resetDice)
            {
                Dice1Sprite.sprite = ResetDiceSprite;
                Dice2Sprite.sprite = ResetDiceSprite;
                EnableDiceCollider();
            }
        }

        public void SetDice1Result(int value)
        {
            CustomDiceResult = true;
            CustomDice1Value = value;
            CustomDice1Text.text = CustomDice1Value.ToString();
        }

        public void SetDice2Result(int value)
        {
            CustomDiceResult = true;
            CustomDice2Value = value;
            CustomDice2Text.text = CustomDice2Value.ToString();
        }

        private bool _canRoll;

        public void EnableDiceTouch()
        {
            _canRoll = true;
            DiceCollider.enabled = true;
        }

        public void DisableDiceTouch()
        {
            _canRoll = false;
            DiceCollider.enabled = false;
        }

        public void ActivateDice()
        {
            gameObject.SetActive(true);
            Dice1Sprite.gameObject.SetActive(true);
            Dice2Sprite.gameObject.SetActive(true);
        }

        public void DeactivateDice()
        {
            DisableDiceTouch();
            gameObject.SetActive(false);
            Dice1Sprite.gameObject.SetActive(false);
            Dice2Sprite.gameObject.SetActive(false);
        }

        public void DimDice1Sprite()
        {
            Dice1Sprite.color = new Color(0.29f, 0.29f, 0.29f);
        }

        public void DimDice2Sprite()
        {
            Dice2Sprite.color = new Color(0.29f, 0.29f, 0.29f);
        }

        public void ResetDiceImage()
        {
            Dice1Sprite.color = Color.white;
            Dice1Sprite.color = Color.white;
        }
    }
}