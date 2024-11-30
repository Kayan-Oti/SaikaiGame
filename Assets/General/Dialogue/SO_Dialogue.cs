using UnityEngine;

[CreateAssetMenu(menuName = "DATA/DialogueData")]
public class SO_Dialogue : ScriptableObject
{
    public string Name;
    public bool loop;
    [TextArea(5,10)]
    public string[] Paragraphs;
}
