using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PrimeTween;

public class FallingWord : MonoBehaviour
{
    [SerializeField] public string Word; // A palavra que o jogador precisa digitar
    [SerializeField] private float baseSpeed = 1f; // Velocidade base
    [SerializeField] private TextMeshPro _textMesh;

    [Header("Ajuste de texto")]
    public Vector2 padding = new Vector2(0.5f, 0.5f); // Espaço extra ao redor do texto

    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private ParticleSystem _particlesOnHit;
    [SerializeField] private ParticleSystem _particlesOnMiss;
    [SerializeField] private TweenSettings<float> _tweenOnSpawn;
    [SerializeField] private TweenSettings<Vector3> _tweenOnDestroy;
    [SerializeField] private ShakeSettings _shakeOnType;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _highlightColor;
    private Collider2D _collider;
    private float speedMultiplier = 1f; // Multiplicador de velocidade
    private float _spawnTime; // Tempo em que a palavra aparece na tela
    private bool _isActive = false;


    private void OnEnable()
    {
        _spawnTime = Time.time; // Armazena o tempo de spawn da palavra
    }
    private void Start(){
        _collider = GetComponent<Collider2D>();
        ResetColor();
    }

    private void Update()
    {
        if(!_isActive)
            return;
        // Faz a palavra cair
        transform.Translate(Vector3.down * baseSpeed * speedMultiplier * Time.deltaTime);
    }

    public void StartFallingWord(string word, float multiplier){
        //Word
        Word = word;
        _textMesh.text = word;
        AdjustSize();

        //Speed
        speedMultiplier = multiplier;

        Tween.PositionY(target: transform, settings: _tweenOnSpawn)
            .OnComplete(target: this, target => target._isActive = true);
    }

    #region Settings
    
    private void AdjustSize()
    {
        // Garante que o TextMeshPro atualize seus valores
        _textMesh.ForceMeshUpdate();

        // Obtém os bounds renderizados do texto
        Bounds textBounds = _textMesh.bounds;

        // Calcula o tamanho dos bounds em relação à escala do texto
        Vector3 textScale = _textMesh.transform.lossyScale;
        Vector2 adjustedSize = new Vector2(
            textBounds.size.x * textScale.x,
            textBounds.size.y * textScale.y
        );

        // Ajusta o tamanho do SpriteRenderer com base no texto
        _spriteRenderer.size = adjustedSize + padding;
    }

    public void SetColor(string typedWord){
        // Inicializa partes formatadas
        string correctPart = "";
        string incorrectPart = "";
        string remainingPart = "";

        bool isIncorret = false;
        for (int i = 0; i < typedWord.Length; i++)
        {
            if (i >= Word.Length)
                break;
            
            //Se já foi incorreto anteriormente, pula direto pro else
            if (!isIncorret && char.ToUpper(typedWord[i]) == char.ToUpper(Word[i])){
                correctPart += $"<color=yellow>{Word[i]}</color>";
            }
            else{
                if (Word[i] == ' ') // Espaço em branco correto
                    incorrectPart += $"<color=red>_</color>";
                else
                    incorrectPart += $"<color=red>{Word[i]}</color>";
                isIncorret = true;
            }
        }

        // Parte restante da palavra que ainda não foi digitada
        if (typedWord.Length < Word.Length)
            remainingPart = Word.Substring(typedWord.Length);
        

        // Monta o texto formatado
        string formattedText = correctPart + incorrectPart + remainingPart;
        _textMesh.text = formattedText;

        //Box Color
        _spriteRenderer.color = correctPart != "" ? _highlightColor : _defaultColor;
    }

    public void ResetColor(){
        _textMesh.text = Word;
        _spriteRenderer.color = _defaultColor;
    }

    #endregion

    #region Events

    public void OnType(string typedWord){
        SetColor(typedWord);
        Tween.ShakeLocalRotation(target: transform, _shakeOnType);
    }

    public void OnTypeMiss(string typedWord){
        SetColor(typedWord);

    }

    public void OnHitWord(){
        AnimationEffect(_particlesOnHit);
    }

    public void OnMissWord(){
        AnimationEffect(_particlesOnMiss);
    }

    private void AnimationEffect(ParticleSystem particleSystem){
        _isActive = false;
        _collider.enabled = false;

        Tween.Scale(target: transform, _tweenOnDestroy)
        .OnComplete(target: this, target => target.ParticleEffect(particleSystem));
    }

    private void ParticleEffect(ParticleSystem particleSystem){
        _spriteRenderer.enabled = false;
        _textMesh.enabled = false;

        particleSystem.Play();

        float totalDuration = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
        Destroy(gameObject, totalDuration);
    }

    #endregion
}
