﻿using CryptoQuest.Gameplay.BaseGameplayData;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CryptoQuest.Character
{
    public class CharacterClass : GenericData
    {
        [field: SerializeField] public AssetReferenceT<Sprite> Icon { get; private set; }
    }
}