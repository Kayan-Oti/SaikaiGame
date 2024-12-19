using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PrimeTween;
using System;

public class TutorialRevisor_FallingWord : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string _word;
    [SerializeField] private bool _isCorrect;

    [Header("Components")]
    [SerializeField] private TextMeshPro _textMesh;
    [SerializeField] private BoxCollider2D _collider;

    [Header("Settings")]
    public Vector2 padding = new Vector2(0.4f, 0.4f); // Espaço extra ao redor do texto

    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private ParticleSystem _particlesOnHit;
    [SerializeField] private TweenSettings<Vector3> _tweenOnDestroy;
    [SerializeField] private ShakeSettings _shakeOnMiss;

    [Header("Tutorial Settings")]
    [SerializeField] private TweenSettings<Vector3> _fallingWordAnimation;
    public event Action OnDestroyTrigger;

    private void Start() {
        _textMesh.text = _word;
        AdjustSize();
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
    }

    public void StartFalling(){
        Tween.LocalPositionAtSpeed(target: transform, settings: _fallingWordAnimation);
    }

    private void OnDestroy(){
        OnDestroyTrigger?.Invoke();
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

        // Ajusta o tamanho do SpriteRenderer e Collider
        _spriteRenderer.size = adjustedSize + padding;
        _collider.size = adjustedSize;
    }

    #endregion

    #region Events

    private void OnHitWord(){
        //Avisa que foi destruido

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.WordHit);
        AnimationEffect(_particlesOnHit);
    }

    private void OnMissWord(){
        //Avisa que foi errado

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.TypeMiss);
        Tween.ShakeLocalRotation(target: transform, _shakeOnMiss);
    }

    private void AnimationEffect(ParticleSystem particleSystem){
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
