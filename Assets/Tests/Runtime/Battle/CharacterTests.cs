﻿using CryptoQuest.Tests.Runtime.Battle.Builder;
using NUnit.Framework;
using UnityEngine;

namespace CryptoQuest.Tests.Runtime.Battle
{
    public class CharacterTests : BattleFixtureBase
    {
        private GameObject _characterGameObject;
        private CryptoQuest.Battle.Components.Character _character;

        [SetUp]
        public void Setup()
        {
            _character = A.Character.Build();
        }

        [Test, Category("Smokes")]
        public void Create_ShouldHaveAbilitySystem()
        {
            Assert.NotNull(_character.AbilitySystem);
        }
    }
}