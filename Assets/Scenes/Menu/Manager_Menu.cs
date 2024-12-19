using UnityEngine;
using UnityEngine.EventSystems;

public class Manager_Menu : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private GameObject _mainContainer;
    [SerializeField] private GameObject _playButton;
    [SerializeField] private GameObject _settingsButton;
    [SerializeField] private GameObject _creditsButton;

    [Header("Level Selector")]
    [SerializeField] private GameObject _levelContainer;
    [SerializeField] private GameObject _levelButtonFirst;
    [Header("Settings")]
    [SerializeField] private GameObject _settingsContainer;
    [SerializeField] private GameObject _settingsFirstButton;
    [Header("Credits")]
    [SerializeField] private GameObject _creditsContainer;
    [SerializeField] private GameObject _creditsFirstButton;
    
    [Header("Ranking")]
    [SerializeField] private LeaderboardManager _leaderboardManager;

    private void OnEnable() {
        EventManager.GameManager.OnLoadedScene.Get().AddListener(OnLoadScene);
    }

    private void OnDisable() {
        EventManager.GameManager.OnLoadedScene.Get().RemoveListener(OnLoadScene);
    }

    private void Start(){
        Invoke(nameof(WaitStart),0.1f);
    }
    private void WaitStart(){
        EventSystem.current.SetSelectedGameObject(_playButton);
        _levelContainer.SetActive(false);
        _settingsContainer.SetActive(false);
        _creditsContainer.SetActive(false);
    }

    #region Scene Management
    private void OnLoadScene() {
        //Play Song Menu
        AudioManager.Instance.InitializeMusic(FMODEvents.Instance.MusicMenu, MusicIntensity.Intensity3);
    }

    #endregion

    #region Onclick

    public void Onclick_Open(GameObject menuContainer, GameObject firstButton){
        _mainContainer.SetActive(false);
        menuContainer.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstButton);
    }

    public void Onclick_Close(GameObject menuContainer, GameObject menuButton){
        _mainContainer.SetActive(true);
        menuContainer.SetActive(false);
        EventSystem.current.SetSelectedGameObject(menuButton);
    }

    //--Menu Play
    public void OnClick_Play(){
        Onclick_Open(_levelContainer, _levelButtonFirst);
    }

    public void OnClick_PlayClose(){
        Onclick_Close(_levelContainer, _playButton);
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
