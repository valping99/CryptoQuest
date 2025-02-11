﻿using System;
using IndiGames.GameplayAbilitySystem.AbilitySystem;
using IndiGames.GameplayAbilitySystem.EffectSystem;
using IndiGames.GameplayAbilitySystem.EffectSystem.ScriptableObjects;
using UnityEngine;

namespace CryptoQuest.AbilitySystem.Abilities.PostNormalAttackPassive
{
    [CreateAssetMenu(menuName = "Crypto Quest/Ability System/Passive/Condition Skill/Target Both",
        fileName = "ConditionSkill")]
    public class AbsorbPassiveAbility : PostNormalAttackPassiveBase
    {
        [field: SerializeField] public GameplayEffectDefinition Effect { get; private set; }

        [Header("Absorb Info")]
        [SerializeField] private GameplayEffectContext _absorbContext;

        public GameplayEffectContext AbsorbContext => _absorbContext;
        [field: SerializeField] public GameplayEffectDefinition AbsorbEffect { get; private set; }

        protected override GameplayAbilitySpec CreateAbility()
            => new AbsorbPassiveSpec(this);
#if UNITY_EDITOR
        public override PostNormalAttackPassiveBase CreateInstance()
        {
            return (AbsorbPassiveAbility)Activator.CreateInstance(this.GetType());
        }
#endif
    }

    public class AbsorbPassiveSpec : PostNormalAttackPassiveSpecBase
    {
        private ActiveGameplayEffect _damageEffect;
        private ActiveGameplayEffect _absorbEffect;
        private AbsorbPassiveAbility _ability;

        public AbsorbPassiveSpec(AbsorbPassiveAbility ability)
        {
            _ability = ability;
        }

        protected override void OnAttacked(DamageContext postAttackContext)
        {
            var target = postAttackContext.Target;
            if (!IsTargetValid(target)) return;
            if (!IsTargetValid(Character)) return;
            if (FailedToActive(_ability)) return;

            _damageEffect = ApplyEffect(SkillContext, _ability.Effect, target);

            _absorbEffect = ApplyEffect(_ability.AbsorbContext, _ability.AbsorbEffect, Character);
        }

        private ActiveGameplayEffect ApplyEffect(GameplayEffectContext context, GameplayEffectDefinition effect,
            Battle.Components.Character target)
        {
            var gameplayEffectContextHandle = new GameplayEffectContextHandle(context);
            var targetSelfSpec = effect.CreateEffectSpec(Owner, gameplayEffectContextHandle);
            return target.AbilitySystem.ApplyEffectSpecToSelf(targetSelfSpec);
        }
    }
}