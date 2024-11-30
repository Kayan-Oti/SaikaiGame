using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System.Linq;
using System;

public class UI_ManagerAnimation : MonoBehaviour
{
    [Serializable]
    public struct Animation{
        [Tooltip("Optional Name")]
        public string name;
        [Space(2)]
        [Tooltip("Enable Interactable in the End of The animation")]
        public bool enableInteractable;
        [Space(2)]
        [Tooltip("Enable Visibility in the End of The animation")]
        public bool enableVisibility;
        [Space(2)]
        [Tooltip("Wait animation to End Before Start Next")]
        public bool waitAnimationEnd;
        [Space(2)]
        [Tooltip("If Delay After animations")]
        public bool hasDelay;
        [Tooltip("Delay in Seconds After animation")]
        [ConditionalField(nameof(hasDelay))] public float delaySeconds;
        [Space(5)]
        public UI_Animator target;
        [Space(2)]
        public SO_Animation SOAnimation;
    }

    [Serializable]
    public struct AnimationsList{
        [Tooltip("Name used to Filter")]
        public string name;
        [Space(5)]
        public bool useUnascaleTime;

        [Space(10)]
        public List<Animation> animations;
    }

    enum AnimationState{
        notExist = 0,
        isActive,
        isDesactive
    }

    [SerializeField] private List<AnimationsList> _listAnimations = new List<AnimationsList>();
    /// <summary>
    /// Dictionay of Currents Animation
    /// </summary>
    private Dictionary<string, bool> _animationActivity = new Dictionary<string, bool>();
    private int _countCoroutines;

    /// <summary>
    /// Used to Count Coroutines that don't need to WaitToEnd
    /// </summary>
    private IEnumerator CountCoroutine(IEnumerator animation){
        _countCoroutines++;
        yield return animation;
        _countCoroutines--;
    }
    
    public void PlayAnimation(string nameFilter, Action DoLast = null){
        StartCoroutine(PlayAnimationCoroutine(nameFilter, DoLast));
    }

    public IEnumerator PlayAnimationCoroutine(string nameFilter, Action DoLast = null)
    {
        //Creates or Active animation
        if(!ActiveAnimation(nameFilter))
            yield break;

        //Values
        _countCoroutines = 0;
        AnimationsList animationsList = GetAnimationListByName(nameFilter);

        //Loop
        foreach(Animation animation in animationsList.animations)
        {
            //Exception: Skip further animation
            if(!_animationActivity[nameFilter]){
                animation.target.SkipAnimation(animation.SOAnimation, animation.enableInteractable, animation.enableVisibility);
                continue;
            }
            //Play Animation, WaitingEnd or Not
            if(animation.waitAnimationEnd)
                yield return animation.target.StartAnimation(animation.SOAnimation, animation.enableInteractable, animation.enableVisibility, animationsList.useUnascaleTime);
            else
                 StartCoroutine(CountCoroutine(animation.target.StartAnimation(animation.SOAnimation, animation.enableInteractable, animation.enableVisibility, animationsList.useUnascaleTime)));
            
            //Delay before next
            if(animation.hasDelay)
                yield return DelayAnimation(animation, nameFilter, animationsList.useUnascaleTime);
        }
        
        //Wait all animations to End
        while (_countCoroutines > 0)
            yield return null;

        //Do After Animations End
        _animationActivity[nameFilter] = false; // Desactive Animation
        DoLast?.Invoke();
    }

    /// <summary>
    /// Alternative version of a WaitForSeconds, that can be break
    /// </summary>
    private IEnumerator DelayAnimation(Animation animation, string nameFilter, bool isUnscaleTime){
        //Alternative WaitForSeconds
        for(float timer = animation.delaySeconds; timer >= 0; timer -= isUnscaleTime ? Time.unscaledTime:Time.deltaTime){
            //Skip Delay if !Activity
            if(!_animationActivity[nameFilter])
                yield break;
                    
            yield return null;
        }
    }

    private AnimationsList GetAnimationListByName(string nameFilter){
        return _listAnimations.First(x => x.name == nameFilter);
    }

    /// <summary>
    /// Actives or Add a new Animation to de Dictonary
    /// </summary>
    /// <returns>Return false if is Already Active</returns>
    private bool ActiveAnimation(string nameFilter){
        bool works;
        switch(CheckAnimationState(nameFilter)){
            case AnimationState.notExist:
                _animationActivity.Add(nameFilter, true);
                works = true;
                break;
            case AnimationState.isActive:
                Debug.LogError($"Animation {nameFilter} is already Active");
                works = false;
                break;
            case AnimationState.isDesactive:
                _animationActivity[nameFilter] = true;
                works = true;
                break;
            default:
                Debug.LogError("Unexpected AnimationState");
                works = false;
                break;
        }
        return works;
    }

    /// <summary>
    /// Checks if Animation does not Exist, is Active or Desactive
    /// </summary>
    private AnimationState CheckAnimationState(string nameFilter){
        //If Doesn't Exist
        if(!_animationActivity.ContainsKey(nameFilter))
            return AnimationState.notExist;
        //If is Active
        if(_animationActivity[nameFilter])
            return AnimationState.isActive;
        //If is Desactive
        return AnimationState.isDesactive;
    }

    public void SkipAnimation(string nameFilter){
        //Check if Animation is Active
        if(CheckAnimationState(nameFilter) != AnimationState.isActive)
            return;
        
        _animationActivity[nameFilter] = false;
        AnimationsList animationsList = GetAnimationListByName(nameFilter);
        foreach(Animation animation in animationsList.animations){
            animation.target.CompleteAnimation();
        }
    }
}