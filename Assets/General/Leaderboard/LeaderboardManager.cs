using System;
using System.Collections;
using Dan.Main;
using Dan.Models;
using MyBox;
using TMPro;
using UnityEngine;

[Serializable]
public enum Enum_LeaderBoardReference {
    Tradutor = 0,
    Revisor
}

public class LeaderboardManager : MonoBehaviour
{
    [Header("Leaderboard Essentials:")]
    [SerializeField] public Enum_LeaderBoardReference LeaderBoardEnum;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Transform _entryDisplayParent;
    [SerializeField] private EntryDisplay _entryDisplayPrefab;

    [Header("Loading UIs:")]
    [SerializeField] private CanvasGroup _blockInteract;
    [SerializeField] private CanvasGroup _LoadingCanvasGroup;
    [SerializeField] private TextMeshProUGUI _LoadingText;
    [SerializeField] private CanvasGroup _LoadingPlayerCanvasGroup;
    [SerializeField] private TextMeshProUGUI _LoadingPlayerText;
    [Header("Others UIs:")]
    [SerializeField] private TextMeshProUGUI _title;

    [Header("Search Query Essentials:")]
    [SerializeField] private TMP_InputField _pageInput;
    [SerializeField] private int _defaultPageNumber = 1, _defaultEntriesToTake = 100;
    [SerializeField] private Color _highlightPageInputColor;

    [Header("Player Data:")]
    [SerializeField] private EntryDisplay _personalEntryDisplay;
    [SerializeField] private TMP_InputField _playerUsernameInput;

    private LeaderboardReference _leaderboardReference;
    private int _maxPages = 1;
    private Entry _playerEntry;
    private Color _defaultPageInputColor;

    #region Setup
    private void Start()
    {
        SetLeaderboardReference();
        
        InitializeComponents();
        _canvas.enabled = false;

        _leaderboardReference.GetPersonalEntry(GetPlayerEntry, ErrorCallback);
    }

    public void ChangeLeaderBoard(Enum_LeaderBoardReference leaderboard){
        LeaderBoardEnum = leaderboard;
        SetLeaderboardReference();
        Reload();
    }

    private void SetLeaderboardReference(){
        if(LeaderBoardEnum == Enum_LeaderBoardReference.Tradutor){
            _leaderboardReference = Leaderboards.SaikaiEspecialNatal;
            _title.text = "Ranking Tradutor";
        }
        else if(LeaderBoardEnum == Enum_LeaderBoardReference.Revisor){
            _leaderboardReference = Leaderboards.SaikaiEspecialNatal2;
            _title.text = "Ranking Revisor";
        }
    }

    private void GetPlayerEntry(Entry entry){
        //Mesmo se não exister tudo bem, só preciso do Score
        _playerEntry = entry;

        //Se existe entry, atualiza o nome local
        if(entry.Rank != 0){
            GameManager.Instance.PlayerDisplayName = entry.Username;
        }
    }

    [ButtonMethod]
    public void OpenLeaderBoard(){
        _canvas.enabled = true;
        Reload();
    }

    [ButtonMethod]
    public void CloseLeaderBoard(){
        _canvas.enabled = false;
    }

    private void InitializeComponents()
    {
        //Loading Animations
        StartCoroutine(LoadingTextCoroutine(_LoadingText));
        StartCoroutine(LoadingTextCoroutine(_LoadingPlayerText));
        _blockInteract.blocksRaycasts = false;

        _defaultPageInputColor = _pageInput.image.color;
        _pageInput.onValueChanged.AddListener(_ => _pageInput.image.color = _highlightPageInputColor);

        _pageInput.placeholder.GetComponent<TextMeshProUGUI>().text = _defaultPageNumber.ToString();
    }
    #endregion

    #region Entries Managers
    public void Load()
    {
        var pageNumber = int.TryParse(_pageInput.text, out var pageValue) ? pageValue : _defaultPageNumber;
        pageNumber = Mathf.Clamp(pageNumber, 1, _maxPages);
        _pageInput.text = pageNumber.ToString();
        
        var take = _defaultEntriesToTake;
        take = Mathf.Clamp(take, 1, 100);
        
        var searchQuery = new LeaderboardSearchQuery
        {
            Skip = (pageNumber - 1) * take,
            Take = take
        };
        
        _pageInput.image.color = _defaultPageInputColor;
        
        _leaderboardReference.GetEntries(searchQuery, OnLeaderboardLoaded, ErrorCallback);
        _leaderboardReference.GetEntryCount(SetMaxPage, ErrorCallback);
        ToggleLoadingEntries(true);
    }

    public void SubmitNewEntry(int playerScore,  string extraData)
    {
        playerScore = Mathf.Max(0, playerScore);
        _leaderboardReference.UploadNewEntry(GameManager.Instance.PlayerDisplayName, playerScore, extraData, OnSubmit, ErrorCallback);
        ToggleLoadingEntries(true);
        ToggleLoadingPlayerEntrie(true);
    }

    public void GetPersonalEntry()
    {
        _leaderboardReference.GetPersonalEntry(OnPersonalEntryLoaded, ErrorCallback);
        ToggleLoadingPlayerEntrie(true);
    }

    #endregion

    #region CallBacks

    private void ErrorCallback(string error)
    {
        Debug.LogError(error);
    }
    private void OnSubmit(bool success)
    {
        if (success){
            Debug.Log("Submit - Sucess");
            Reload();
        }
        else{
            Debug.Log("Submit - Unsucess");
        }
    }
        

    private void OnLeaderboardLoaded(Entry[] entries)
    {
        //Clear Currents displays
        foreach (Transform t in _entryDisplayParent) 
            Destroy(t.gameObject);

        //Add New Displays
        foreach (var t in entries)
            CreateEntryDisplay(t);
        
        ToggleLoadingEntries(false);
    }

    private void OnPersonalEntryLoaded(Entry entry)
    {
        //Local entry
        _playerEntry = entry;

        //Display
        _personalEntryDisplay.SetEntry(entry);

        //Loading Visual
        ToggleLoadingPlayerEntrie(false);
    }

    private void SetMaxPage(int totalEntries){
        if(totalEntries == 0){
            _maxPages = 1;
            return;
        }

        var remainder = totalEntries % _defaultEntriesToTake;
        var numPages = (totalEntries - remainder)/_defaultEntriesToTake;

        _maxPages = numPages + (remainder > 0 ? 1 : 0);
    }

    #endregion

    #region Events

    //Call by Button
    public void ChangePageBy(int amount)
    {
        var pageNumber = int.TryParse(_pageInput.text, out var pageValue) ? pageValue : _defaultPageNumber;
        pageNumber = Mathf.Max(1,pageNumber+amount);
        _pageInput.text = pageNumber.ToString();
        Load();
    }

    //Call by Button
    public void Reload(){
        Load();
        //Load PlayerEntry
        GetPersonalEntry();
    }

    public void ChangeDisplayName(){
        if(_playerUsernameInput.text == "")
            return;
        
        GameManager.Instance.PlayerDisplayName = _playerUsernameInput.text;

        if(_playerEntry.Rank == 0){
            _personalEntryDisplay.SetName(_playerUsernameInput.text);
        }else{
            SubmitNewEntry(_playerEntry.Score, _playerEntry.Extra);
        }
    }

    public int GetCurrentScore(){
        return _playerEntry.Score;
    }

    #endregion

    #region Visual

    private void ToggleLoadingEntries(bool isOn){
        ToggleLoadingPanel(_LoadingCanvasGroup, isOn, true);
    }

    private void ToggleLoadingPlayerEntrie(bool isOn){
        ToggleLoadingPanel(_LoadingPlayerCanvasGroup, isOn, false);
    }

    private void ToggleLoadingPanel(CanvasGroup canvasGroup, bool isOn, bool blockIntect)
    {
        canvasGroup.alpha = isOn ? 1f : 0f;
        canvasGroup.interactable = isOn;
        canvasGroup.blocksRaycasts = isOn;

        if(blockIntect)
            _blockInteract.blocksRaycasts = isOn;
    }

    private IEnumerator LoadingTextCoroutine(TextMeshProUGUI textUI)
    {
        var loadingText = "Carregando";
        for (int i = 0; i < 3; i++)
        {
            loadingText += ".";
            textUI.text = loadingText;
            yield return new WaitForSeconds(0.25f);
        }
        
        StartCoroutine(LoadingTextCoroutine(textUI));
    }

    private void CreateEntryDisplay(Entry entry)
    {
        var entryDisplay = Instantiate(_entryDisplayPrefab.gameObject, _entryDisplayParent);
        entryDisplay.GetComponent<EntryDisplay>().SetEntry(entry);
    }

    #endregion

    #region Test

    [ButtonMethod]
    public void DeleteEntry()
    {
        _leaderboardReference.DeleteEntry(OnSubmit, ErrorCallback);
        ToggleLoadingEntries(true);
        ToggleLoadingPlayerEntrie(true);
    }

    [ButtonMethod]
    public void NewEntryTest()
    {
        SubmitNewEntry(0, "10");
    }

    #endregion

}
