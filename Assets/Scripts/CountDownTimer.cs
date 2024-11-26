using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountDownTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private float _startTime;
    [SerializeField] private bool _isActive = true;
    private float _remainingTime;
    private float _timePassed;
    private const float INTERVAL_SPEED_INCREASE = 10f;

    // Evento para notificar aumento do multiplicador
    public static event Action OnSpeedMultiplierIncrease;

    private void Start(){
        _remainingTime = _startTime;
        _timePassed = 0f;
    }

    private void Update() {
        if(!_isActive)
            return;
        
        if(_remainingTime > 0){
            _remainingTime -= Time.deltaTime;

            _timePassed += Time.deltaTime;
            // A cada 10 segundos, dispara o evento
            if (_timePassed >= INTERVAL_SPEED_INCREASE)
            {
                OnSpeedMultiplierIncrease?.Invoke();
                _timePassed = 0f; // Reseta o contador
            }
        }
        else
            _remainingTime = 0;

        UpdateVisualTimer();
    }

    private void UpdateVisualTimer(){
        int minutes = Mathf.FloorToInt(_remainingTime/60);
        int seconds = Mathf.FloorToInt(_remainingTime % 60);

        _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
