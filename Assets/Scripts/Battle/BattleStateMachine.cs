using CryptoQuest.Battle.States;
using CryptoQuest.Gameplay;
using CryptoQuest.Input;
using IndiGames.Core.Events.ScriptableObjects;
using UnityEngine;

namespace CryptoQuest.Battle
{
    public interface IState
    {
        void OnEnter(BattleStateMachine stateMachine);
        void OnExit(BattleStateMachine stateMachine);
    }

    public class BattleStateMachine : MonoBehaviour
    {
        [SerializeField] private ResultSO _result;
        [SerializeField] private GameStateSO _gameState;
        [field: SerializeField] public BattleInput BattleInput { get; private set; }

        #region State Context

        [field: SerializeField] public BattlePresenter BattlePresenter { get; private set; }
        [field: SerializeField] public VoidEventChannelSO SceneLoadedEvent { get; private set; }

        #endregion

        #region State management
        [field: SerializeField] public ExecuteCharactersActions HandleActions { get; private set; }
        [field: SerializeField] public ResultChecker ResultChecker { get; private set; }

        private IState _currentState;

        private void Awake()
        {
            SceneLoadedEvent.EventRaised += GotoLoadingState;
        }

        /// <summary>
        /// I use OnDisable to prevent error in editor
        /// because when state Exit it only disabling GO and those GO already destroyed 
        /// </summary>
        private void OnDisable() => Unload();

        private void Unload()
        {
            SceneLoadedEvent.EventRaised -= GotoLoadingState;
            _currentState?.OnExit(this);
            _currentState = null;
        }

        private void GotoLoadingState()
        {
            _result.State = ResultSO.EState.None;
            _gameState.UpdateGameState(EGameState.Battle);
            ChangeState(new Loading());
        }

        public void ChangeState(IState state)
        {
            Debug.Log($"BattleStateMachine: ChangeState {state.GetType().Name}");
            _currentState?.OnExit(this);
            _currentState = state;
            _currentState.OnEnter(this);
        }

        public bool TryGetPresenterComponent<T>(out T component) where T : Component
            => BattlePresenter.TryGetComponent(out component);

        #endregion
    }
}