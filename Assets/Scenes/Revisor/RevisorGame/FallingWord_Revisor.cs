using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PrimeTween;
using MyBox;

public class FallingWord_Revisor : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] [ReadOnly] private string _word;
    [SerializeField] [ReadOnly] private bool _isCorrect;

    [Header("Components")]
    [SerializeField] private TextMeshPro _textMesh;
    [SerializeField] private BoxCollider2D _collider;

    [Header("Settings")]
    [SerializeField] private float _baseSpeed = 1f; // Velocidade base
    public Vector2 Padding = new Vector2(0.5f, 0.5f); // Espaço extra ao redor do texto

    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private ParticleSystem _particlesOnHit;
    [SerializeField] private ParticleSystem _particlesOnMiss;
    [SerializeField] private TweenSettings<Vector3> _tweenOnDestroy;
    private float _speedMultiplier = 1f; // Multiplicador de velocidade
    private bool _isActive = false;
    private float _horizontalSpeed = 0;
    private int _horizontalDirection = 0;
    private float MIN_SPEED_HORIZONTAL = 0.25f;

    private void Update()
    {
        if(!_isActive)
            return;
        // Faz a palavra cair
        transform.Translate(new Vector3(_horizontalSpeed * _horizontalDirection, -_baseSpeed * _speedMultiplier, 0) * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("CorrectTrigger")){
            if(_isCorrect)
                OnHitWord();
            else
                OnMissWord();
            return;
        }

        if(other.gameObject.CompareTag("IncorrectTrigger")){
            if(!_isCorrect)
                OnHitWord();
            else
                OnMissWord();
            return;
        }

        if(other.gameObject.CompareTag("DestroyArea")){
            LostWord();
            return;
        }

        if(other.gameObject.CompareTag("Wall")){
            _horizontalDirection*=-1;
            return;
        }
    }


    #region Settings
    public void StartFallingWord(string word, bool isCorrect, float speedMultiplier){
        //Word
        _word = word;
        _textMesh.text = word;
        AdjustSize();

        //Type
        _isCorrect = isCorrect;

        //Speed
        _speedMultiplier = speedMultiplier;

        _horizontalSpeed = Random.Range(0+MIN_SPEED_HORIZONTAL, _speedMultiplier);

        _horizontalDirection = Random.Range(-1,2);

        _isActive = true;
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

        // Ajusta o tamanho do SpriteRenderer e Collider
        _spriteRenderer.size = adjustedSize + Padding;
        _collider.size = adjustedSize;
    }

    #endregion

    #region Events

    private void OnHitWord(){
        RevisorGameManager.Instance.OnHit(this);
        AnimationEffect(_particlesOnHit);
    }

    private void OnMissWord(){
        RevisorGameManager.Instance.OnMiss(this);
        AnimationEffect(_particlesOnMiss);
    }

    private void LostWord(){
        RevisorGameManager.Instance.OnLostWord(this);
        AnimationEffect(_particlesOnMiss);
    }

    public void EffectOnEnd(){
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
