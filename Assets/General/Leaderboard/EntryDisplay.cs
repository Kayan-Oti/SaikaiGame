using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dan.Models;


public class EntryDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rankText, _usernameText, _scoreText, _extraText;
        
    public void SetEntry(Entry entry)
    {
        //Default
        if(entry.Rank == 0){
            _rankText.text = "??";
            _usernameText.text = GameManager.Instance.PlayerDisplayName;
            _scoreText.text = "0";
            _extraText.text = "0";
            return;
        }

        _rankText.text = entry.RankSuffix();
        _usernameText.text = entry.Username;
        _scoreText.text = entry.Score.ToString();
        _extraText.text = entry.Extra.ToString();
        
        GetComponent<Image>().color = entry.IsMine() ? Color.yellow : Color.white;
    }

    public string GetRank(){
        return _rankText.text;
    }

    public void SetName(string username){
        _usernameText.text = username;
    }
}
