using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FallingWord : MonoBehaviour
{
    [SerializeField] public string Word; // A palavra que o jogador precisa digitar
    [SerializeField] private TextMeshPro _textMesh;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [SerializeField] private float baseSpeed = 1f; // Velocidade base
    private float speedMultiplier = 1f; // Multiplicador de velocidade

    [Header("Ajuste de texto")]
    public Vector2 padding = new Vector2(0.5f, 0.5f); // Espaço extra ao redor do texto

    private void Update()
    {
        // Faz a palavra cair
        transform.Translate(Vector3.down * baseSpeed * speedMultiplier * Time.deltaTime);
    }

    public void OnGetCorrectWord(){
        Destroy(gameObject);
    }

    public void OnLostWord(){
        Destroy(gameObject);
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

    public void SetWord(string word){
        Word = word;
        _textMesh.text = word;
        AdjustSize();
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }
}
