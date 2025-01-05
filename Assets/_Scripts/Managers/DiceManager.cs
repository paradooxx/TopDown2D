using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Board;
using _Scripts.Utils;
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
        public List<DiceState> DiceStates = new List<DiceState>();
        public int CustomDice1Value = 5, CustomDice2Value = 2;
        
        

        private void Start()
        {
            ResetDiceImage();
        }

        private void OnMouseDown()
        {
            OnDiceButtonClicked?.Invoke();
        }

        public void RollDice(Action<int, int> onDiceRolled)
        {
            StartCoroutine(RollDiceTask(onDiceRolled));
        }

        // animating and deciding the dice roll result
        public SerializableQueue previousSums = new SerializableQueue();

        private IEnumerator RollDiceTask(Action<int, int> onDiceRolled)
        {
            float elapsedTime = 0.0f;

            // Animation loop
            while (elapsedTime < TotalAnimationTime)
            {
                Dice1Sprite.sprite = DiceAnimationSprites[Random.Range(0, DiceAnimationSprites.Length)];
                Dice2Sprite.sprite = DiceAnimationSprites[Random.Range(0, DiceAnimationSprites.Length)];

                yield return new WaitForSeconds(DiceSpriteChangeTime);

                elapsedTime += DiceSpriteChangeTime;
            }

            if (CustomDiceResult)
            {
                Dice1Result = CustomDice1Value;
                Dice2Result = CustomDice2Value;
            }
            else
            {
                do
                {
                    Dice1Result = Random.Range(1, 7);
                    Dice2Result = Random.Range(1, 7);
                } while (IsThirdConsecutiveSum(Dice1Result + Dice2Result));

                // Update sum history
                previousSums.Enqueue(Dice1Result + Dice2Result);
                if (previousSums.Count > 3)
                {
                    previousSums.Dequeue();
                }
            }

            // Update sprites and invoke callback
            Dice1Sprite.sprite = DiceAnimationSprites[Dice1Result - 1];
            Dice2Sprite.sprite = DiceAnimationSprites[Dice2Result - 1];

            // DiceStates.Clear();
            // AddDiceState(new DiceState(Dice1Result, true));
            // AddDiceState(new DiceState(Dice2Result, true));


            onDiceRolled?.Invoke(Dice1Result, Dice2Result);
        }

        public void AddDiceState(DiceState diceState)
        {
            DiceStates.Add(diceState);
        }

        public void DisableDiceState(int result)
        {
            var find = DiceStates.Find(x => x.DiceResult == result);
            if (find != null) find.diceState = false;
        }


        private bool IsThirdConsecutiveSum(int currentSum)
        {
            if (previousSums.Count < 2)
                return false;

            return previousSums.Queue.All(sum => sum == currentSum);
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
        }

        public void SetDice2Result(int value)
        {
            CustomDiceResult = true;
            CustomDice2Value = value;
        }
        public void EnableDiceTouch()
        {
            DiceCollider.enabled = true;
        }

        public void DisableDiceTouch()
        {
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
    }
}