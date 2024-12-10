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
    [SerializeField] private UI_ManagerAnimation _managerAnimation;
    [SerializeField] private LeaderboardManager _leaderboardManager;

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI _textScore;
    [SerializeField] private TextMeshProUGUI _textMaxCombo;
    [SerializeField] private TextMeshProUGUI _textHits;
    [SerializeField] private TextMeshProUGUI _textTypeMiss;

    [Header("Values")]
    [SerializeField] private SceneIndex _nextLevelIndex;

    private const float DELAY_TO_START = 1.5f;
    private const float DELAY_TO_END = 1.5f;

    #region Unity Setup
    private void OnEnable() {
        EventManager.GameManager.OnLoadedScene.Get().AddListener(OnLoadedScene);
        EventManager.LevelManager.OnLevelEnd.Get().AddListener(OnEndLevel);
        EventManager.LevelManager.OnTutorialEnd.Get().AddListener(OnEndTutorial);
    }

    private void OnDisable(){
        EventManager.GameManager.OnLoadedScene.Get().RemoveListener(OnLoadedScene);
        EventManager.LevelManager.OnLevelEnd.Get().RemoveListener(OnEndLevel);
        EventManager.LevelManager.OnTutorialEnd.Get().RemoveListener(OnEndTutorial);
    }

    #endregion

    #region Events
    private void OnLoadedScene() {
        //Play Song
        AudioManager.Instance.InitializeMusic(FMODEvents.Instance.MusicTradutor);

        if(GameManager.Instance.ActiveTradutorTutorial)
            Invoke(nameof(StartTutorial), DELAY_TO_START);
        else
            Invoke(nameof(StartLevel), DELAY_TO_START);
    }

    #endregion

    #region Tutorial

    [ButtonMethod]    
    private void StartTutorial(){
        EventManager.LevelManager.OnTutorialStart.Get().Invoke();
    }

    private void OnEndTutorial(){
        GameManager.Instance.ActiveTradutorTutorial = false;
        
        StartLevel();
    }

    #endregion

    #region Match Events
    
    [ButtonMethod]
    private void StartLevel(){
        EventManager.LevelManager.OnLevelStart.Get().Invoke();
    }

    private void OnEndLevel(){
        if(_typeGameManager.Score > _leaderboardManager.GetCurrentScore()){
            _leaderboardManager.SubmitNewEntry(_typeGameManager.Score, _typeGameManager.Hits.ToString());
        }
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
