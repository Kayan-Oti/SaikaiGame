using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;

public class SpawnerWords : MonoBehaviour
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

    private List<string> wordList;

    private void Start(){
        LoadWords();
    }

    private void Update()
    {
        if(!_isActive)
            return;

        // Controla o temporizador de spawn
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= _spawnInterval)
        {
            SpawnWord();
            spawnTimer = 0f;
        }
    }

    public void StartSpawn(){
        _isActive = true;
        spawnTimer = 0;
        
        SpawnWord();
    }

    public void EndSpawn(){
        _isActive = false;
    }

    void LoadWords()
    {
        TextAsset wordFile = Resources.Load<TextAsset>("words");
        if (wordFile != null)
        {
            wordList = new List<string>(wordFile.text.Split('\n'));
            for (int i = 0; i < wordList.Count; i++)
                wordList[i] = wordList[i].Trim(); // Remove espaços ou quebras desnecessárias
        }
        else
            Debug.LogError("Arquivo words.txt não encontrado!");
    }

    private void SpawnWord()
    {
        // Gera uma posição aleatória no intervalo X
        float randomX = Random.Range(_spawnRangeX.x, _spawnRangeX.y);
        Vector3 spawnPosition = new Vector3(randomX, _spawnPoint.position.y, _spawnPoint.position.z);

        // Instancia o prefab na posição calculada
        GameObject newWordObject = Instantiate(_wordPrefab, spawnPosition, Quaternion.identity, transform);

        // Configura o texto dinamicamente
        FallingWord wordObject = newWordObject.GetComponent<FallingWord>();
        if (wordObject != null)
        {
            wordObject.SetWord(GetRandomWord()); // Define a palavra
            wordObject.SetSpeedMultiplier(TypeGameManager.Instance.SpeedMultiplier); // Passa a velocidade atual
            TypeGameManager.Instance.AddWord(wordObject);
        }
    }

    public void SetSpawnInterval(float newInterval)
    {
        _spawnInterval = newInterval;
    }

    private string GetRandomWord()
    {
        return wordList[Random.Range(0, wordList.Count)];
    }
}
