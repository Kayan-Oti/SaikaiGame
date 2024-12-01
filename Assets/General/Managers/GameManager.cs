using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private LoadingScreen _loadingScreen;
    [SerializeField] private bool _loadSceneOnStart = true;
    private List<AsyncOperation> _scenesLoading = new List<AsyncOperation>();
    private int _currentSceneIndex;
    private const float MIN_WAITSECONDS_LOADSCREEN = 0.75f;

    #region Initial Setup
    private void Start() {
        if(_loadSceneOnStart)
            StartCoroutine(FirstScene(SceneIndex.Menu));
    }

    //Method similar to LoadScene, but without animation
    public IEnumerator FirstScene(SceneIndex scene){
        //Scene to load
        _scenesLoading.Add(SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Additive));
        _currentSceneIndex = (int)scene;

        //Wait loading
        yield return WaitLoading();

        //On Scene Loaded
        InvokeOnLoadedScene();
    }

    #endregion

    public void LoadScene(SceneIndex scene){
        AudioManager.Instance.StopMusic();
        StartCoroutine(GetSceneLoadProgress(scene));
    }

    private IEnumerator GetSceneLoadProgress(SceneIndex scene){
        //Ativa a animação de Loading
        yield return _loadingScreen.OnStartLoadScene();

        //Scene to load
        _scenesLoading.Add(SceneManager.UnloadSceneAsync(_currentSceneIndex));
        _currentSceneIndex = (int)scene;
        _scenesLoading.Add(SceneManager.LoadSceneAsync(_currentSceneIndex, LoadSceneMode.Additive));

        //Tempo minimo de espera
        yield return new WaitForSecondsRealtime(MIN_WAITSECONDS_LOADSCREEN);

        //Wait loading
        yield return WaitLoading();

        //On Scene Loaded
        OnSceneLoaded();
    }

    private IEnumerator WaitLoading(){
        for(int i = 0; i<_scenesLoading.Count; i++){
            while(!_scenesLoading[i].isDone)
                yield return null;
        }
    }

    private void OnSceneLoaded(){
        Time.timeScale = 1.0f;
        //Animação ao terminar de Carrega
        _loadingScreen.OnEndLoadScene(InvokeOnLoadedScene);
    }

    private void InvokeOnLoadedScene(){
        EventManager.GameManager.OnLoadedScene.Get().Invoke();
    }
}
