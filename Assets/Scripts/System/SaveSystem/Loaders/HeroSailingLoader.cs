﻿using UnityEngine;
using CryptoQuest.Events;
using CryptoQuest.Gameplay.Ship;
using IndiGames.Core.Events;
using CryptoQuest.Character;

namespace CryptoQuest.System.SaveSystem.Loaders
{
    public class HeroSailingLoader : SagaBase<SpawnShipAction>
    {
        private static bool _isLoaded;
        [SerializeField] private PositionEventChannelSO _spawnShipAtPositionEvent;
        [SerializeField] private ShipBus _shipBus;

        private void Start()
        {
            // Because editor need to reload domain so this can be called multiple times
#if !UNITY_EDITOR
            if (_isLoaded) return;
            _isLoaded = true;
#endif
            if (_shipBus.CurrentSailState != ESailState.Sailing) return;
            _spawnShipAtPositionEvent.RaiseEvent(transform.position);
        }

        protected override void HandleAction(SpawnShipAction ctx)
        {
            if (_shipBus.CurrentSailState != ESailState.Sailing) return;
            var shipGo = ctx.JustSpawnedShip.GameObject;
            var interactComponent = shipGo.GetComponent<IInteractableOnTouch>();
            interactComponent.Interact(gameObject);
        }
    }
}