using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderVolumeSelect : UI_Abstract_Selectable
{
    private UI_SliderVolume _container;

    private void Start(){
        _container = GetComponentInParent<UI_SliderVolume>();
    }
    protected override void OnSelectDo(BaseEventData eventData)
    {
        _container.OnSelect();
    }

    protected override void OnDeselectDo(BaseEventData eventData)
    {
        _container.OnDeselect();
    }
}
