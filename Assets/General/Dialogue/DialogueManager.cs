using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MyBox;
using System;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private UI_ManagerAnimation _animationManager;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private GameObject _button;
    [SerializeField] private float typeSpeed = 15f;
    [SerializeField] public bool CanClick = true;
    private SO_Dialogue _dialogue;
    private Action _onEndDialogueAction;

    private Queue<string> _paragraphs = new Queue<string>();
    private Coroutine _typeDialogueCoroutine;
    private string _currentParagraph;
    private bool _isDialogueActive = false;
    private bool _isTyping = false;
    private const float MAX_TYPE_TIME = 0.5f;

    private void Start(){
        ResetText();
        SetButtonActivity(false);
    }

    private void Update() {
        if(!_isDialogueActive)
            return;
    }

    private void ResetText(){
        _nameText.text = "";
        _dialogueText.text = "";
    }

    private void SetButtonActivity(bool state){
        if(!CanClick)
            state = false;

        _button.SetActive(state);
    }

    public void StartDialogue(SO_Dialogue dialogue, Action onEndDialogueAction = null){
        _dialogue = dialogue;
        _onEndDialogueAction  = onEndDialogueAction;
        StartCoroutine(StartDialogueCoroutine());
    }

    private IEnumerator StartDialogueCoroutine(){
        ResetText();
        SetButtonActivity(true);

        //Wait Animation "Start" end
        yield return _animationManager.PlayAnimationCoroutine("Start");
        _isDialogueActive = true;
        
        //Display name
        _nameText.text = _dialogue.Name;

        //Add Paragraphs to Queue
        foreach(string paragraph in _dialogue.Paragraphs)
            _paragraphs.Enqueue(paragraph);

        //First Paragraph
        DisplayNextParagraph();
    }

    private void EndDialogue(){
        _paragraphs.Clear();
        SetButtonActivity(false);
        _isDialogueActive = false;

        //Libera o player ao terminar a animação
        _animationManager.PlayAnimation("End", ()  => _onEndDialogueAction?.Invoke());
    }

    public void Onclick(){
        if(_isDialogueActive){
            DisplayNextParagraph();
        }else{
            _animationManager.SkipAnimation("Start");
        }
    }

    [ButtonMethod]
    private void DisplayNextParagraph(){
        //Exception: No more paragraphs, Dialogue End
        if(_paragraphs.Count == 0 && !_isTyping){
            EndDialogue();
            return;
        }

        //Skip Typing
        if(_isTyping){
            SkipTyping();
        }
        
        //New Paragraph
        else{
            _currentParagraph = _paragraphs.Dequeue();
            _typeDialogueCoroutine = StartCoroutine(TypeDialogueText());
        }
    }

    private IEnumerator TypeDialogueText()
    {
        _isTyping = true;

        int maxVisibleChars = 0;

        _dialogueText.text = _currentParagraph;
        _dialogueText.maxVisibleCharacters = maxVisibleChars;        

        foreach (char c in _currentParagraph.ToCharArray())
        {
            maxVisibleChars++;
            _dialogueText.maxVisibleCharacters = maxVisibleChars;

            yield return new WaitForSeconds(MAX_TYPE_TIME / typeSpeed);
        }

        _isTyping = false;
    }

    private void SkipTyping(){
        StopCoroutine(_typeDialogueCoroutine);
        _dialogueText.maxVisibleCharacters = _currentParagraph.Length;
        _isTyping = false;
    }
}