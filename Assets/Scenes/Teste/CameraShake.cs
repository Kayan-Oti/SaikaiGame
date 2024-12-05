using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
using MyBox;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    [ButtonMethod]
    public void CameraShakeShot(){
        PrimeTween.Tween.ShakeCamera(Camera.main, strengthFactor: 0.5f, duration: 1f, frequency: 10);
    }

    [ButtonMethod]
    public void CameraShakeShotDOTWeen(){
        Camera.main.DOShakePosition(1,0.5f);
    }
}
