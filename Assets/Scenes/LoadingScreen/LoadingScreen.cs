using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private UI_ManagerAnimation _animation;

    public IEnumerator OnStartLoadScene(){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LoadingScreenStart);
        yield return _animation.PlayAnimationCoroutine("Start");
        yield return null;
    }

    public void OnEndLoadScene(Action DoLast){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LoadingScreenEnd);
        _animation.PlayAnimation("End", DoLast);
    }
}
