using MyBox;
using UnityEngine;
using UnityEngine.EventSystems;

public class Manager_Menu : MonoBehaviour
{
    [Header("Manager")]
    [SerializeField] private UI_ManagerAnimation _managerAnimation;

    [Header("Main")]
    [SerializeField] private GameObject _mainContainer;
    [SerializeField] private GameObject _playButton;
    [SerializeField] private GameObject _settingsButton;
    [SerializeField] private GameObject _creditsButton;

    [Header("Settings")]
    [SerializeField] private GameObject _settingsContainer;
    [SerializeField] private GameObject _settingsFirstButton;
    [Header("Credits")]
    [SerializeField] private GameObject _creditsContainer;
    [SerializeField] private GameObject _creditsFirstButton;
    
    [Header("Ranking")]
    [SerializeField] private LeaderboardManager _leaderboardManager;

    private const float DELAY_TO_START = 1.5f;

    private void OnEnable() {
        EventManager.GameManager.OnLoadedScene.Get().AddListener(OnLoadScene);
        EventManager.LevelManager.OnTutorialEnd.Get().AddListener(OnEndDialogue);
    }

    private void OnDisable() {
        EventManager.GameManager.OnLoadedScene.Get().RemoveListener(OnLoadScene);
        EventManager.LevelManager.OnTutorialEnd.Get().RemoveListener(OnEndDialogue);
    }

    private void Start(){
        _settingsContainer.SetActive(false);
        _creditsContainer.SetActive(false);
    }

    #region Scene Management
    private void OnLoadScene() {
        //Play Song Menu
        AudioManager.Instance.InitializeMusic(FMODEvents.Instance.MusicMenu, MusicIntensity.Intensity3);

        if(GameManager.Instance.ActiveInitialDialogue)
            Invoke(nameof(StartDialogue), DELAY_TO_START);
        else
            _managerAnimation.PlayAnimation("StartMain");
    }

    #endregion

    #region Dialogue

    [ButtonMethod]
    private void StartDialogue(){
        EventManager.LevelManager.OnTutorialStart.Get().Invoke();
    }

    private void OnEndDialogue(){
        GameManager.Instance.ActiveInitialDialogue = false;
        _managerAnimation.PlayAnimation("StartMain");
    }

    #endregion

    #region Onclick

    public void Onclick_Open(GameObject menuContainer, GameObject firstButton){
        _mainContainer.SetActive(false);
        menuContainer.SetActive(true);
        // EventSystem.current.SetSelectedGameObject(firstButton);
    }

    public void Onclick_Close(GameObject menuContainer, GameObject menuButton){
        _mainContainer.SetActive(true);
        menuContainer.SetActive(false);
        // EventSystem.current.SetSelectedGameObject(menuButton);
    }

    public void OnClick_LevelTradutor(){
        GameManager.Instance.LoadScene(SceneIndex.Tradutor);
    }

    public void OnClick_LevelRevisor(){
        GameManager.Instance.LoadScene(SceneIndex.Revisor);
    }

    //--Menu Settings
    public void OnClick_Settings(){
        Onclick_Open(_settingsContainer, _settingsFirstButton);
    }

    public void OnClick_SettingsClose(){
        Onclick_Close(_settingsContainer, _settingsButton);
    }

    //--Menu Ranking
    public void OnClick_Ranking(){
        _mainContainer.SetActive(false);
        _leaderboardManager.OpenLeaderBoard();
    }

    public void OnClick_RakingClose(){
        _mainContainer.SetActive(true);
        _leaderboardManager.CloseLeaderBoard();
    }

    public void Onclick_SwitchRanking(){
        _leaderboardManager.ChangeLeaderBoard(_leaderboardManager.LeaderBoardEnum == Enum_LeaderBoardReference.Tradutor ? Enum_LeaderBoardReference.Revisor : Enum_LeaderBoardReference.Tradutor);
    }

    //--Menu Credits
    public void OnClick_Credits(){
        Onclick_Open(_creditsContainer, _creditsFirstButton);
    }

    public void OnClick_CreditsClose(){
        Onclick_Close(_creditsContainer, _creditsButton);
    }

    #endregion
}
