using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Board;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace _Scripts.Managers
{
    public class DiceManager : MonoBehaviour
    {
        [SerializeField] private List<SpriteRenderer> DiceSprites = new List<SpriteRenderer>();

        [SerializeField] private Sprite[] DiceAnimationSprites;
        [SerializeField] private Sprite ResetDiceSprite;

        [SerializeField] private float TotalAnimationTime = 2.0f;
        [SerializeField] private float DiceSpriteChangeTime = 0.01f;

        [SerializeField] private Collider2D DiceCollider;
        public UnityEvent OnDiceButtonClicked;

        public bool CustomDiceResult = true;
        public int CustomDice1Value = 5, CustomDice2Value = 2;


        private void Start()
        {
            // ResetDiceImage();
        }

        private void OnMouseDown()
        {
            OnDiceButtonClicked?.Invoke();
        }


        public IEnumerator AnimateDiceTask(Action onDiceRolled, List<DiceState> diceStates)
        {
            float elapsedTime = 0.0f;

            // Animation loop
            while (elapsedTime < TotalAnimationTime)
            {
                foreach (var s in DiceSprites)
                {
                    s.sprite = DiceAnimationSprites[Random.Range(0, DiceAnimationSprites.Length)];
                }

                yield return new WaitForSeconds(DiceSpriteChangeTime);

                elapsedTime += DiceSpriteChangeTime;
            }

            // for (int i = 0; i < DiceSprites.Count; i++)
            // {
            //     DiceSprites[i].sprite = DiceAnimationSprites[diceStates[i].Value - 1];
            // }
            DiceSprites[0].sprite = DiceAnimationSprites[diceStates[0].Value - 1];
            DiceSprites[1].sprite = DiceAnimationSprites[diceStates[1].Value - 1];


            onDiceRolled?.Invoke();
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
                foreach (var s in DiceSprites)
                {
                    s.sprite = ResetDiceSprite;
                }

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
            foreach (var s in DiceSprites)
            {
                s.gameObject.SetActive(true);
            }
        }

        public void DeactivateDice()
        {
            DisableDiceTouch();
            gameObject.SetActive(false);
            foreach (var s in DiceSprites)
            {
                s.gameObject.SetActive(false);
            }
        }

        public void MakeSpriteNormalColor()
        {
            foreach (var s in DiceSprites)
            {
                s.color = Color.white;
            }
        }

        public void SetDiceSpriteToResult(int index1, int index2)
        {
            DiceSprites[0].sprite = DiceAnimationSprites[index1 - 1];
            DiceSprites[1].sprite = DiceAnimationSprites[index2 - 1];
        }

        public void DimSprite(int index)
        {
            DiceSprites[index].color = new Color(100f / 250f, 100f / 250f, 100f / 250f);
        }
    }
}