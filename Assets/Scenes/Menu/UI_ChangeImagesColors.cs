using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChangeImagesColors : MonoBehaviour
{
    [SerializeField] private List<Image> _imagesList;
    [SerializeField] private Color _newColor;
    private List<Color> _defaultColor = new List<Color>();

    private void Start() {
        //Salva as cores padrões
        _imagesList.ForEach(image => _defaultColor.Add(image.color));
    }

    public void OnNewColor(){
        _imagesList.ForEach(image => image.color = _newColor);
    }

    public void OnResetColor(){
        for(int i = 0; i<_imagesList.Count;i++){
            _imagesList[i].color = _defaultColor[i];
        }
    }
}
