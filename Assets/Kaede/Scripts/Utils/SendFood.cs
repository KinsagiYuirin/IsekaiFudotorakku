using System;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.Utils
{
    public class SendFood : MonoBehaviour
    {
        [SerializeField] private GameObject food;
        [SerializeField] private Transform startPosition;
        [SerializeField] private Transform endPosition;
        
        [SerializeField] private float duration = 0.8f;
        [SerializeField] private Ease ease = Ease.OutCubic;
        [SerializeField] private float delay = 0.1f;
        [field: SerializeField] public Image FoodSprite { get; private set; }
        
        private Tween _moveTween;
        
        void Start()
        {
            gameObject.transform.position = startPosition.position;
            SetActive(false);
        }
        
        private void SetActive(bool active)
        {
            food.SetActive(active);
        }
        
        public async void SetToStartPosition(Sprite sprite)
        {
            if (_moveTween.isAlive) _moveTween.Stop();
            food.transform.position = startPosition.position;
            FoodSprite.sprite = sprite;
            SetActive(true);

            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            SendFoodToPosition();
        }

        private void SendFoodToPosition()
        {
            if (_moveTween.isAlive) _moveTween.Stop();
            if ((food.transform.position - endPosition.position).sqrMagnitude < 0.0001f)
            {
                SetActive(false);
                return;
            }

            _moveTween = Tween.Position(
                food.transform, startPosition.position, endPosition.position, duration, ease, useUnscaledTime: false);
        }

    }
}
