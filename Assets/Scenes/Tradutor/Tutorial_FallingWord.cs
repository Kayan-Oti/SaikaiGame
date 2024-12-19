using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PrimeTween;
using System;

public class Tutorial_FallingWord : MonoBehaviour
{
    [SerializeField] public string Word; // A palavra que o jogador precisa digitar
    [SerializeField] private TextMeshPro _textMesh;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private ParticleSystem _particles;
    [SerializeField] private TweenSettings<Vector3> _fallingWordAnimation;
    [Header("Ajuste de texto")]
    public Vector2 padding = new Vector2(0.5f, 0.5f); // Espaço extra ao redor do texto
    public event Action OnDestroyTrigger;

    private void Start(){
        SetWord(Word);
    }

    public void StartFalling(){
        Tween.LocalPositionAtSpeed(target: transform, settings: _fallingWordAnimation);
    }
    
    public void OnGetCorrectWord(){
        _particles.Play();
        OnDestroyWord(_particles);
    }

    private void OnDestroyWord(ParticleSystem particleMain){
        _spriteRenderer.enabled = false;
        _textMesh.enabled = false;

        float totalDuration = particleMain.main.duration + particleMain.main.startLifetime.constantMax;
        Destroy(gameObject, totalDuration);
    }

    private void OnDestroy(){
        OnDestroyTrigger?.Invoke();
    }

    #region Setup

    public void SetWord(string word){
        Word = word;
        _textMesh.text = word;
        AdjustSize();
    }

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

    #endregion

    #region Visual Effects

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

        _spriteRenderer.color = Color.gray;
    }

    public void ResetColor(){
        _textMesh.text = Word;
        _spriteRenderer.color = Color.black;
    }

    #endregion
}
