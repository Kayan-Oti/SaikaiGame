using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using TMPro;
using UnityEngine;

public class LevelManager_Tradutor : Singleton<LevelManager_Tradutor>
{
    [Header("GameObjects")]
    [SerializeField] private TypeGameManager _typeGameManager;
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private UI_ManagerAnimation _managerAnimation;

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI _textScore;
    [SerializeField] private TextMeshProUGUI _textMaxCombo;
    [SerializeField] private TextMeshProUGUI _textHits;
    [SerializeField] private TextMeshProUGUI _textTypeMiss;

    [Header("Dialogue")]
    [SerializeField] private SO_Dialogue _dialogueStart;
    [SerializeField] private SO_Dialogue _dialogueEnd;

    [Header("Values")]
    [SerializeField] private SceneIndex _nextLevelIndex;
    [SerializeField] private bool _isStoryLevel = false;
    private const float DELAY_TO_START = 1.5f;
    private const float DELAY_TO_END = 1.5f;

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
        AudioManager.Instance.InitializeMusic(FMODEvents.Instance.MusicTradutor);
        
        if(_isStoryLevel)
            Invoke(nameof(StartDialogue), DELAY_TO_START);
        else
            Invoke(nameof(StartLevel), DELAY_TO_START);
    }

    [ButtonMethod]
    private void StartDialogue(){
        _dialogueManager.StartDialogue(_dialogueStart, StartLevel);
    }

    [ButtonMethod]
    private void EndLevelDialogue(){
        _dialogueManager.StartDialogue(_dialogueEnd, () => SetGameOverUI(true));
    }

    #endregion

    #region Match Events
    
    [ButtonMethod]
    private void StartLevel(){
        EventManager.LevelManager.OnLevelStart.Get().Invoke();
    }

    [ButtonMethod]
    private void OnEndLevel(){
        if(_isStoryLevel)
            Invoke(nameof(EndLevelDialogue), DELAY_TO_END);
        else
            Invoke(nameof(WaitToCallGameOver), DELAY_TO_END);
    }

    private void WaitToCallGameOver(){
        SetGameOverUI(true);
    }

    #endregion

    #region UI Events

    private void SetGameOverUI(bool state){
        if(state){
            _managerAnimation.PlayAnimation("GameOver_Start");
            _textScore.text = _typeGameManager.Score.ToString();
            _textMaxCombo.text = _typeGameManager.MaxCombo.ToString("F1");
            _textHits.text = _typeGameManager.Hits.ToString();
            _textTypeMiss.text = _typeGameManager.TypeMiss.ToString();
        }
        else{
            _managerAnimation.PlayAnimation("GameOver_End", StartLevel);
        }
    }

    public void PlayAgain(){
        SetGameOverUI(false);
    }

    public void BackToMenu(){
        GameManager.Instance.LoadScene(SceneIndex.Menu);
    }

    public void NextLevel(){
        SceneIndex sceneIndex = _nextLevelIndex;
        GameManager.Instance.LoadScene(sceneIndex);
    }

    #endregion
}
