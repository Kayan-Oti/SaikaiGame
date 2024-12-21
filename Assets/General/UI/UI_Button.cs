using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Button : UI_Abstract_Selectable, IPointerEnterHandler, IPointerExitHandler, ISubmitHandler, IPointerClickHandler
{
    [SerializeField] protected float _scaleEffect = 1.15f;
    [SerializeField] protected float _duration = 0.25f;
    [SerializeField] private Navigation _navigation;
    [SerializeField] protected UnityEvent _onclick;
    [SerializeField] protected UnityEvent _onSelect;
    [SerializeField] protected UnityEvent _onDeselect;
    protected override void OnSelectDo(BaseEventData eventData)
    {
        OnSelectEvent();
    }

   protected override void OnDeselectDo(BaseEventData eventData)
    {
        OnDeselectEvent();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        eventData.selectedObject = gameObject;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        eventData.selectedObject = null;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickEvent();
    }
    public void OnSubmit(BaseEventData eventData)
    {
        OnClickEvent();
    }

    protected virtual void OnClickEvent(){
        _onclick.Invoke();
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ButtonClick, transform.position);
        OnDeselectEvent();
    }

    protected virtual void OnSelectEvent(){
        _onSelect.Invoke();
        Vector3 scale  = new Vector3(_scaleEffect, _scaleEffect, 1);
        transform.DOScale(scale, _duration);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ButtonHover, transform.position);
    }

    protected virtual void OnDeselectEvent(){
        _onDeselect.Invoke();
        transform.DOScale(Vector3.one, _duration);
    }


}