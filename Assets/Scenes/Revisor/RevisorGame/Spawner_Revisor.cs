using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class Spawner_Revisor : MonoBehaviour
{
    //Prefab
    [SerializeField] private GameObject _wordPrefab; // Prefab FallingWord

    //Spawn Area
    [SerializeField] private Transform _spawnPoint; // Ponto inicial de spawn
    [SerializeField] private Vector2 _spawnRangeX = new Vector2(-5f, 5f); // Limite horizontal para spawn

    //Timer
    [SerializeField] private float _spawnInterval = 1f; // Intervalo de spawn em segundos
    private float spawnTimer;
    private bool _isActive = false;

    private List<string> _correctWordList;
    private List<string> _incorrectWordList;


    private void Start(){
        LoadWords(ref _correctWordList, "words_correct");
        LoadWords(ref _incorrectWordList, "words_incorrect");
    }

    private void Update()
    {
        if(!_isActive)
            return;

        // Controla o temporizador de spawn
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= _spawnInterval)
        {
            _spawnInterval = RevisorGameManager.Instance.SpawnInterval;
            spawnTimer = 0;
            
            SpawnWord();
        }
    }

    [ButtonMethod]
    public void StartSpawn(){
        _isActive = true;
        spawnTimer = 0;
        
        SpawnWord();
    }

    public void EndSpawn(){
        _isActive = false;
    }

    private void LoadWords(ref List<string> list, string asssetName)
    {
        TextAsset wordFile = Resources.Load<TextAsset>(asssetName);
        if (wordFile != null)
        {
            list = new List<string>(wordFile.text.Split('\n'));
            for (int i = 0; i < list.Count; i++)
                list[i] = list[i].Trim(); // Remove espaços ou quebras desnecessárias
        }
        else
            Debug.LogError($"Arquivo {asssetName}.txt não encontrado!");
    }

    private void SpawnWord()
    {
        // Gera uma posição aleatória no intervalo X
        float randomX = Random.Range(_spawnRangeX.x, _spawnRangeX.y);
        Vector3 spawnPosition = new Vector3(randomX, _spawnPoint.position.y, _spawnPoint.position.z);

        // Instancia o prefab na posição calculada
        FallingWord_Revisor wordObject = Instantiate(_wordPrefab, spawnPosition, Quaternion.identity, transform).GetComponent<FallingWord_Revisor>();

        //Exepction
        if (wordObject == null){
            Debug.LogError("WordObject is Null");
            return;
        }

        //Randomico certo ou errado
        float randomWordType = Random.Range(0,2);

        //Correct Word
        if(randomWordType == 0){
            wordObject.StartFallingWord(GetRandomWord(_correctWordList), true, RevisorGameManager.Instance.SpeedMultiplier);
        }
        //Incorrect Word
        else{
            wordObject.StartFallingWord(GetRandomWord(_incorrectWordList), false, RevisorGameManager.Instance.SpeedMultiplier);
        }

        RevisorGameManager.Instance.AddWord(wordObject);
        
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.WordSpawn);
    }

    public void SetSpawnInterval(float newInterval)
    {
        _spawnInterval = newInterval;
    }

    private string GetRandomWord(List<string> wordList)
    {
        return wordList[Random.Range(0, wordList.Count)];
    }
}
