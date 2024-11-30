using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class EventManager
{
    public static readonly GameEvents GameManager = new GameEvents();
    public static readonly LevelEvents LevelManager = new LevelEvents();

    public class GenericEvent<T> where T: class, new()
    {
        private Dictionary<string, T> map = new Dictionary<string, T>();

        public T Get(string channel = ""){
            map.TryAdd(channel, new T());
            return map[channel];
        }
    }

    public class GameEvents{
        public class ChangingScene: UnityEvent {}
        public GenericEvent<ChangingScene> OnChangingScene = new GenericEvent<ChangingScene>();
        public class LoadedScene: UnityEvent {}
        public GenericEvent<LoadedScene> OnLoadedScene = new GenericEvent<LoadedScene>();
        public class ChangeCurrentSelectedUI: UnityEvent<GameObject> {}
        public GenericEvent<ChangeCurrentSelectedUI> OnChangeCurrentSelectedUI = new GenericEvent<ChangeCurrentSelectedUI>();
        public class ForcedToCrouch: UnityEvent<bool> {}
        public GenericEvent<ForcedToCrouch> OnForcedToCrouch = new GenericEvent<ForcedToCrouch>();
    }

    public class LevelEvents{
        public class LevelStart: UnityEvent {}
        public GenericEvent<LevelStart> OnLevelStart = new GenericEvent<LevelStart>();
        public class LevelEnd: UnityEvent {}
        public GenericEvent<LevelEnd> OnLevelEnd = new GenericEvent<LevelEnd>();
        public class CountDownEnd: UnityEvent {}
        public GenericEvent<CountDownEnd> OnCountDownEnd = new GenericEvent<CountDownEnd>();
        public class CountDownCycle: UnityEvent {}
        public GenericEvent<CountDownCycle> OnCountDownCycle = new GenericEvent<CountDownCycle>();
    }
}
