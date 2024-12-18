using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using TMPro;
using MyBox;
using PrimeTween;

public class RevisorGameManager : Singleton<RevisorGameManager>
{
    [Header("Debug ReadOnly")]
    [SerializeField] [ReadOnly] private List<FallingWord_Revisor> _activeWordObjects = new List<FallingWord_Revisor>(); // Palavras na tela
    [SerializeField] [ReadOnly] public float SpawnInterval = 3.5f; // Intervalo entre os spawns (usado no Spawner)

    //Statistics
    [SerializeField] [ReadOnly] public int Score = 0;
    [SerializeField] [ReadOnly] public float MaxCombo = 1f;
    [SerializeField] [ReadOnly] public int Hits = 0;
    [SerializeField] [ReadOnly] public int Misses = 0;


    [Header("GameObjects")]
    [SerializeField] private Spawner_Revisor _spawner;
    [SerializeField] private CountDownTimer _timer;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _comboText;

    [Header("Timer")]
    [SerializeField] private float _startTime;

    [Header("Combo + Score")]
    [Tooltip("Score ganho por Acerto")]
    [SerializeField] private int SCORE_GAIN = 100;
    [Tooltip("Score perdido por Erro")]
    [SerializeField] private int SCORE_LOST = 25;
    [Tooltip("Combo perdido por Erro")]
    [SerializeField] private int COMBO_PENALTY = 3; // Penalidade no combo ao errar

    [Header("Speed")]
    [Tooltip("Speed Inicial")]
    [SerializeField] private float SPEED_MULTIPLIER_DEFAULT = 0.25f;
    [Tooltip("Incremento de Speed a cada 10s")]
    [SerializeField] private float SPEED_MULTIPLIER_INCREMENT = 0.010f; // Aumento a velocidade

    [Header("Spawn")]
    [Tooltip("Intervalo de Spawn Inicial")]
    [SerializeField] private float SPAWN_INTERVAL_DEFAULT= 3.5f; // Valor máximo do intervalo de spawn
    [Tooltip("Intervalo Mínimo de Spawn")]
    [SerializeField] private float SPAWN_INTERVAL_MIN = 2.0f; // Valor mínimo do intervalo de spawn
    [Tooltip("Intervalo Máximo de Spawn")]
    [SerializeField] private float SPAWN_INTERVAL_MAX = 6.0f; // Valor máximo do intervalo de spawn
    [Tooltip("Incremento de Intervalo, quando palavra é spawnada")]
    [SerializeField] private float SPAWN_INTERVAL_INCREASE = 0.25f; // Valor usado ao spawnar uma palavra
    [Tooltip("Exponencial que controla mudanças no Intervalo de Spawn")]
    [SerializeField] private AnimationCurve SPAWN_EXPONENCIAL_CURVE; // Valor usado ao spawnar uma palavra

    [Header("Reaction")]
    [Tooltip("Minimo de tempo para atingir o Máximo de redução")]
    [SerializeField] private float REACTION_TIME_MIN = 3f; // Ponto máximo de redução
    [Tooltip("Máximo de tempo para atingir o Mínimo de redução")]
    [SerializeField] private float REACTION_TIME_MAX = 10.0f; // Ponto máximo de redução
    [Tooltip("Redução Máxima de Intervalo de Spawn")]
    [SerializeField] private float REACTION_TIME_ADJUST_HIT = 1f;

    [Serializable]
    public struct CameraShakeSettings{
        public float strengthFactor;
        public float duration;
        public float frequency;
    }

    [Header("Shake Settings")]
    [SerializeField] private CameraShakeSettings _shakeOnMiss;
    [SerializeField] private CameraShakeSettings _shakeOnHit;
    [SerializeField] private CameraShakeSettings _shakeOnLostWord;
    
    //Combo
    private int _comboCount = 0; // Contador de combos
    private float _comboMultiplier = 1f; // Multiplicador de pontos baseado no combo

    //Dificulty
    [HideInInspector] public float SpeedMultiplier {get; private set;} = 0.25f; // Multiplicador de velocidade das palavras

    //Conditions
    private float _lastHitTime = 0f; // Momento do último acerto
    private bool _hasActiveWords;

    private void OnEnable()
    {
        EventManager.LevelManager.OnLevelStart.Get().AddListener(StartLevel);
        EventManager.LevelManager.OnCountDownEnd.Get().AddListener(EndLevel);
        EventManager.LevelManager.OnCountDownCycle.Get().AddListener(IncreaseSpeedMultiplier);
    }

    private void OnDisable()
    {
        EventManager.LevelManager.OnLevelStart.Get().RemoveListener(StartLevel);
        EventManager.LevelManager.OnCountDownEnd.Get().RemoveListener(EndLevel);
        EventManager.LevelManager.OnCountDownCycle.Get().RemoveListener(IncreaseSpeedMultiplier);
    }
    
    private void Start(){
        ResetLevel();
    }

    #region Set Up
    public void StartLevel(){
        EffectOnStart();

        ResetLevel();
        _spawner.StartSpawn();
        _timer.StartTimer(_startTime);
    }

    public void ResetLevel(){
        //---Values
        SpawnInterval = SPAWN_INTERVAL_DEFAULT;
        _spawner.SetSpawnInterval(SpawnInterval);

        //Pego diretamente pelo SpawnerWords
        SpeedMultiplier = SPEED_MULTIPLIER_DEFAULT;

        _comboMultiplier = 1f;
        _comboCount = 0;

        _lastHitTime = 0f;

        Score = 0;
        MaxCombo = 1f;
        Hits = 0;
        Misses = 0;

        //---Visual
        UpdateVisualScorePoint();
        UpdateVisualCombo();
    }

    public void EndLevel(){
        EffectOnEnd();

        _spawner.EndSpawn();
        ClearActiveWords();
        EventManager.LevelManager.OnLevelEnd.Get().Invoke();
    }

    [ButtonMethod]
    public void EndTimer(){
        _timer.EndTimer();
    }

    #endregion

    #region Combo & Score

    private void AddScore(){
        Score += Mathf.RoundToInt(SCORE_GAIN * _comboMultiplier);
        UpdateVisualScorePoint();
    }

    private void SubtractScore(){
        Score -= SCORE_LOST;
        UpdateVisualScorePoint();
    }

    private void AddCombo(){
        _comboCount++;
        _comboMultiplier = 1f + (_comboCount * 0.1f);

        if(_comboMultiplier > MaxCombo)
            MaxCombo = _comboMultiplier;

        UpdateVisualCombo();
    }

    private void SubtractCombo(){
        _comboCount = Mathf.Max(0, _comboCount - COMBO_PENALTY);
        _comboMultiplier = 1f + (_comboCount * 0.1f);
        UpdateVisualCombo();
    }

    #endregion

    #region SpawnInterval

    private void DecreaseSpawnInterval(float reactionTime)
    {
        // --- --- --- Reaction Time --- --- ---        
        float reactionFactor = 0;
        float intervalAdjustment = 0;

        //Não terá redução de tempo
        if(reactionTime >= REACTION_TIME_MAX)
            return;

        // Calcula o desempenho em relação ao Reaction Time
        // Retorna em porcentagem (0 a 1)
        reactionFactor = Mathf.InverseLerp(REACTION_TIME_MIN, REACTION_TIME_MAX, reactionTime);
        // Calcula o quanto será extraído do Reaction Time Adjust
        intervalAdjustment = Mathf.Lerp(-REACTION_TIME_ADJUST_HIT, 0, reactionFactor);

        // --- --- --- Exponencial --- --- ---
        float adjustedInterval = CalcExponencial(intervalAdjustment);

        // --- --- --- Spawn Interval --- --- ---

        // Atualiza o intervalo acumulativamente
        SpawnInterval = Mathf.Clamp(SpawnInterval + adjustedInterval, SPAWN_INTERVAL_MIN, SPAWN_INTERVAL_MAX);

        // Atualiza o spawn interval no Spawner
        // _spawner.SetSpawnInterval(SpawnInterval);

        // Debug.Log($"Novo Spawn Interval: {SpawnInterval} (Reação: {reactionTime}, Ajuste: {intervalAdjustment}, Expo: {adjustedInterval})");
    }

    private void IncreaseSpawnInterval(){
        // --- --- --- Exponencial --- --- ---
        float increaseTime = CalcExponencial(SPAWN_INTERVAL_INCREASE, true);

        // --- --- --- Spawn Interval --- --- ---
        SpawnInterval = Mathf.Min(SpawnInterval + increaseTime, SPAWN_INTERVAL_MAX);
        
        // Atualiza o spawn interval no Spawner
        // _spawner.SetSpawnInterval(SpawnInterval);

        // Debug.Log($"Increase Interval: {SpawnInterval}, Increase: {increaseTime})");
    }

    private float CalcRactionTime(){
        // Calcula o tempo de reação desde o último acerto
        float currentTime = Time.time; // Tempo atual
        float reactionTime = currentTime - _lastHitTime;
        _lastHitTime = currentTime; // Atualiza o tempo do último acerto

        return reactionTime;
    }

    private float CalcExponencial(float intervalAdjustment, bool inversedExpo = false){

        // Normaliza o spawnInterval entre 0 e 1 (onde 0 é o valor mínimo e 1 é o valor máximo)
        float normalizedInterval = Mathf.InverseLerp(SPAWN_INTERVAL_MIN, SPAWN_INTERVAL_MAX, SpawnInterval);

        //Inverte a curva
        if(inversedExpo)
            normalizedInterval = 1 - normalizedInterval;

        // Avalia o valor da curva no ponto normalizado
        float curveFactor = SPAWN_EXPONENCIAL_CURVE.Evaluate(normalizedInterval);

        // Multiplica o intervalAdjustment pelo fator exponencial
        return intervalAdjustment * curveFactor;
    }

    #endregion

    #region Event
    private void IncreaseSpeedMultiplier()
    {
        SpeedMultiplier += SPEED_MULTIPLIER_INCREMENT;
    }

    public void OnHit(FallingWord_Revisor wordObject){
        RemoveWord(wordObject);

        //Statistic
        Hits++;

        //Score
        AddScore();

        //Combo
        AddCombo();

        // Ajusta Intervalo
        DecreaseSpawnInterval(CalcRactionTime());

        //Effect
        EffectOnHit();
    }

    public void OnMiss(FallingWord_Revisor wordObject){
        RemoveWord(wordObject);

        //Statistic
        Misses++;
        
        //Score
        SubtractScore();

        //Combo
        SubtractCombo();

        //Effect
        EffectOnMiss();
    }

    public void OnLostWord(FallingWord_Revisor wordObject){
        RemoveWord(wordObject);

        //Effect
        EffectOnLostWord();
    }

    public void AddWord(FallingWord_Revisor wordObject)
    {
        _activeWordObjects.Add(wordObject);

        //Invertal
        IncreaseSpawnInterval();

        if(!_hasActiveWords)
            _lastHitTime = Time.time;
    }

    public void RemoveWord(FallingWord_Revisor wordObject)
    {
        _activeWordObjects.Remove(wordObject);
    }

    private void ClearActiveWords(){
        foreach(FallingWord_Revisor wordObject in _activeWordObjects.ToList()){
            RemoveWord(wordObject);
            wordObject.EffectOnEnd();
        }
    }

    #endregion

    #region AudioVisual Effects

    private void EffectOnStart(){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LevelStart);
    }

    private void EffectOnEnd(){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LevelEnd);
    }

    private void EffectOnHit(){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.WordHit);
        ShakeCamera(_shakeOnHit);
    }

    private void EffectOnMiss(){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.TypeMiss);
        ShakeCamera(_shakeOnMiss);
    }

    private void EffectOnLostWord(){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.WordMiss);
        ShakeCamera(_shakeOnLostWord);
    }

    private void UpdateVisualScorePoint(){
        _scoreText.text = Score.ToString();
    }

    private void UpdateVisualCombo()
    {
        _comboText.text = "x" + _comboMultiplier.ToString("F1");
    }

    private void ShakeCamera(CameraShakeSettings settings){
        Tween.ShakeCamera(Camera.main, strengthFactor: settings.strengthFactor, duration: settings.duration, frequency: settings.frequency);
    }

    #endregion
}