using UnityEngine;
using FMODUnity;
using MyBox;

public class FMODEvents : Singleton<FMODEvents>
{
    // [field: Header("Temp")]
    // [field: SerializeField] public EventReference temp { get; private set;}

    [field: Header("Music")]
    [field: SerializeField] public EventReference MusicMenu { get; private set;}
    [field: SerializeField] public EventReference MusicTradutor { get; private set;}

    [field: Header("UI")]
    [field: SerializeField] public EventReference ButtonHover { get; private set;}
    [field: SerializeField] public EventReference ButtonClick { get; private set;}
    [field: SerializeField] public EventReference ButtonPlay { get; private set;}

    [field: Header("LoadingScreen")]
    [field: SerializeField] public EventReference LoadingScreenStart { get; private set;}
    [field: SerializeField] public EventReference LoadingScreenEnd { get; private set;}

    [field: Header("Level1")]
    [field: SerializeField] public EventReference Type { get; private set;}
    [field: SerializeField] public EventReference TypeMiss { get; private set;}
    [field: SerializeField] public EventReference WordSpawn { get; private set;}
    [field: SerializeField] public EventReference WordHit { get; private set;}
    [field: SerializeField] public EventReference WordMiss { get; private set;}
    [field: SerializeField] public EventReference LevelStart { get; private set;}
    [field: SerializeField] public EventReference LevelEnd { get; private set;}
}
