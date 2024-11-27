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
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _inputText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _comboText;

    [Header("Game Settings")]
    private int _comboCount = 0; // Contador de combos
    private float _comboMultiplier = 1f; // Multiplicador de pontos baseado no combo
    private float _spawnInterval = SPAWN_INTERVAL_DEFAULT; // Intervalo entre os spawns (usado no Spawner)
    public float SpeedMultiplier {get; private set;} = 0.25f ; // Multiplicador de velocidade das palavras


    //Combo + Score
    private const int SCORE_GAIN = 100;
    private const int SCORE_LOST = 25;
    private const int COMBO_PENALTY = 3; // Penalidade no combo ao errar

    // Fall Speed
    private const float SPEED_MULTIPLIER_INCREMENT = 0.010f; // Aumento a velocidade

    //Spawn
    private const float SPAWN_INTERVAL_MIN = 1.0f; // Valor mínimo do intervalo de spawn
    private const float SPAWN_INTERVAL_MAX = 6.0f; // Valor máximo do intervalo de spawn
    private const float SPAWN_INTERVAL_DEFAULT= 3.5f; // Valor máximo do intervalo de spawn
    private const float SPAWN_INTERVAL_INCREASE = 0.25f; // Valor usado ao spawnar uma palavra

    // Reaction
    private const float REACTION_TIME_MIN = 2f; // Ponto máximo de redução
    private const float REACTION_TIME_MEDIUM = 4f; // Até esse valor diminui o interval, depois dele aumenta
    private const float REACTION_TIME_MAX = 10.0f; // Ponto máximo de aumento
    private const float REACTION_TIME_ADJUST_HIT = 0.65f;

    private float _lastHitTime = 0f; // Momento do último acerto
    private bool _hasActiveWords = false;

    private void OnEnable()
    {
        // Subscrição ao evento do timer
        CountDownTimer.OnSpeedMultiplierIncrease += IncreaseSpeedMultiplier;
    }

    private void OnDisable()
    {
        // Remoção da subscrição ao evento
        CountDownTimer.OnSpeedMultiplierIncrease -= IncreaseSpeedMultiplier;
    }

    private void Start(){
        _inputText.text = "";
        UpdateVisualScorePoint();
        UpdateVisualCombo();
    }

    private void Update()
    {
        //Exception: there is no active objects
        if(activeWordObjects.Count == 0){
            _hasActiveWords = false;
            return;
        }

        _hasActiveWords = true;

        if(possibleWords.Count == 0)
            ResetTypedWord();

        if (Input.anyKeyDown)
        {
            string keyPressed = Input.inputString;

            //Ignora Backspace e Enter
            if (keyPressed.Length == 1)
            {
                typedWord += keyPressed;
                CheckForMatch();
                UpdateVisualInputText();
            }
        }
    }

    #region Word Typed
    private void CheckForMatch(){
        foreach(FallingWord wordObject in possibleWords.ToList()){
            if(wordObject.Word.StartsWith(typedWord, StringComparison.InvariantCultureIgnoreCase)){
                //Set Color
                wordObject.SetColor(typedWord);

                if(wordObject.Word.Equals(typedWord, StringComparison.InvariantCultureIgnoreCase)){
                    OnHit(wordObject);
                    return;
                }
            }else{
                //Reset Color
                wordObject.ResetColor();
                possibleWords.Remove(wordObject);
            }
        }

        //Typed Miss
        if (possibleWords.Count == 0)
            OnMiss();
    }

    private void OnMiss(){
        //Score
        SubtractScore();

        //Combo
        SubtractCombo();
        
        //Reset Input
        ResetTypedWord();
    }

    private void OnHit(FallingWord word){
        // Calcula o tempo de reação
        float reactionTime = CalcRactionTime();

        //Score
        AddScore();

        //Combo
        AddCombo();

        // Ajusta Intervalo
        AdjustSpawnInterval(reactionTime);

        //Disable Word
        RemoveWord(word, true);

        //Reset Input
        ResetTypedWord();
    }

    private void ResetTypedWord()
    {
        typedWord = "";
        possibleWords.Clear(); // Limpa a lista de palavras possíveis
        possibleWords = activeWordObjects.ToList();
    }

    public void AddWord(FallingWord wordObject)
    {
        activeWordObjects.Add(wordObject);

        //Invertal
        IncreaseInterval();

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
            if (possibleWords.Count == 0)
                OnMiss();
        }
    }

    #endregion

    #region Combo & Score

    private void AddScore(){
        score += Mathf.RoundToInt(SCORE_GAIN * _comboMultiplier);
        UpdateVisualScorePoint();
    }

    private void SubtractScore(){
        score -= SCORE_LOST;
        UpdateVisualScorePoint();
    }

    private void AddCombo(){
        _comboCount++;
        _comboMultiplier = 1f + (_comboCount * 0.1f);
        UpdateVisualCombo();
    }

    private void SubtractCombo(){
        _comboCount = Mathf.Max(0, _comboCount - COMBO_PENALTY);
        _comboMultiplier = 1f + (_comboCount * 0.1f);
        UpdateVisualCombo();
    }

    #endregion

    #region Dificulty

    private void AdjustSpawnInterval(float reactionTime)
    {
        // --- --- --- Reaction Time --- --- ---        
        float reactionFactor = 0;
        float intervalAdjustment = 0;

        if(reactionTime <= REACTION_TIME_MEDIUM){
            // Calcula o desempenho em relação ao Reaction Time
            // Retorna em porcentagem (0 a 1)
            reactionFactor = Mathf.InverseLerp(REACTION_TIME_MIN, REACTION_TIME_MEDIUM, reactionTime);
            // Calcula o quanto será extraído do Reaction Time Adjust
            intervalAdjustment = Mathf.Lerp(-REACTION_TIME_ADJUST_HIT, 0, reactionFactor);
        }else{
            // Calcula o desempenho em relação ao Reaction Time
            // Retorna em porcentagem (0 a 1)
            reactionFactor = Mathf.InverseLerp(REACTION_TIME_MEDIUM, REACTION_TIME_MAX, reactionTime);
            // Calcula o quanto será extraído do Reaction Time Adjust
            intervalAdjustment = Mathf.Lerp(0, REACTION_TIME_ADJUST_HIT, reactionFactor);
        }

        // --- --- --- Exponencial --- --- ---
        float adjustedInterval = CalcExponencial(intervalAdjustment);

        // --- --- --- Spawn Interval --- --- ---

        // Atualiza o intervalo acumulativamente
        _spawnInterval = Mathf.Clamp(_spawnInterval + adjustedInterval, SPAWN_INTERVAL_MIN, SPAWN_INTERVAL_MAX);

        Debug.Log($"Novo Spawn Interval: {_spawnInterval} (Reação: {reactionTime}, Ajuste: {intervalAdjustment}, Expo: {adjustedInterval})");
        // Atualiza o spawn interval no Spawner
        SpawnerWords.Instance.SetSpawnInterval(_spawnInterval);
    }

    private float CalcRactionTime(){
        // Calcula o tempo de reação desde o último acerto
        float currentTime = Time.time; // Tempo atual
        float reactionTime = _lastHitTime > 0 ? currentTime - _lastHitTime : 1f; // Default: 1s para o primeiro acerto
        _lastHitTime = currentTime; // Atualiza o tempo do último acerto

        return reactionTime;
    }

    private float CalcExponencial(float intervalAdjustment, bool inversedExpo = false){

        // Normaliza o spawnInterval entre 0 e 1 (onde 0 é o valor mínimo e 1 é o valor máximo)
        float normalizedInterval = Mathf.InverseLerp(SPAWN_INTERVAL_MIN, SPAWN_INTERVAL_MAX, _spawnInterval);

        //Inverte a curva
        if(inversedExpo)
            normalizedInterval = 1 - normalizedInterval;

        // Aplica uma curva exponencial ao valor normalizado. Isso amplifica o efeito do ajuste quando o spawnInterval está perto do máximo
        float exponentialFactor = Mathf.Pow(2f, normalizedInterval) - 1;  // Curva exponencial

        // Multiplica o intervalAdjustment pelo fator exponencial
        return intervalAdjustment * exponentialFactor;
    }

    private void IncreaseInterval(){
        float increaseTime = CalcExponencial(SPAWN_INTERVAL_INCREASE, true);
        _spawnInterval = Mathf.Min(_spawnInterval + increaseTime, SPAWN_INTERVAL_MAX);
        
        Debug.Log($"Increase Interval: {_spawnInterval}, Increase: {increaseTime})");

        // Atualiza o spawn interval no Spawner
        SpawnerWords.Instance.SetSpawnInterval(_spawnInterval);
    }

    #endregion

    #region Event
    private void IncreaseSpeedMultiplier()
    {
        SpeedMultiplier += SPEED_MULTIPLIER_INCREMENT;
        Debug.Log($"SpeedMultiplier: {SpeedMultiplier}");
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
