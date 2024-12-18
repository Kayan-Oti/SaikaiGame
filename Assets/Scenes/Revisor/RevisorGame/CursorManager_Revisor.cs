using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager_Revisor : MonoBehaviour
{
    [SerializeField] private CursorTrigger _correctCursorTrigger;
    [SerializeField] private CursorTrigger _incorrectCursorTrigger;

    private void Update(){
        //Correct Cursor
        if(Input.GetMouseButtonDown(0)){
            //Disable Other
            if(_incorrectCursorTrigger.IsActive) 
                StopReviewing(_incorrectCursorTrigger);
            //Enable this
            StartReviewing(_correctCursorTrigger);
        }
        else if(Input.GetMouseButtonUp(0)){
            StopReviewing(_correctCursorTrigger);
        }

        //Incorrect Cursor
        if(Input.GetMouseButtonDown(1)){
            if(_correctCursorTrigger.IsActive) 
                StopReviewing(_correctCursorTrigger);
            StartReviewing(_incorrectCursorTrigger);
        }
        else if(Input.GetMouseButtonUp(1)){
            StopReviewing(_incorrectCursorTrigger);
        }

        //Continue Active
        if(_correctCursorTrigger.IsActive || _incorrectCursorTrigger.IsActive){
            ContinueReviewing();
        }
    }

    private void StartReviewing(CursorTrigger cursorTrigger){
        ContinueReviewing();
        cursorTrigger.EnableTrigger();
    }

    private void StopReviewing(CursorTrigger cursorTrigger){
        cursorTrigger.DisableTrigger();
    }

    private void ContinueReviewing(){
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newPosition.z = 0f;
        transform.position = newPosition;
    }
}
