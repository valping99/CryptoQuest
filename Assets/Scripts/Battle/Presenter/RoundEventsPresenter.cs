﻿using System.Collections;
using System.Collections.Generic;
using CryptoQuest.Battle.Events;
using CryptoQuest.Battle.Presenter.Commands;
using CryptoQuest.Input;
using TinyMessenger;
using UnityEngine;

namespace CryptoQuest.Battle.Presenter
{
    /// <summary>
    /// Presenting actions and events in a round.
    /// </summary>
    public class RoundEventsPresenter : MonoBehaviour
    {
        [SerializeField] private BattleInput _input;
        private readonly Queue<IPresentCommand> _eventCommands = new();
        private TinyMessageSubscriptionToken _startPresentingEvent;
        private TinyMessageSubscriptionToken _enqueueCommandEvent;
        private TinyMessageSubscriptionToken _roundStartedEvent;

        private bool _presented;
        private Coroutine _presentingCoroutine;

        private void Awake()
        {
            _startPresentingEvent = BattleEventBus.SubscribeEvent<StartPresentingEvent>(StartPresenting);
            _enqueueCommandEvent = BattleEventBus.SubscribeEvent<EnqueuePresentCommandEvent>(EnqueueCommand);
            _roundStartedEvent = BattleEventBus.SubscribeEvent<RoundStartedEvent>(ClearAndAcceptCommand);
        }

        private void OnDestroy()
        {
            BattleEventBus.UnsubscribeEvent(_startPresentingEvent);
            BattleEventBus.UnsubscribeEvent(_enqueueCommandEvent);
            BattleEventBus.UnsubscribeEvent(_roundStartedEvent);
            _input.ConfirmedEvent -= InputOnConfirmedEvent;
        }

        public void EnqueueCommand(IPresentCommand command) => _eventCommands.Enqueue(command);

        private void ClearAndAcceptCommand(RoundStartedEvent ctx)
        {
            // To prevent queue command before the round started
            // anything want to be presented in round should be queue after RoundStartedEvent
            // by listening to StartAcceptCommand
            _eventCommands.Clear();
            BattleEventBus.RaiseEvent(new StartAcceptCommand()
            {
                RoundStartedContext = ctx
            });
        }

        private void EnqueueCommand(EnqueuePresentCommandEvent eventObject)
        {
            EnqueueCommand(eventObject.PresentCommand);
        }

        private void StartPresenting(StartPresentingEvent startPresentingEvent)
        {
            _input.ConfirmedEvent += InputOnConfirmedEvent;
            StartCoroutine(CoPresenting());
        }

        private void InputOnConfirmedEvent()
        {
            if (_presented) return;
            StopCoroutine(_presentingCoroutine);
            _presented = true;
        }

        private IEnumerator CoPresenting()
        {
            while (_eventCommands.Count > 0)
            {
                var command = _eventCommands.Dequeue();
                _presented = false;
                _presentingCoroutine = StartCoroutine(CoInternalPresent(command));
                yield return new WaitUntil(() => _presented);
            }

            _input.ConfirmedEvent -= InputOnConfirmedEvent;
            
            BattleEventBus.RaiseEvent(new FinishedPresentingActionsEvent());
        }

        private IEnumerator CoInternalPresent(IPresentCommand command)
        {
            yield return command.Present();
            _presented = true;
        }
    }
}