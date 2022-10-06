using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovingPlatform : MonoBehaviour
{
    public Vector3 endLocation;
    public float moveTime;

    private void Start()
    {
        transform.DOMove(endLocation, moveTime).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }
}
