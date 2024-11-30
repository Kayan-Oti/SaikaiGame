using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UI_Abstract_Selectable : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        EventManager.GameManager.OnChangeCurrentSelectedUI.Get().Invoke(gameObject);
        OnSelectDo(eventData);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        OnDeselectDo(eventData);
    }

    protected abstract void OnSelectDo(BaseEventData eventData);
    protected abstract void OnDeselectDo(BaseEventData eventData);

}
