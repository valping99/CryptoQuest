﻿using System;
using System.Collections.Generic;
using System.Net;
using CryptoQuest.API;
using CryptoQuest.Networking;
using CryptoQuest.Sagas.Objects;
using CryptoQuest.UI.Actions;
using IndiGames.Core.Common;
using IndiGames.Core.Events;
using UniRx;
using UnityEngine;

namespace CryptoQuest.Sagas.Equipment
{
    public class DetachBody
    {
        public string id;
        public List<int> stones;
    }

    public class DetachMagicStones : SagaBase<DetachStones>
    {
        private int _equipmentID;
        private List<int> _stoneIDs;

        protected override void HandleAction(DetachStones ctx)
        {
            ActionDispatcher.Dispatch(new ShowLoading());
            Debug.Log($"<color=white>Detach id={ctx.EquipmentID} -- stones={ctx.StoneIDs[0]}</color>");
            var restClient = ServiceProvider.GetService<IRestClient>();
            restClient
                .WithBody(new DetachBody()
                {
                    id = ctx.EquipmentID.ToString(),
                    stones = ctx.StoneIDs
                })
                .Post<EquipmentsResponse>(EquipmentAPI.DETACH_MAGIC_STONE)
                .Subscribe(ProcessResponse, OnError, OnCompleted);

            _equipmentID = ctx.EquipmentID;
            _stoneIDs = ctx.StoneIDs;
        }

        private void ProcessResponse(EquipmentsResponse response)
        {
            if (response.code != (int)HttpStatusCode.OK) return;
            ActionDispatcher.Dispatch(new DetachSucceeded()
            {
                EquipmentID = _equipmentID,
                StoneIDs = _stoneIDs
            });
            foreach (var equipmentResponse in response.data.equipments)
            {
                if (_equipmentID != equipmentResponse.id || equipmentResponse.attachId == 0) continue;
                ActionDispatcher.Dispatch(new RemoveStonePassiveRequest()
                {
                    EquipmentID = _equipmentID,
                    StoneIDs = _stoneIDs,
                    CharacterID = equipmentResponse.attachId
                });
            }
        }

        private void OnError(Exception error)
        {
            Debug.Log($"<color=white>Saga::DetachMagicStones::Error</color>:: {error}");
        }

        private void OnCompleted()
        {
            ActionDispatcher.Dispatch(new ShowLoading(false));
        }
    }
}