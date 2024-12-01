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
        AudioManager.Instance.InitializeMusic(FMODEvents.Instance.MenuMusic, MusicIntensity.Intensity3);
    }

    #endregion

    #region Onclick

    public void OnClick_Play(){
        GameManager.Instance.LoadScene(_playScene);
    }

    //--Menu Settings
    public void OnClick_Settings(){
        _mainContainer.SetActive(false);
        _settingsContainer.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_settingsFirstButton);
    }

    public void OnClick_SettingsClose(){
        _mainContainer.SetActive(true);
        _settingsContainer.SetActive(false);
        EventSystem.current.SetSelectedGameObject(_settingsButton);
    }

    //--Menu Credits
    public void OnClick_Credits(){
        _mainContainer.SetActive(false);
        _creditsContainer.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_creditsFirstButton);
    }

    public void OnClick_CreditsClose(){
        _mainContainer.SetActive(true);
        _creditsContainer.SetActive(false);
        EventSystem.current.SetSelectedGameObject(_creditsButton);
    }

    //--Menu Exit
    public void OnClick_Exit(){
        Debug.Log("OnClick_Exit");
        Application.Quit();
    }

    #endregion
}
