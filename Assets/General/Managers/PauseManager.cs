using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private UI_ManagerAnimation _pauseUI;
    public static bool IsPaused { get; private set;} = false;

    private void Update() {
        //Pause
        if(InputManager.playerInputActions.Commom.Pause.WasPerformedThisFrame())
            ChangePauseState();
    }

    private void PauseGame(){
        IsPaused = true;
        Time.timeScale = 0;
        InputManager.playerInputActions.Player.Disable();

        _pauseUI.PlayAnimation("Start", AfterPauseAnimation);
    }

    private void AfterPauseAnimation(){
        InputManager.playerInputActions.UI.Enable();
    }
    
    private void UnpauseGame(){
        InputManager.playerInputActions.UI.Disable();

        _pauseUI.PlayAnimation("End", AfterUnpauseAnimation);
    }
    
    private void AfterUnpauseAnimation(){
        IsPaused = false;
        Time.timeScale = 1;
        InputManager.playerInputActions.Player.Enable();
    }

    public void ChangePauseState(){
        if(!IsPaused)
            PauseGame();
        else
            UnpauseGame();
    }

    //Button Action
    public void BackToMenu(){
        AudioManager.Instance.StopAmbience();
        AudioManager.Instance.StopMusic();
        GameManager.Instance.LoadScene(SceneIndex.Menu);
        ChangePauseState();
    }
}