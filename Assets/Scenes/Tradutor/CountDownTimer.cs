using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;

public class CountDownTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timerText;
    private bool _isActive = false;
    private float _remainingTime;
    private float _timePassed;
    private const float INTERVAL_SPEED_INCREASE = 10f;

    private void Update() {
        if(!_isActive)
            return;
        
        if(_remainingTime > 0){
            _remainingTime -= Time.deltaTime;
            _timePassed += Time.deltaTime;

            // A cada 10 segundos, dispara o evento
            if (_timePassed >= INTERVAL_SPEED_INCREASE)
            {
                TriggerCycle();
                _timePassed = 0f; // Reseta o contador
            }
        }
        else{
            EndTimer();
        }

        UpdateVisualTimer();
    }

    public void StartTimer(float startTime){
        _remainingTime = startTime;
        _timePassed = 0f;
        _isActive = true;
    }

    public void EndTimer(){
        _remainingTime = 0;
        _isActive = false;
        TriggerEnd();
        UpdateVisualTimer();
    }

    private void UpdateVisualTimer(){
        int minutes = Mathf.FloorToInt(_remainingTime/60);
        int seconds = Mathf.FloorToInt(_remainingTime % 60);

        _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    #region EventTrigger

    private void TriggerEnd(){
        EventManager.LevelManager.OnCountDownEnd.Get().Invoke();
    }

    private void TriggerCycle(){
        EventManager.LevelManager.OnCountDownCycle.Get().Invoke();
    }

    #endregion

}
