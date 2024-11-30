using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class UI_ManagerNavegation : MonoBehaviour
{
    
    //Inputs
    private MainInputSystem _inputActions;
    private InputAction _navigateUI;

    //Variables
    private GameObject _currentSelected;

    #region Setup
    private void Awake(){
        _inputActions = InputManager.playerInputActions;

        _navigateUI = _inputActions.UI.Navigate;
    }

    private void OnEnable() {
        _inputActions.UI.Enable();

        EventManager.GameManager.OnChangeCurrentSelectedUI.Get().AddListener(ChangeCurrentSelectedUI);
    }

    private void OnDisable() {
        _inputActions.UI.Disable();

        EventManager.GameManager.OnChangeCurrentSelectedUI.Get().RemoveListener(ChangeCurrentSelectedUI);
    }
    #endregion

    private void Update(){
        if(_navigateUI.WasPerformedThisFrame())
            CheckSelectedUI();
    }

    #region Do

    private void ChangeCurrentSelectedUI(GameObject current){
        _currentSelected = current;
    }

    private void CheckSelectedUI(){
        if(EventSystem.current.currentSelectedGameObject == null)
            EventSystem.current.SetSelectedGameObject(_currentSelected);
    }

    #endregion
}
