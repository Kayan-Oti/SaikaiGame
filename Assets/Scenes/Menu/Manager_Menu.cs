using UnityEngine;
using UnityEngine.EventSystems;

public class Manager_Menu : MonoBehaviour
{
    [Header("Play")]
    [SerializeField] private GameObject _mainContainer;
    [SerializeField] private GameObject _playButton;
    [Header("Settings")]
    [SerializeField] private GameObject _settingsContainer;
    [SerializeField] private GameObject _settingsButton;
    [SerializeField] private GameObject _settingsFirstButton;
    [Header("Credits")]
    [SerializeField] private GameObject _creditsContainer;
    [SerializeField] private GameObject _creditsButton;
    [SerializeField] private GameObject _creditsFirstButton;

    [Header("Levels")]
    [SerializeField] private SceneIndex _playScene;


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
        GameManager.Instance.LoadScene(_playScene);
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
        // Onclick_Open(, );
    }

    public void OnClick_RakingClose(){
        // Onclick_Close(, );
    }

    //--Menu Credits
    public void OnClick_Credits(){
        Onclick_Open(_creditsContainer, _creditsFirstButton);
    }

    public void OnClick_CreditsClose(){
        Onclick_Close(_creditsContainer, _creditsButton);
    }

    //--Menu Exit
    public void OnClick_Exit(){
        Debug.Log("OnClick_Exit");
        Application.Quit();
    }

    #endregion
}
