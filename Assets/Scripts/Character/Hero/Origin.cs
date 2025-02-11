﻿using System;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Localization;

namespace CryptoQuest.Character.Hero
{
    /// <summary>
    /// Where this character come from?
    /// name
    /// age, height, weight
    /// </summary>
    public class Origin : ScriptableObject
    {
        [Serializable]
        public struct Information
        {
            public int Id;
            public LocalizedString LocalizedName;
            public Gender Gender;
            public LocalizedString LocalizedBirthPlace;
            public float Height;
            public float Weight;
            public LocalizedString LocalizedDescription;
        }

        [field: SerializeField] public Information DetailInformation { get; set; }
    }
}