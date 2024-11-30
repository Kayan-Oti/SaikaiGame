using MyBox;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public static MainInputSystem playerInputActions;

    private void Awake(){
        playerInputActions = new MainInputSystem();
        playerInputActions.Commom.Enable();
    }
}
