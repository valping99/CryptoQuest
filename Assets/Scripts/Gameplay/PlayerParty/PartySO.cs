﻿using System;
using System.Collections;
using System.Collections.Generic;
using CryptoQuest.Character.Hero;
using UnityEngine;

namespace CryptoQuest.Gameplay.PlayerParty
{
    /// <summary>
    /// Wrapper of hero spec in a party, only in party heroes able to equip items
    /// </summary>
    [Serializable]
    public class PartySlotSpec
    {
        [field: SerializeField] public HeroSpec Hero { get; set; } = new();

        [field: SerializeField] public Equipments EquippingItems { get; set; } = new();

        public bool IsValid() => Hero != null && Hero.IsValid();
    }

    public class PartySO : ScriptableObject, IEnumerable<PartySlotSpec>, IPartyProvider
    {
        [SerializeField] private PartySlotSpec[] _heroSpecs;
        public PartySlotSpec[] GetParty() => _heroSpecs;
        public int Count => _heroSpecs.Length;

        public void SetParty(PartySlotSpec[] newSpecs)
        {
            _heroSpecs = newSpecs;
            Changed?.Invoke();
        }

        public event Action Changed;

        public PartySlotSpec this[int i]
        {
            get => _heroSpecs[i];
            set
            {
                _heroSpecs[i] = value;
                Changed?.Invoke();
            }
        }

        public IEnumerator<PartySlotSpec> GetEnumerator() => ((IEnumerable<PartySlotSpec>)_heroSpecs).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Exists(HeroSpec item)
        {
            foreach (var heroSpec in _heroSpecs)
            {
                if (heroSpec.Hero.Id == item.Id) return true;
            }

            return false;
        }
    }
}