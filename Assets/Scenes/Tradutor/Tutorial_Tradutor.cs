using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_Tradutor : MonoBehaviour
{
    [SerializeField] private List<GameObject> _listTutorialText;
    [SerializeField] private GameObject _canvas;
    private int _currentTutorial = 0;

    private void OnEnable() {
        EventManager.LevelManager.OnTutorialStart.Get().AddListener(StartTutorial);
    }

    private void OnDisable() {
        EventManager.LevelManager.OnTutorialStart.Get().RemoveListener(StartTutorial);
    }

    private void Start(){
        _listTutorialText.ForEach((x) => x.SetActive(false));
        
        _canvas.SetActive(false);
    }

    private void StartTutorial(){
        _canvas.SetActive(true);
        _listTutorialText[_currentTutorial].SetActive(true);
    }

    public void NextTutorial(){
        _listTutorialText[_currentTutorial].SetActive(false);

        _currentTutorial++;

        if(_currentTutorial < _listTutorialText.Count)
            _listTutorialText[_currentTutorial].SetActive(true);
        else
            EndTutorial();
    }

    public void SkipTutorial(){
        EndTutorial();
    }

    private void EndTutorial(){
        _canvas.SetActive(false);

        EventManager.LevelManager.OnTutorialEnd.Get().Invoke();
    }
}
