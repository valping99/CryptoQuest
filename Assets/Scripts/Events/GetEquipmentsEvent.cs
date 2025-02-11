﻿using System.Collections.Generic;
using CryptoQuest.Sagas.Objects;
using IndiGames.Core.Events.ScriptableObjects;

namespace CryptoQuest.Events
{
    public class GetEquipmentsEvent : GenericEventChannelSO<List<EquipmentResponse>> { }
}