using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using UnityEngine;

public class LevelManager_Tradutor : Singleton<LevelManager_Tradutor>
{
    [Header("GameObjects")]
    [SerializeField] private TypeGameManager _typeGameManager;
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private UI_ManagerAnimation _managerAnimation;

    [Header("Dialogue")]
    [SerializeField] private SO_Dialogue _dialogueStart;
    [SerializeField] private SO_Dialogue _dialogueEnd;

    [Header("Values")]
    [SerializeField] private SceneIndex _nextLevelIndex;
    private const float DELAY_TO_START = 0.25f;

    #region Unity Setup
    private void OnEnable() {
        EventManager.GameManager.OnLoadedScene.Get().AddListener(OnLoadedScene);
        EventManager.LevelManager.OnLevelEnd.Get().AddListener(OnEndLevel);
    }

    private void OnDisable(){
        EventManager.GameManager.OnLoadedScene.Get().RemoveListener(OnLoadedScene);
        EventManager.LevelManager.OnLevelEnd.Get().RemoveListener(OnEndLevel);

    }

    #endregion

    #region Dialogue Events
    private void OnLoadedScene() {
        //Play Song
        AudioManager.Instance.InitializeMusic(FMODEvents.Instance.MenuMusic);
        Invoke(nameof(StartDialogue), DELAY_TO_START);
    }

    [ButtonMethod]
    private void StartDialogue(){
        _dialogueManager.StartDialogue(_dialogueStart, OnEndDialogueStart);
    }

    [ButtonMethod]
    private void EndLevelDialogue(){
        _dialogueManager.StartDialogue(_dialogueEnd, OnEndDialogueEnd);
    }

    private void OnEndDialogueStart(){
        StartLevel();
    }

    private void OnEndDialogueEnd(){
        SetGameOverUI(true);
    }

    #endregion

    #region Match Events
    
    [ButtonMethod]
    private void StartLevel(){
        EventManager.LevelManager.OnLevelStart.Get().Invoke();
    }

    [ButtonMethod]
    private void OnEndLevel(){
        EndLevelDialogue();
    }

    #endregion

    #region UI Events

    private void SetGameOverUI(bool state){
        if(state)
            _managerAnimation.PlayAnimation("GameOver_Start");
        else
            _managerAnimation.PlayAnimation("GameOver_End", StartLevel);
    }

    public void PlayAgain(){
        SetGameOverUI(false);
    }

    private void LeavingLevel(){
        AudioManager.Instance.StopMusic();
    }

    public void BackToMenu(){
        LeavingLevel();
        GameManager.Instance.LoadScene(SceneIndex.Menu);
    }

    public void NextLevel(){
        LeavingLevel();
        SceneIndex sceneIndex = _nextLevelIndex;
        GameManager.Instance.LoadScene(sceneIndex);
    }

    #endregion
}
