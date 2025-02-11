﻿using CryptoQuest.AbilitySystem.Abilities;
using CryptoQuest.AbilitySystem.Attributes;
using CryptoQuest.Menus.Beast.UI;
using CryptoQuest.Tests.Editor.Beast.Builder;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace CryptoQuest.Tests.Editor.Beast
{
    [TestFixture]
    public class UIBeastDetailsTests
    {
        private UIMenuBeastDetail _uiMenuBeastDetails;

        [SetUp]
        public void Setup()
        {
            var panelPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Menu/Beast/BeastPanel.prefab");
            var panel = Object.Instantiate(panelPrefab);
            _uiMenuBeastDetails = panel.GetComponentInChildren<UIMenuBeastDetail>();
        }

        [Test]
        public void FillUI_WithDarkBeast_CorrectDarkElementSprite()
        {
            var darkElement =
                AssetDatabase.LoadAssetAtPath<Elemental>(
                    "Assets/ScriptableObjects/Character/Attributes/Elemental/Dark/Dark.asset");

            _uiMenuBeastDetails.FillUI(A.Beast.WithElement(darkElement).Build());

            var panelSO = new SerializedObject(_uiMenuBeastDetails);
            var beastElementImage = panelSO.FindProperty("_beastElement").objectReferenceValue as Image;

            Assert.AreEqual(darkElement.Icon, beastElementImage.sprite);
        }

        [Test]
        public void FillUI_WithNullPassive_ShouldShowsEmptyPassive()
        {
            _uiMenuBeastDetails.FillUI(A.Beast.Build());

            var panelSO = new SerializedObject(_uiMenuBeastDetails);
            var beastPassiveSkill =
                panelSO.FindProperty("_beastPassiveSkill").objectReferenceValue as LocalizeStringEvent;

            Assert.AreEqual(new LocalizedString(), beastPassiveSkill.StringReference);
        }

        [Test]
        public void FillUI_WithPassive_ShouldShowsPassive()
        {
            var passive =
                AssetDatabase.LoadAssetAtPath<PassiveAbility>(
                    "Assets/ScriptableObjects/Character/Skills/Conditionals/3001.asset");
            _uiMenuBeastDetails.FillUI(A.Beast.WithPassive(passive).Build());

            var panelSO = new SerializedObject(_uiMenuBeastDetails);
            var beastPassiveSkill =
                panelSO.FindProperty("_beastPassiveSkill").objectReferenceValue as LocalizeStringEvent;

            Assert.AreEqual(passive.Description, beastPassiveSkill.StringReference);
        }

        [Test]
        public void FillUI_WithAnotherBeast_ShouldUpdateBeastUI()
        {
            var passive =
                AssetDatabase.LoadAssetAtPath<PassiveAbility>(
                    "Assets/ScriptableObjects/Character/Skills/Conditionals/3001.asset");
            _uiMenuBeastDetails.FillUI(A.Beast.WithPassive(passive).Build());

            var passiveName = _uiMenuBeastDetails.transform.Find("Detail/Panel/Passives/Panel/Text (Legacy)")
                .GetComponent<LocalizeStringEvent>();
            Assert.AreEqual(passive.Description, passiveName.StringReference);

            passive = AssetDatabase.LoadAssetAtPath<PassiveAbility>(
                "Assets/ScriptableObjects/Character/Skills/Conditionals/3005.asset");
            _uiMenuBeastDetails.FillUI(A.Beast.WithPassive(passive).Build());

            Assert.AreEqual(passive.Description, passiveName.StringReference);
        }
    }
}