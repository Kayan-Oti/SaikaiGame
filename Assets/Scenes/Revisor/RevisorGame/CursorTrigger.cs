using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorTrigger : MonoBehaviour
{
    private Collider2D _collider;
    private TrailRenderer _trail;

    public bool IsActive {get; private set;} = false;

    private void Awake() {
        _collider = GetComponent<Collider2D>();
        _trail = GetComponentInChildren<TrailRenderer>();
    }
    
    private void OnEnable()
    {
        IsActive = false;
        
        _collider.enabled = false;
        _trail.enabled = false;
    }

    private void OnDisable()
    {
        IsActive = false;
        
        _collider.enabled = false;
        _trail.enabled = false;
    }
    

    public void EnableTrigger(){
        if(IsActive)
            return;
        
        IsActive = true;

        _collider.enabled = true;
        _trail.enabled = true;
        _trail.Clear();
    }

    public void DisableTrigger(){
        if(!IsActive)
            return;

        IsActive = false;
        
        _collider.enabled = false;
        _trail.enabled = false;
    }

}
