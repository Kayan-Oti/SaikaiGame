using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyWordArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto é uma palavra
        if (other.CompareTag("Word"))
        {
            FallingWord word = other.GetComponent<FallingWord>();
            TypeGameManager.Instance.RemoveWord(word, false);
        }
    }
}
