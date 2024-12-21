using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
using TMPro;

public class TutorialManager_Revisor : MonoBehaviour
{
    [SerializeField] private List<GameObject> _listTutorialText;
    [SerializeField] private GameObject _canvas;
    [SerializeField] private TutorialRevisor_FallingWord _fakeFallingWordCorrect;
    [SerializeField] private TutorialRevisor_FallingWord _fakeFallingWordIncorrect;
    [SerializeField] private GameObject _buttonNext;
    [SerializeField] private int _tutorialPositionInList = 3;

    private int _currentTutorial = 0;
    private int _hitWords = 0;

    private void OnEnable() {
        EventManager.LevelManager.OnTutorialStart.Get().AddListener(StartTutorial);
        _fakeFallingWordCorrect.OnDestroyTrigger += EndRevisorTutorial;
        _fakeFallingWordIncorrect.OnDestroyTrigger += EndRevisorTutorial;
    }

    private void OnDisable() {
        EventManager.LevelManager.OnTutorialStart.Get().RemoveListener(StartTutorial);
        _fakeFallingWordCorrect.OnDestroyTrigger -= EndRevisorTutorial;
        _fakeFallingWordIncorrect.OnDestroyTrigger -= EndRevisorTutorial;
    }

    private void Start(){
        _listTutorialText.ForEach((x) => x.SetActive(false));

        _fakeFallingWordCorrect.gameObject.SetActive(false);
        _fakeFallingWordIncorrect.gameObject.SetActive(false);
        _canvas.SetActive(false);
    }

    #region Tutorial Manager

    private void StartTutorial(){
        _canvas.SetActive(true);
        _listTutorialText[_currentTutorial].SetActive(true);
    }

    public void NextTutorial(){
        _listTutorialText[_currentTutorial].SetActive(false);

        _currentTutorial++;

        if(_currentTutorial == _tutorialPositionInList){
            SpawnFakeWord();
        }

        if(_currentTutorial < _listTutorialText.Count)
            _listTutorialText[_currentTutorial].SetActive(true);
        else
            EndTutorial();
    }

    private void SpawnFakeWord(){
        _fakeFallingWordCorrect.gameObject.SetActive(true);
        _fakeFallingWordIncorrect.gameObject.SetActive(true);

        _fakeFallingWordCorrect.StartFalling();
        _fakeFallingWordIncorrect.StartFalling();

        _buttonNext.SetActive(false);
    }

    public void SkipTutorial(){
        EndTutorial();
    }

    private void EndTutorial(){
        EventManager.LevelManager.OnTutorialEnd.Get().Invoke();
        gameObject.SetActive(false);
    }

    #endregion 

    #region Tutorial

    private void EndRevisorTutorial(){

        _hitWords++;
        if(_hitWords != 2)
            return;

        NextTutorial();
        _buttonNext.SetActive(true);
    }

    #endregion
}