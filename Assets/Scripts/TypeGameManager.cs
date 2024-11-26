using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using TMPro;
using MyBox;

public class TypeGameManager : Singleton<TypeGameManager>
{
    [SerializeField] [ReadOnly] private List<FallingWord> activeWordObjects = new List<FallingWord>(); // Palavras na tela
    [SerializeField] [ReadOnly] private List<FallingWord> possibleWords = new List<FallingWord>(); // Lista de possíveis
    [SerializeField] [ReadOnly] private int score = 0; // Pontuação
    [SerializeField] [ReadOnly] private string typedWord;

    [Header("Game Settings")]
    private int _comboCount = 0; // Contador de combos
    private float _comboMultiplier = 1f; // Multiplicador de pontos baseado no combo
    private float _spawnInterval = SPAWN_INTERVAL_DEFAULT; // Intervalo entre os spawns (usado no Spawner)
    public float SpeedMultiplier {get; private set;} = 1f ; // Multiplicador de velocidade das palavras

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _inputText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _comboText;

    private const int SCORE_GAIN = 100;
    private const int SCORE_LOST = 25;
    private const float SPAWN_INTERVAL_ADJUST = 0.01f; // Ajuste do intervalo baseado no combo
    private const float SPAWN_INTERVAL_MIN = 0.3f; // Valor mínimo do intervalo de spawn
    private const float SPAWN_INTERVAL_MAX = 3f; // Valor máximo do intervalo de spawn
    private const float SPAWN_INTERVAL_DEFAULT= 1.5f; // Valor máximo do intervalo de spawn
    private const int COMBO_PENALTY = 3; // Penalidade no combo ao errar
    private const float SPEED_MULTIPLIER_INCREMENT = 0.02f; // Aumento a velocidade

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
        if(activeWordObjects.Count == 0)
            return;

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
                if(wordObject.Word.Equals(typedWord, StringComparison.InvariantCultureIgnoreCase)){
                    OnHit(wordObject);
                    return;
                }
            }else{
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

        // Ajusta Intervalo
        AdjustSpawnInterval();
        
        //Reset Input
        ResetTypedWord();
    }

    private void OnHit(FallingWord word){
        //Score
        AddScore();

        //Combo
        AddCombo();

        // Ajusta Intervalo
        AdjustSpawnInterval();

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

    private void AdjustSpawnInterval()
    {
        // Ajusta o intervalo de spawn baseado no combo
        if (_comboCount > 0)
            _spawnInterval = Mathf.Max(SPAWN_INTERVAL_MIN, SPAWN_INTERVAL_DEFAULT - (_comboCount * SPAWN_INTERVAL_ADJUST));
        else
            _spawnInterval =  Mathf.Min(SPAWN_INTERVAL_MAX, _spawnInterval + SPAWN_INTERVAL_ADJUST);

        Debug.Log(_spawnInterval);
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
        _comboText.text = "Combo: x" + _comboMultiplier.ToString("F1");
    }

    #endregion
}
