using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Button_Play : UI_Button
{
    protected override void OnClickEvent()
    {
        _onclick.Invoke();
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ButtonPlay, transform.position);
    }
}
