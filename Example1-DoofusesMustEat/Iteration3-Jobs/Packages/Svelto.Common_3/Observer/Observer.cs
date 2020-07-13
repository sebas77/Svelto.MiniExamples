using System;

namespace Svelto.Observer.InterNamespace
{
    /// <summary>
    /// Example:
    /// namespace Svelto.ECS.Example.Survive.Observers.HUD
    ///    {
    ///        public class ScoreOnEnemyKilledObserver:Observer<PlayerTargetType, ScoreActions>
    ///        {
    ///            public ScoreOnEnemyKilledObserver(EnemyKilledObservable observable): base(observable)
    ///            {}
    ///    
    ///            protected override ScoreActions TypeMap(ref PlayerTargetType dispatchNotification)
    ///            {
    ///                return _targetTypeToScoreAction[dispatchNotification];
    ///            }
    ///    
    ///            readonly Dictionary<PlayerTargetType, ScoreActions> _targetTypeToScoreAction = new Dictionary<PlayerTargetType, ScoreActions>
    ///            {
    ///                { PlayerTargetType.Bear, ScoreActions.bearKilled },
    ///                { PlayerTargetType.Bunny, ScoreActions.bunnyKilled },
    ///                { PlayerTargetType.Hellephant, ScoreActions.HellephantKilled },
    ///            };
    ///        }
    ///    
    ///        public enum ScoreActions
    ///        {
    ///            bunnyKilled,
    ///            bearKilled,
    ///            HellephantKilled
    ///        }
    ///    }
    ///  This is the only observer that makes still sense to be used with Svelto.ECS. This observer is
    ///  meant to put in communication totally indipendent systems. The mapping of the enums would allow
    ///  even names to be totally indipendent 
    /// 
    /// </summary>
    /// <typeparam name="DispatchType"></typeparam>
    /// <typeparam name="ActionType"></typeparam>
    public abstract class Observer<DispatchType, ActionType> : IObserver<ActionType>
    {
        protected Observer(Observable<DispatchType> observable)
        {
            observable.Notify += OnObservableDispatched;

            _unsubscribe = () => observable.Notify -= OnObservableDispatched;
        }

        public void AddAction(ObserverAction<ActionType> action)
        {
            _actions += action;
        }

        public void RemoveAction(ObserverAction<ActionType> action)
        {
            _actions -= action;
        }

        public void Unsubscribe()
        {
            _unsubscribe();
        }

        void OnObservableDispatched(ref DispatchType dispatchNotification)
        {
            if (_actions != null)
            {
                var actionType = TypeMap(ref dispatchNotification);

                _actions(ref actionType);
            }
        }

        protected abstract ActionType TypeMap(ref DispatchType dispatchNotification);

        ObserverAction<ActionType> _actions;
        readonly Action _unsubscribe;
    }
}

namespace Svelto.Observer
{
    public interface IObserver<WatchingType>
    {
        void AddAction(ObserverAction<WatchingType> action);
        void RemoveAction(ObserverAction<WatchingType> action);

        void Unsubscribe();
    }

    public interface IObserver
    {
        void AddAction(Action action);
        void RemoveAction(Action action);

        void Unsubscribe();
    }
}