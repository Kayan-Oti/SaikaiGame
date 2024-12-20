using System.IO; // Para manipulação de arquivos
using UnityEngine; // Para utilizar no Unity

public class TxtIncorrectMaker : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Nome do arquivo de entrada (inclua a extensão .txt)")]
    public string inputFileName = "input.txt";
    
    [Tooltip("Nome do arquivo de saída (inclua a extensão .txt)")]
    public string outputFileName = "output.txt";

    private void Start()
    {
        ProcessFile();
    }

    private void ProcessFile()
    {
        // Caminho completo do arquivo de entrada
        string inputPath = Path.Combine(Application.dataPath, inputFileName);
        string outputPath = Path.Combine(Application.dataPath, outputFileName);

        // Verifica se o arquivo de entrada existe
        if (!File.Exists(inputPath))
        {
            Debug.LogError("Arquivo de entrada não encontrado: " + inputPath);
            return;
        }

        // Lê todas as linhas do arquivo de entrada
        string[] lines = File.ReadAllLines(inputPath);

        // Lista para armazenar as linhas processadas
        string[] processedLines = new string[lines.Length];

        // Processa cada linha
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim(); // Remove espaços extras
            if (string.IsNullOrEmpty(line))
            {
                // Ignora linhas vazias
                processedLines[i] = string.Empty;
                continue;
            }

            string modifiedLine = SwapAdjacentLetters(line); // Troca letras
            processedLines[i] = modifiedLine; // Salva no array
        }

        // Salva as linhas processadas no arquivo de saída
        File.WriteAllLines(outputPath, processedLines);
        Debug.Log("Arquivo processado salvo em: " + outputPath);
    }

    private string SwapAdjacentLetters(string input)
    {
        char[] characters = input.ToCharArray();

        // Verifica se a string tem pelo menos 2 caracteres para trocar
        if (characters.Length > 2)
        {
            // Encontra um par aleatório de índices próximos para trocar
            int index = Random.Range(1, characters.Length - 1);
            int secondIndex = index-1;
            if(secondIndex < 1)
                secondIndex = index+1;

            // Troca os caracteres
            char temp = characters[index];
            characters[index] = characters[secondIndex];
            characters[secondIndex] = temp;
        }

        return new string(characters);
    }
}
