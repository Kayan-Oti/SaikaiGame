using DG.Tweening;
using UnityEngine;

public class UI_Animator : UI_Abstract_Animator
{
    //Translation
    private Vector2 _defaultPos;
    private Vector2 _targetPosition;

    //Rotation
    private float _defaultRotationZ;
    private float _targetRotationZ;

    //Scale  
    private Vector2 _targetScale;

    //Fade
    private float _targetFade;


    public override void SetValues()
    {
        //Default Values
        _defaultPos = _rectTransform.anchoredPosition;
        _defaultRotationZ = _rectTransform.localEulerAngles.z;
    }

    public override void SetComponents(){
        //Start Position
        if(_animationStruct.DoTranslation){
            switch(_animationStruct.StylePosition){
                case AnimationStyle.Appearing:
                    _targetPosition = _defaultPos;
                    _rectTransform.localPosition = (Vector3)(_defaultPos + _animationStruct.Distance);
                break;
                case AnimationStyle.Leaving:
                    _targetPosition = _defaultPos + _animationStruct.Distance;
                    _rectTransform.localPosition = (Vector3)_defaultPos;
                break;
            }
        }else{
            _rectTransform.anchoredPosition = _defaultPos;
        }
        
        //Start Rotation
        if(_animationStruct.DoRotation){
            switch(_animationStruct.StyleRotation){
                case AnimationStyle.Appearing:
                    _targetRotationZ = _defaultRotationZ;
                    _rectTransform.rotation = Quaternion.Euler(0,0, _defaultRotationZ + _animationStruct.RotationZ);

                break;
                case AnimationStyle.Leaving:
                    _targetRotationZ = _defaultRotationZ + _animationStruct.RotationZ;
                    _rectTransform.rotation = Quaternion.Euler(0,0, _defaultRotationZ);
                break;
            }
        }else{
            _rectTransform.rotation = Quaternion.Euler(0,0, _defaultRotationZ);
        }

        //Start Fade
        if(_animationStruct.DoFade){
            switch(_animationStruct.StyleFade){
                case AnimationStyle.Appearing:
                    _targetFade = 1f;
                    _canvasGroup.alpha = 0f;
                break;
                case AnimationStyle.Leaving:
                    _targetFade = 0f;
                    _canvasGroup.alpha = 1f;
                break;
            }
        }else{
            _canvasGroup.alpha = 1f;
        }

        //Start Scale
        if(_animationStruct.DoScale){
            switch(_animationStruct.StyleScale){
                case AnimationStyle.Appearing:
                    _targetScale = Vector2.one;
                    _rectTransform.localScale = Vector2.zero;
                break;
                case AnimationStyle.Leaving:
                    _targetScale = Vector2.zero;
                    _rectTransform.localScale = Vector2.one;
                break;
            }
        }else{
            _rectTransform.localScale = Vector2.one;
        }
    }

    public override Tween GetTween()
    {
        Sequence animation = DOTween.Sequence();

        if(_animationStruct.DoTranslation)
            animation.Insert(0,_rectTransform.DOAnchorPos(_targetPosition, _duration).SetEase(_ease));

        if(_animationStruct.DoRotation)
            animation.Insert(0,_rectTransform.DORotate(new Vector3(0,0,_targetRotationZ), _duration).SetEase(_ease));

        if(_animationStruct.DoFade)
            animation.Insert(0,_canvasGroup.DOFade(_targetFade, _duration).SetEase(_ease));

        if(_animationStruct.DoScale)
            animation.Insert(0,_rectTransform.DOScale(_targetScale, _duration).SetEase(_ease));

        return animation;
    }
}