using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using TMPro;
using MyBox;

public class TypeGameManager : Singleton<TypeGameManager>
{
    [Header("Debug ReadOnly")]
    [SerializeField] [ReadOnly] private List<FallingWord> activeWordObjects = new List<FallingWord>(); // Palavras na tela
    [SerializeField] [ReadOnly] private List<FallingWord> possibleWords = new List<FallingWord>(); // Lista de possíveis
    [SerializeField] [ReadOnly] private int score = 0;
    [SerializeField] [ReadOnly] private string typedWord;

    [Header("GameObjects")]
    [SerializeField] private SpawnerWords _spawner;
    [SerializeField] private CountDownTimer _timer;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _inputText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _comboText;

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

    //Statistics
    private float _maxCombo = 1f;
    private int _hits = 0;
    private int _typeMiss = 0;

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
        if(activeWordObjects.Count == 0){
            _hasActiveWords = false;
            return;
        }

        _hasActiveWords = true;

        if(possibleWords.Count == 0)
            ResetTypedWord();

        if (Input.anyKeyDown)
            CheckKeyPressed();
    }

    #region Set Up
    public void StartLevel(){
        _isActive = true;
        ResetLevel();
        _spawner.StartSpawn();
        _timer.StartTimer();
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

        _maxCombo = 1f;
        _hits = 0;
        _typeMiss = 0;

        //---Visual
        _inputText.text = "";
        UpdateVisualScorePoint();
        UpdateVisualCombo();
    }

    public void EndLevel(){
        _isActive = false;
        _spawner.EndSpawn();
        ClearActiveWords();
        EventManager.LevelManager.OnLevelEnd.Get().Invoke();
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
            

            CheckForMatch();
            UpdateVisualInputText();
        }
    }

    private void CheckForMatch(){
        //O foreach é usado na lista de todos os objetos ativos, porque o jogador pode apagar, ou seja, o que anteriormente não estaria na lista de possibilidades pode entrar novamente.
        foreach(FallingWord wordObject in activeWordObjects.ToList()){
            //
            if(CompareTypeAndObjects(wordObject))
                break;
        }
    }

    private bool CompareTypeAndObjects(FallingWord wordObject){
        //Typed corret
        if(wordObject.Word.StartsWith(typedWord, StringComparison.InvariantCultureIgnoreCase)){
            //Type está correto
            _isTypeWrong = false;

            //Retorna a lista de possibilidades, se for preciso
            if(!possibleWords.Contains(wordObject))
                possibleWords.Add(wordObject);
            
            //Set Color
            wordObject.SetColor(typedWord);

            //Hit Word
            if(wordObject.Word.Equals(typedWord, StringComparison.InvariantCultureIgnoreCase)){
                OnHit(wordObject);
                return true;
            }
        }
        //Typed Incorrect
        else if(possibleWords.Contains(wordObject)){
            //Missed the last possibleWord
            if(possibleWords.Count == 1 && typedWord.Length > 1){
                wordObject.SetColor(typedWord);
                if(!_isTypeWrong){
                    _isTypeWrong = true;
                    OnTypeMiss();
                }
            }
            //Reset Color
            else{
                wordObject.ResetColor();
                possibleWords.Remove(wordObject);
            }
        }

        return false;
    }

    private void OnTypeMiss(){
        //Score
        SubtractScore();

        //Combo
        SubtractCombo();

        //Statistic
        _typeMiss++;
    }

    private void OnWordMiss(){
        //Push back the words
        ClearActiveWords();

        //Reset Input
        ResetTypedWord();
    }

    private void OnHit(FallingWord word){
        // Calcula o tempo de reação
        float reactionTime = CalcRactionTime();

        //Score
        AddScore(word.Word.Length);

        //Combo
        AddCombo();

        // Ajusta Intervalo
        DecreaseSpawnInterval(reactionTime);

        //Disable Word
        RemoveWord(word, true);

        //Reset Input
        ResetTypedWord();

        //Statistic
        _hits++;
    }

    private void ResetTypedWord()
    {
        typedWord = "";
        UpdateVisualInputText();

        foreach(FallingWord word in possibleWords)
            word.ResetColor();

        possibleWords.Clear(); // Limpa a lista de palavras possíveis
        possibleWords = activeWordObjects.ToList();
    }

    public void AddWord(FallingWord wordObject)
    {
        activeWordObjects.Add(wordObject);

        //Invertal
        IncreaseSpawnInterval();

        if(!_hasActiveWords)
            _lastHitTime = Time.time;
    }

    public void RemoveWord(FallingWord wordObject, bool typed)
    {
        activeWordObjects.Remove(wordObject);
        possibleWords.Remove(wordObject);

        if(typed){
            wordObject.OnGetCorrectWord();
        }
        // Quando a palavra atingir o DestroyWordArea
        else{
            wordObject.OnLostWord();
            OnWordMiss();
        }
    }

    private void ClearActiveWords(){
        foreach(FallingWord wordObject in activeWordObjects.ToList()){
            activeWordObjects.Remove(wordObject);
            possibleWords.Remove(wordObject);
            wordObject.OnLostWord();
        }
    }

    #endregion

    #region Combo & Score

    private void AddScore(int numberOfCharacter){
        score += Mathf.RoundToInt(numberOfCharacter * SCORE_GAIN * _comboMultiplier);
        UpdateVisualScorePoint();
    }

    private void SubtractScore(){
        score -= SCORE_LOST;
        UpdateVisualScorePoint();
    }

    private void AddCombo(){
        _comboCount++;
        _comboMultiplier = 1f + (_comboCount * 0.1f);

        if(_comboMultiplier > _maxCombo)
            _maxCombo = _comboMultiplier;

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

    #region Visual

    private void UpdateVisualInputText(){
        _inputText.text = typedWord;
    }

    private void UpdateVisualScorePoint(){
        _scoreText.text = score.ToString();
    }

    private void UpdateVisualCombo()
    {
        _comboText.text = "x" + _comboMultiplier.ToString("F1");
    }

    #endregion
}
