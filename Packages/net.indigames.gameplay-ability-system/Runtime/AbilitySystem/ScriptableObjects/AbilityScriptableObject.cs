using System;
using IndiGames.GameplayAbilitySystem.AbilitySystem.Components;
using IndiGames.GameplayAbilitySystem.TagSystem.ScriptableObjects;
using UnityEngine;

namespace IndiGames.GameplayAbilitySystem.AbilitySystem.ScriptableObjects
{
    public abstract class AbilityScriptableObject : ScriptableObject
    {
        [SerializeField] private AbilityTags tags = new();
        public AbilityTags Tags => tags;

        public GameplayAbilitySpec GetAbilitySpec(AbilitySystemBehaviour owner)
        {
            var ability = CreateAbility();
            ability.InitAbility(owner, this);
            return ability;
        }

        protected abstract GameplayAbilitySpec CreateAbility();
    }

    /// <summary>
    /// Override this to create new ability SO with a new abstract ability
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbilityScriptableObject<T> : AbilityScriptableObject where T : GameplayAbilitySpec, new()
    {
        protected override GameplayAbilitySpec CreateAbility() => new T();
    }

    [Serializable]
    public class AbilityTags
    {
        /// <summary>
        /// Tag to define this ability should be unique
        /// </summary>
        public TagScriptableObject AbilityTag;

        /// <summary>
        /// Not implemented
        /// Active the ability on the same system will cancel any ability that have these tags
        /// </summary>
        public TagScriptableObject[] CancelAbilityWithTags = Array.Empty<TagScriptableObject>();

        /// <summary>
        /// Not implemented
        /// Prevents execution of any other Abilities with a matching Tag while this Ability is executing.
        /// Ability that have these tags will be blocked from activating on the same system
        /// e.g. silencing ability that enemy could use to prevent use to use any ability
        /// </summary>
        public TagScriptableObject[] BlockAbilityWithTags = Array.Empty<TagScriptableObject>();

        /// <summary>
        /// These tags will be granted to the source system while this ability is active
        /// </summary>
        public TagScriptableObject[] ActivationTags = Array.Empty<TagScriptableObject>();

        /// <summary>
        /// This ability can only active if owner system has all of the RequiredTags
        /// and none of the Ignore tags
        /// </summary>
        public TagRequireIgnoreDetails OwnerTags = new();

        /// <summary>
        /// Not implemented
        /// This ability can only active if the Source system has all the required tags
        /// and none of the Ignore tags
        /// </summary>
        public TagRequireIgnoreDetails SourceTags = new();

        /// <summary>
        /// Not implemented
        /// This ability can only active if the Target system has all the required tags
        /// and none of the Ignore tags
        /// </summary>
        public TagRequireIgnoreDetails TargetTags = new();
    }
}