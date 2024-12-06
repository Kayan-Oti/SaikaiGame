using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
using MyBox;

public class CameraShake : MonoBehaviour
{
    [ButtonMethod]
    public void CameraShakeShot(){
        Tween.ShakeCamera(Camera.main, strengthFactor: 0.5f, duration: 1f, frequency: 10);
    }
}
