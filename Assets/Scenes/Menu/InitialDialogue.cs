using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialDialogue : MonoBehaviour
{
    [SerializeField] private List<GameObject> _listDialogueText;
    [SerializeField] private GameObject _canvas;
    private int _currentTutorial = 0;

    private void OnEnable() {
        EventManager.LevelManager.OnTutorialStart.Get().AddListener(StartDialogue);
    }

    private void OnDisable() {
        EventManager.LevelManager.OnTutorialStart.Get().RemoveListener(StartDialogue);
    }

    private void Start(){
        _listDialogueText.ForEach((x) => x.SetActive(false));
        _canvas.SetActive(false);
    }

    #region Dialogue Manager

    private void StartDialogue(){
        _canvas.SetActive(true);
        _listDialogueText[_currentTutorial].SetActive(true);
    }

    public void NextDialogue(){
        _listDialogueText[_currentTutorial].SetActive(false);

        _currentTutorial++;

        if(_currentTutorial < _listDialogueText.Count)
            _listDialogueText[_currentTutorial].SetActive(true);
        else
            EndDialogue();
    }


    public void SkipDialogue(){
        EndDialogue();
    }

    private void EndDialogue(){
        EventManager.LevelManager.OnTutorialEnd.Get().Invoke();
        gameObject.SetActive(false);
    }

    #endregion
}
