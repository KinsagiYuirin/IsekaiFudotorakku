using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class SendFood : MonoBehaviour
{
    [SerializeField] private GameObject food;
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform middlePosition;   // จุดกลาง
    [SerializeField] private Transform endPosition;

    [SerializeField] private float duration = 0.8f;      // เวลารวม Start→Middle→End
    [SerializeField] private Ease ease = Ease.OutCubic;
    [SerializeField] private float middleDelay = 0.25f;  // << หน่วงที่จุดกลาง (วินาที)
    [SerializeField, Range(0.05f, 0.95f)]
    private float firstLegRatio = 0.5f;                  // สัดส่วนเวลา Start→Middle

    [field: SerializeField] public Image FoodSprite { get; private set; }

    private Tween _moveTween;
    private CancellationTokenSource _middleDelayCts;

    void Start()
    {
        food.transform.position = startPosition.position;
        SetActive(false);
    }

    private void SetActive(bool active)
    {
        food.SetActive(active);
    }

    public async void SetToStartPosition(Sprite sprite)
    {
        // ยกเลิกทุกอย่างก่อนเริ่มใหม่
        if (_moveTween.isAlive) _moveTween.Stop();
        _middleDelayCts?.Cancel();
        _middleDelayCts = null;

        food.transform.position = startPosition.position;
        FoodSprite.sprite = sprite;
        SetActive(true);

        SendFoodToPosition();
    }

    private void SendFoodToPosition()
    {
        if (_moveTween.isAlive) _moveTween.Stop();
        _middleDelayCts?.Cancel();
        _middleDelayCts = new CancellationTokenSource();

        // ถึงปลายทางแล้วก็ซ่อน
        if ((food.transform.position - endPosition.position).sqrMagnitude < 0.0001f) {
            SetActive(false);
            return;
        }

        // แบ่งเวลาเป็น 2 ช่วง
        float d1 = Mathf.Max(0.0001f, duration * firstLegRatio);
        float d2 = Mathf.Max(0.0001f, duration * (1f - firstLegRatio));

        // ช่วง 1: ไป middle ก่อน
        _moveTween = Tween.Position(
            food.transform,
            food.transform.position,
            middlePosition.position,
            d1,
            ease,
            useUnscaledTime: false
        )
        .OnComplete(() =>
        {
            PauseAtMiddleThenGo(d2, _middleDelayCts.Token).Forget();
        });
    }

    private async UniTaskVoid PauseAtMiddleThenGo(float secondLegDuration, CancellationToken token)
    {
        try {
            // หน่วงที่ middle
            if (middleDelay > 0f) {
                await UniTask.Delay(TimeSpan.FromSeconds(middleDelay), cancellationToken: token);
            }
        }
        catch (OperationCanceledException) {
            return;
        }

        // ช่วง 2: middle → end
        _moveTween = Tween.Position(
            food.transform,
            middlePosition.position,
            endPosition.position,
            secondLegDuration,
            ease,
            useUnscaledTime: false
        )
        .OnComplete(() =>
        {
            food.transform.position = endPosition.position;
            SetActive(false);
        });
    }

    // ถ้าต้องการปุ่ม/เมธอดรีเซ็ตทันที
    public void ResetNow()
    {
        if (_moveTween.isAlive) _moveTween.Stop();
        _middleDelayCts?.Cancel();
        _middleDelayCts = null;
        food.transform.position = startPosition.position;
        SetActive(false);
    }
}
