﻿using CryptoQuest.AbilitySystem.Abilities;
using CryptoQuest.Battle.Components;
using CryptoQuest.Gameplay.PlayerParty;
using CryptoQuest.System;
using IndiGames.Core.Common;
using UnityEngine;

namespace CryptoQuest.Menus.Skill.UI
{
    public class UsingSkillPresenter : MonoBehaviour
    {
        [SerializeField] private SkillTargetType _targetSelfChannel;
        [SerializeField] private SkillTargetType _targetOneAllyChannel;
        [SerializeField] private SkillTargetType _targetAllAllyChannel;
        [SerializeField] private UITargetSingleCharacter _uiTargetCharacter;
        [SerializeField] private UICharacterSelection _characterSelection;

        private IPartyController _partyController;
        private HeroBehaviour _inspectingHero;

        private void Awake()
        {
            UISkill.InspectingSkillEvent += SaveLastInspecting;
        }

        private void OnEnable()
        {
            _targetOneAllyChannel.EventRaised += ShowSelectSingleHeroUI;
            _targetAllAllyChannel.EventRaised += SelectAllAliveHeroes;
            _targetSelfChannel.EventRaised += SelectSelf;
            _uiTargetCharacter.SelectedCharacterEvent += UseSkillOnCharacter;

            _partyController ??= ServiceProvider.GetService<IPartyController>();
        }

        private void OnDisable()
        {
            _targetOneAllyChannel.EventRaised -= ShowSelectSingleHeroUI;
            _targetAllAllyChannel.EventRaised -= SelectAllAliveHeroes;
            _targetSelfChannel.EventRaised -= SelectSelf;
            _uiTargetCharacter.SelectedCharacterEvent -= UseSkillOnCharacter;
        }

        private void OnDestroy()
        {
            UISkill.InspectingSkillEvent -= SaveLastInspecting;
        }

        private CastSkillAbility _inspectingSkill;

        private void SetInspectingCharacter(HeroBehaviour hero)
        {
            _inspectingHero = hero;
        }

        private void SaveLastInspecting(UISkill skill)
        {
            _inspectingSkill = skill.Skill;
        }

        private void ShowSelectSingleHeroUI(CastSkillAbility skill)
        {
        }

        private void SelectAllAliveHeroes(CastSkillAbility skill)
        {
            var party = ServiceProvider.GetService<IPartyController>();
            foreach (var hero in _partyController.OrderedAliveMembers)
            {
                if (!hero.IsValid()) continue;
                UseSkillOnCharacter(hero);
            }
        }

        private void SelectSelf(CastSkillAbility skill)
        {
            if (_inspectingHero == null || !_inspectingHero.IsValid()) return;

            UseSkillOnCharacter(_inspectingHero);
        }

        private void UseSkillOnCharacter(HeroBehaviour hero)
        {
            var target = hero.AbilitySystem;
            var spec = _inspectingHero.AbilitySystem.GiveAbility<CastSkillAbilitySpec>(_inspectingSkill);
            spec.Execute(target);
        }
    }
}