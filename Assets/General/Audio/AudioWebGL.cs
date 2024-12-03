using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AudioWebGL : MonoBehaviour
{
    [FMODUnity.BankRef]
    public List<string> Banks = new List<string>();
    private bool _banksLoaded = false; // Para garantir que os bancos sejam carregados apenas uma vez.

    void OnEnable()
    {
        #if UNITY_WEBGL
        // Inscreve-se no evento focusChanged
        Application.focusChanged += OnFocusChanged;
        #endif
    }

    void OnDisable()
    {
        #if UNITY_WEBGL
        // Remove a inscrição do evento para evitar problemas
        Application.focusChanged -= OnFocusChanged;
        #endif
    }

    #if UNITY_WEBGL
    void OnFocusChanged(bool hasFocus)
    {
        if (hasFocus)
        {
            // Retoma o mixer do FMOD
            RuntimeManager.CoreSystem.mixerResume();
        }
        else
        {
            // Pausa o mixer do FMOD
            RuntimeManager.CoreSystem.mixerSuspend();
        }
    }
    #endif

    void Update()
    {
        if (!_banksLoaded && Input.anyKeyDown)
        {
            Debug.Log("Interação detectada, iniciando carregamento de bancos...");
            OnFirstClick();
        }
    }

    public void OnFirstClick(){
        StartCoroutine(LoadBanks());
    }

    public IEnumerator LoadBanks(){
        foreach (var bank in Banks)
        {
            Debug.Log($"Carregando banco: {bank}");
            FMODUnity.RuntimeManager.LoadBank(bank, true);
        }

        // Verifica se todos os bancos foram carregados
        while (!FMODUnity.RuntimeManager.HaveAllBanksLoaded)
        {
            Debug.Log("Aguardando carregamento de todos os bancos...");
            yield return null;
        }

        // Verifica se os dados de amostra foram carregados
        while (FMODUnity.RuntimeManager.AnySampleDataLoading())
        {
            Debug.Log("Aguardando carregamento de dados de amostra...");
            yield return null;
        }

        Debug.Log("Todos os bancos foram carregados com sucesso.");
        _banksLoaded = true; // Marca como carregado para evitar chamadas repetidas
        AudioManager.Instance.ResetBus();
    }
}
