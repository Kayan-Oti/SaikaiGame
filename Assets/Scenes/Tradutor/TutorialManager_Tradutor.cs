using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
using TMPro;

public class TutorialManager_Tradutor : MonoBehaviour
{
    [SerializeField] private List<GameObject> _listTutorialText;
    [SerializeField] private GameObject _canvas;
    [SerializeField] private Tutorial_FallingWord _fakeFallingWord;
    [SerializeField] private GameObject _button;
    [SerializeField] private TextMeshProUGUI _inputText;
    [SerializeField] private int _typeTutorialPositionInList = 3;
    [SerializeField] [ReadOnly] private string typedWord;

    private int _currentTutorial = 0;
    private bool _isTypeActive = false;
    private bool _isTypeWrong = false;

    private void OnEnable() {
        EventManager.LevelManager.OnTutorialStart.Get().AddListener(StartTutorial);
        _fakeFallingWord.OnDestroyTrigger += EndTypeTutorial;
    }

    private void OnDisable() {
        EventManager.LevelManager.OnTutorialStart.Get().RemoveListener(StartTutorial);
        _fakeFallingWord.OnDestroyTrigger -= EndTypeTutorial;
    }

    private void Start(){
        _listTutorialText.ForEach((x) => x.SetActive(false));
        _fakeFallingWord.gameObject.SetActive(false);
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

        if(_currentTutorial == _typeTutorialPositionInList){
            SpawnFakeWord();
        }

        if(_currentTutorial < _listTutorialText.Count)
            _listTutorialText[_currentTutorial].SetActive(true);
        else
            EndTutorial();
    }

    private void SpawnFakeWord(){
        _fakeFallingWord.gameObject.SetActive(true);
        _fakeFallingWord.StartFalling();

        _button.SetActive(false);
        _isTypeActive = true;
    }

    public void SkipTutorial(){
        EndTutorial();
    }

    private void EndTutorial(){
        EventManager.LevelManager.OnTutorialEnd.Get().Invoke();
        gameObject.SetActive(false);
    }

    #endregion 

    #region Tutorial Type

    private void EndTypeTutorial(){
        _isTypeActive = false;
        NextTutorial();
        _button.SetActive(true);
    }
    private void Update()
    {
        if(!_isTypeActive)
            return;

        if (Input.anyKeyDown)
            CheckKeyPressed();
    }

    private void CheckKeyPressed(){
        foreach (char keyPressed in Input.inputString)
        {
            //Exception: Enter
            if ((keyPressed== '\n') || (keyPressed == '\r')){
                Debug.Log("Enter: " + typedWord);
                continue;
            }

            //Backspace Pressed
            if (keyPressed == '\b'){
                //Exception: No char
                if(typedWord.Length == 0)
                    continue;

                typedWord = typedWord.Substring(0, typedWord.Length - 1);
            }
                
            //Default chars
            else{
                //Exception: first char space
                if(keyPressed == ' ' && typedWord.Length == 0)
                    continue;

                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Type);
                typedWord += keyPressed;
            }
            

            CompareTypeAndObjects();
            UpdateVisualInputText();
        }
    }

    private void CompareTypeAndObjects(){
        //Typed corret
        if(_fakeFallingWord.Word.StartsWith(typedWord, StringComparison.InvariantCultureIgnoreCase)){
            //Type está correto
            _isTypeWrong = false;
            
            //Set Color
            _fakeFallingWord.SetColor(typedWord);

            //Hit Word
            if(_fakeFallingWord.Word.Equals(typedWord, StringComparison.InvariantCultureIgnoreCase)){
                OnHit();
                return;
            }
        }
        //Typed Incorrect
        else{
            _fakeFallingWord.SetColor(typedWord);
            if(!_isTypeWrong){
                _isTypeWrong = true;
                OnTypeMiss();
            }
        }
    }

    private void OnHit(){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.WordHit);

        _fakeFallingWord.OnGetCorrectWord();

        ResetTypedWord();
    }

    private void OnTypeMiss(){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.TypeMiss);
    }

    private void ResetTypedWord()
    {
        typedWord = "";
        UpdateVisualInputText();
    }

    private void UpdateVisualInputText(){
        _inputText.text = typedWord;
    }

    #endregion
}
