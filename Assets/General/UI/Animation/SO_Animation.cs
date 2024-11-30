using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public enum AnimationStyle{
    Appearing,
    Leaving
}

[System.Serializable]
public struct AnimationStruct
{
    [Separator("Options")]
    [SerializeField] public bool DoTranslation;
    [ConditionalField(nameof(DoTranslation))][SerializeField] public AnimationStyle StylePosition;
    [ConditionalField(nameof(DoTranslation))][SerializeField] public Vector2 Distance;
    [Space(10)]

    [SerializeField] public bool DoRotation;
    [ConditionalField(nameof(DoRotation))][SerializeField] public AnimationStyle StyleRotation;
    [ConditionalField(nameof(DoRotation))][SerializeField] public float RotationZ;
    [Space(10)]

    [SerializeField] public bool DoScale;
    [ConditionalField(nameof(DoScale))][SerializeField] public AnimationStyle StyleScale;
    [Space(10)]
    
    [SerializeField] public bool DoFade;
    [ConditionalField(nameof(DoFade))][SerializeField] public AnimationStyle StyleFade;
}

 [CreateAssetMenu(menuName = "DATA/SO Animation", fileName = "Animation_")]
public class SO_Animation : ScriptableObject
{
    public float _animationDuration = 1;
    public AnimationCurve _ease;
    public AnimationStruct animation;
}
