using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using TMPro;
using MyBox;
using PrimeTween;

public class TypeGameManager : Singleton<TypeGameManager>
{
    [Header("Debug ReadOnly")]
    [SerializeField] [ReadOnly] private List<FallingWord> _activeWordObjects = new List<FallingWord>(); // Palavras na tela
    [SerializeField] [ReadOnly] private List<FallingWord> _possibleWords = new List<FallingWord>(); // Lista de possíveis
    [SerializeField] [ReadOnly] private string typedWord;
    //Statistics
    [SerializeField] [ReadOnly] public int Score = 0;
    [SerializeField] [ReadOnly] public float MaxCombo = 1f;
    [SerializeField] [ReadOnly] public int Hits = 0;
    [SerializeField] [ReadOnly] public int TypeMiss = 0;

    [Header("GameObjects")]
    [SerializeField] private SpawnerWords _spawner;
    [SerializeField] private CountDownTimer _timer;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _inputText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _comboText;

    [Header("Timer")]
    [SerializeField] private float _startTime;

    [Header("Combo + Score")]
    [Tooltip("Score ganho por letra")]
    [SerializeField] private int SCORE_GAIN = 25;
    [Tooltip("Score perdido por Type errado")]
    [SerializeField] private int SCORE_LOST = 100;
    [Tooltip("Combo perdido por Type errado")]
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
    [SerializeField] private CameraShakeSettings _shakeOnTypeMiss;
    [SerializeField] private CameraShakeSettings _shakeOnWordMiss;

    [Header("Zoom Settings")]
    [SerializeField] private TweenSettings<float> _zoomOnWordHit;

    //Combo
    private int _comboCount = 0; // Contador de combos
    private float _comboMultiplier = 1f; // Multiplicador de pontos baseado no combo
    //Dificulty
    private float _spawnInterval = 3.5f; // Intervalo entre os spawns (usado no Spawner)
    [HideInInspector] public float SpeedMultiplier {get; private set;} = 0.25f; // Multiplicador de velocidade das palavras
    //Conditions
    private float _lastHitTime = 0f; // Momento do último acerto
    private bool _hasActiveWords = false;
    private bool _isTypeWrong = false;
    private bool _isActive = false;
    private bool _hitOrMissThisFrame = false;

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

    private void Update()
    {
        if(!_isActive)
            return;

        //Exception: there is no active objects
        if(_activeWordObjects.Count == 0){
            _hasActiveWords = false;
            return;
        }

        _hasActiveWords = true;

        if(_possibleWords.Count == 0)
            ResetTypedWord();

        if (Input.anyKeyDown)
            CheckKeyPressed();
    }

    #region Set Up
    public void StartLevel(){
        EffectOnStart();

        ResetLevel();
        _spawner.StartSpawn();
        _timer.StartTimer(_startTime);
        
        _isActive = true;
    }

    public void ResetLevel(){
        //---Values
        _spawnInterval = SPAWN_INTERVAL_DEFAULT;
        _spawner.SetSpawnInterval(_spawnInterval);

        //Pego diretamente pelo SpawnerWords
        SpeedMultiplier = SPEED_MULTIPLIER_DEFAULT;

        _comboMultiplier = 1f;
        _comboCount = 0;

        _lastHitTime = 0f;

        _hasActiveWords = false;
        _isTypeWrong = false;

        Score = 0;
        MaxCombo = 1f;
        Hits = 0;
        TypeMiss = 0;

        //---Visual
        _inputText.text = "";
        UpdateVisualScorePoint();
        UpdateVisualCombo();
    }

    public void EndLevel(){
        EffectOnEnd();

        _isActive = false;
        _spawner.EndSpawn();
        ClearActiveWords();
        EventManager.LevelManager.OnLevelEnd.Get().Invoke();
    }

    [ButtonMethod]
    public void EndTimer(){
        _timer.EndTimer();
    }

    #endregion

    #region Word Typed

    private void CheckKeyPressed(){
        foreach (char keyPressed in Input.inputString)
        {
            //Exception: Enter
            if ((keyPressed== '\n') || (keyPressed == '\r')){
                Debug.Log("Enter: " + typedWord);
                continue;
            }

            //Backspace Pressed
            if (keyPressed == '\b'){
                //Exception: No char
                if(typedWord.Length == 0)
                    continue;

                typedWord = typedWord.Substring(0, typedWord.Length - 1);
            }
                
            //Default chars
            else{
                //Exception: first char space
                if(keyPressed == ' ' && typedWord.Length == 0)
                    continue;

                typedWord += keyPressed;
            }
            
            //Reset check
            _hitOrMissThisFrame = false;

            //Check for Match, Hits and misses
            CheckForMatch();

            //---Visuals
            //Text
            UpdateVisualInputText();

            //Default Effect
            if(!_hitOrMissThisFrame)
                EffectOnType();
        }
    }

    private void CheckForMatch(){
        //Remove no final
        List<FallingWord> removeFromPossibles = new List<FallingWord>();

        //O foreach é usado na lista de todos os objetos ativos, porque o jogador pode apagar, ou seja, o que anteriormente não estaria na lista de possibilidades pode entrar novamente.
        foreach(FallingWord wordObject in _activeWordObjects.ToList()){
            //Typed corret
            if(wordObject.Word.StartsWith(typedWord, StringComparison.InvariantCultureIgnoreCase)){
                //Type está correto
                _isTypeWrong = false;

                //Hit Word
                if(wordObject.Word.Equals(typedWord, StringComparison.InvariantCultureIgnoreCase)){
                    OnHit(wordObject);
                    return;
                }

                //Retorna a lista de possibilidades, se for preciso
                if(!_possibleWords.Contains(wordObject))
                    _possibleWords.Add(wordObject);

                //Default Effect
                EffectWordType(wordObject);
            }

            //Typed Incorrect
            else if(_possibleWords.Contains(wordObject)){
                removeFromPossibles.Add(wordObject);
            }
        }

        //Só existe uma situação em que possibleWord == 0
        //Quando erra a primeira letra
        if(_possibleWords.Count == 0){
            _hitOrMissThisFrame = true;
            EffectOnTypeMiss();
            return;
        }

        //Return if doesnt have words to remove
        if(removeFromPossibles.Count == 0)
            return;
        
        //Check if all words will be removed
        if(removeFromPossibles.Count == _possibleWords.Count){
            //Keeps the last word
            removeFromPossibles.Remove(_possibleWords[0]);

            //Effects Continuos
            EffectWordContinuosMiss(_possibleWords[0]);

            //Effect FirstMiss
            if(!_isTypeWrong){
                _isTypeWrong = true;
                OnTypeMiss();
            }
        }

        //Remove all others misses
        foreach(FallingWord wordForRemove in removeFromPossibles){
            _possibleWords.Remove(wordForRemove);
            wordForRemove.ResetColor();
        }
    }
    
    private void OnTypeMiss(){
        _hitOrMissThisFrame = true;
        //Effect
        EffectOnTypeMiss();
        
        //Score
        SubtractScore();

        //Combo
        SubtractCombo();

        //Statistic
        TypeMiss++;
    }

    public void OnWordMiss(FallingWord wordObject){
        //Effect
        EffectOnWordMiss();

        //Push back the words
        ClearActiveWords();

        //Reset Input
        ResetTypedWord();
    }

    private void OnHit(FallingWord wordObject){
        _hitOrMissThisFrame = true;

        //Statistic
        Hits++;
        //Score
        AddScore(wordObject.Word.Length);
        //Combo
        AddCombo();
        // Ajusta Intervalo
        DecreaseSpawnInterval(CalcRactionTime());

        //Reset Input
        ResetTypedWord();

        //Disable Word
        RemoveWord(wordObject);

        //Effect
        EffectOnWordHit(wordObject);
    }

    private void ResetTypedWord()
    {
        typedWord = "";
        UpdateVisualInputText();

        foreach(FallingWord word in _possibleWords)
            word.ResetColor();

        _possibleWords.Clear(); // Limpa a lista de palavras possíveis
    }

    public void AddWord(FallingWord wordObject)
    {
        _activeWordObjects.Add(wordObject);

        //Invertal
        IncreaseSpawnInterval();

        if(!_hasActiveWords)
            _lastHitTime = Time.time;
    }

    public void RemoveWord(FallingWord wordObject)
    {
        _activeWordObjects.Remove(wordObject);
        _possibleWords.Remove(wordObject);
    }

    private void ClearActiveWords(){
        foreach(FallingWord wordObject in _activeWordObjects.ToList()){
            RemoveWord(wordObject);
            wordObject.OnMissWord();
        }
    }

    #endregion

    #region Combo & Score

    private void AddScore(int numberOfCharacter){
        Score += Mathf.RoundToInt(numberOfCharacter * SCORE_GAIN * _comboMultiplier);
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
        _spawnInterval = Mathf.Clamp(_spawnInterval + adjustedInterval, SPAWN_INTERVAL_MIN, SPAWN_INTERVAL_MAX);

        // Atualiza o spawn interval no Spawner
        _spawner.SetSpawnInterval(_spawnInterval);

        // Debug.Log($"Novo Spawn Interval: {_spawnInterval} (Reação: {reactionTime}, Ajuste: {intervalAdjustment}, Expo: {adjustedInterval})");
    }

    private void IncreaseSpawnInterval(){
        // --- --- --- Exponencial --- --- ---
        float increaseTime = CalcExponencial(SPAWN_INTERVAL_INCREASE, true);

        // --- --- --- Spawn Interval --- --- ---
        _spawnInterval = Mathf.Min(_spawnInterval + increaseTime, SPAWN_INTERVAL_MAX);
        
        // Atualiza o spawn interval no Spawner
        _spawner.SetSpawnInterval(_spawnInterval);

        // Debug.Log($"Increase Interval: {_spawnInterval}, Increase: {increaseTime})");
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
        float normalizedInterval = Mathf.InverseLerp(SPAWN_INTERVAL_MIN, SPAWN_INTERVAL_MAX, _spawnInterval);

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

    #endregion

    #region AudioVisual Effects

    private void EffectOnStart(){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LevelStart);
    }

    private void EffectOnEnd(){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.LevelEnd);
    }

    private void EffectOnType(){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Type);
    }

    private void EffectOnTypeMiss(){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.TypeMiss);
        ShakeCamera(_shakeOnTypeMiss);
    }

    private void EffectOnWordHit(FallingWord wordObject){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.WordHit);
        Tween.Custom(_zoomOnWordHit,onValueChange: newVal => Camera.main.orthographicSize = newVal);
        wordObject.OnHitWord();
    }

    private void EffectOnWordMiss(){
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.WordMiss);
        ShakeCamera(_shakeOnWordMiss);
    }

    private void EffectWordType(FallingWord wordObject){
        //Effect
        wordObject.OnType(typedWord);
    }

    private void EffectWordContinuosMiss(FallingWord wordObject){
        //Effect
        wordObject.OnTypeMiss(typedWord);
    }

    private void UpdateVisualInputText(){
        _inputText.text = typedWord;
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
