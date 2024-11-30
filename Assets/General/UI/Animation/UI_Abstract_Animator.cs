using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public abstract class UI_Abstract_Animator : MonoBehaviour
{
    [SerializeField] protected bool _startInteractable;
    [SerializeField] protected bool _startVisibility;

    protected Tween _animationTween;
    protected AnimationStruct _animationStruct;
    protected float _duration;
    protected AnimationCurve _ease;
    protected CanvasGroup _canvasGroup;
    protected RectTransform _rectTransform;


    #region --- --- --- Abstract Methods
    public abstract void SetValues();
    public abstract void SetComponents();
    public abstract Tween GetTween();

    #endregion

    void Start()
    {
        //Get Components
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();

        SetValues();
        SetInteractable(_startInteractable);
        SetVisibility(_startVisibility);
    }

    public void SetInteractable(bool state){
        _canvasGroup.interactable = state;
        _canvasGroup.blocksRaycasts = state;
    }

    public void SetVisibility(bool state){
        _canvasGroup.alpha = state ? 1f : 0f;
    }

    public IEnumerator StartAnimation(SO_Animation animationSO, bool enableInteractable, bool enableVisibility, bool useUnascaleTime, Action DoLast = null){
        ConvertSO(animationSO);

        //Before animation
        SetInteractable(false);
        SetVisibility(false);
        SetComponents();

        //Animation
        _animationTween = GetTween();
        yield return _animationTween.SetUpdate(useUnascaleTime).WaitForCompletion();

        //After Animation
        SetInteractable(enableInteractable);
        SetVisibility(enableVisibility);
        
        DoLast?.Invoke();
    }

    public void SkipAnimation(SO_Animation animationSO, bool enableInteractable, bool enableVisibility, Action DoLast = null){
        ConvertSO(animationSO);

        //Before animation
        SetInteractable(false);
        SetVisibility(false);
        SetComponents();

        //Animation
        _animationTween = GetTween();
        _animationTween.Complete();

        //After Animation
        SetInteractable(enableInteractable);
        SetVisibility(enableVisibility);
        
        DoLast?.Invoke();
    }

    public void ConvertSO(SO_Animation soAnimation){
        _duration = soAnimation._animationDuration;
        _ease = soAnimation._ease;
        _animationStruct = soAnimation.animation;
    }

    public void CompleteAnimation(){
        if(_animationTween.IsActive())
            _animationTween.Complete();
    }
}