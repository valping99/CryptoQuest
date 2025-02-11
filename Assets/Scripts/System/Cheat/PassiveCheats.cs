﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommandTerminal;
using CryptoQuest.AbilitySystem.Abilities;
using CryptoQuest.Battle.Components;
using IndiGames.GameplayAbilitySystem.AbilitySystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CryptoQuest.System.Cheat
{
    public class PassiveCheats : MonoBehaviour, ICheatInitializer
    {
        [Serializable]
        private struct Passives
        {
            public int PassiveId;
            public AssetReferenceT<PassiveAbility> Passive;
        }

        [SerializeField] private List<Passives> _passives;
        private readonly Dictionary<int, AssetReferenceT<PassiveAbility>> _passiveDict = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_passives.Count != 0) return;
            var passivesGuids = AssetDatabase.FindAssets("t:PassiveAbility");
            foreach (var guid in passivesGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var passive = AssetDatabase.LoadAssetAtPath<PassiveAbility>(path);
                if (passive == null) continue;
                _passives.Add(new Passives
                {
                    PassiveId = passive.Id == 0 ? passive.Id : int.Parse(passive.name),
                    Passive = new AssetReferenceT<PassiveAbility>(guid)
                });
            }
        }
#endif

        private void Awake()
        {
            foreach (var passive in _passives)
            {
                _passiveDict.TryAdd(passive.PassiveId, passive.Passive);
            }
        }

        public void InitCheats()
        {
            Terminal.Shell.AddCommand("add.passive", AddPassiveToCharacter, 2, 2,
                "add.passive <passive_id> <character_id>, add passive with id to character id from get.characters");
            Terminal.Shell.AddCommand("remove.passive", RemovePassiveFromCharacter, 2, 2,
                "remove.passive <passive_id> <character_id>, remove passive with id from character id from get.characters");
        }

        private readonly Dictionary<int, List<PassiveAbilitySpec>> _passiveSpecDict = new();

        private void AddPassiveToCharacter(CommandArg[] args)
        {
            var passiveIds = ExtractPassiveIds(args);
            var characterId = args[1].Int;
            foreach (var passiveId in passiveIds)
                StartCoroutine(LoadThenAddPassiveToCharacterCo(characterId, passiveId));
        }

        private static int[] ExtractPassiveIds(CommandArg[] args)
        {
            var strPassiveIds = args[0].String.Split(',');
            var passiveIds = new int[strPassiveIds.Length];
            for (var index = 0; index < strPassiveIds.Length; index++)
            {
                var strId = strPassiveIds[index];
                passiveIds[index] = int.Parse(strId);
            }

            return passiveIds;
        }

        private IEnumerator LoadThenAddPassiveToCharacterCo(int characterId, int passiveId)
        {
            var character = CharacterCheats.Instance.GetCharacter(characterId);
            if (character == null)
            {
                Debug.LogWarning($"Cannot find character with instance id [{characterId}].");
                yield break;
            }

            if (!_passiveDict.TryGetValue(passiveId, out var passiveAssetRef))
            {
                Debug.LogWarning($"Doesn't support passive [{passiveId}].");
                yield break;
            }

            var handle =
                !passiveAssetRef.OperationHandle.IsValid()
                    ? passiveAssetRef.LoadAssetAsync()
                    : passiveAssetRef.OperationHandle;
            yield return handle;
            var passiveAbility = (PassiveAbility)handle.Result;
            PassivesController passivesController = character.GetComponent<PassivesController>();

            _passiveSpecDict.TryAdd(characterId, new List<PassiveAbilitySpec>());
            _passiveSpecDict[characterId].Add(passivesController.ApplyPassive(passiveAbility));
            Debug.Log($"Add passive [{passiveId}] to character [{character.DisplayName}] with id [{characterId}].");
        }

        private void RemovePassiveFromCharacter(CommandArg[] args)
        {
            var passiveId = args[0].Int;
            var characterId = args[1].Int;
            RemovePassiveFromCharacter(characterId, passiveId);
        }

        private void RemovePassiveFromCharacter(int characterId, int passiveId)
        {
            if (_passiveSpecDict.TryGetValue(characterId, out var characterPassiveSpecs) == false) return;

            PassiveAbilitySpec spec =
                characterPassiveSpecs.FirstOrDefault(passiveSpec => passiveSpec.SkillContext.SkillInfo.Id == passiveId);

            if (spec == null) return;

            var character = CharacterCheats.Instance.GetCharacter(characterId);
            PassivesController passivesController = character.GetComponent<PassivesController>();
            passivesController.RemovePassive(spec);
            characterPassiveSpecs.Remove(spec);

            Debug.Log(
                $"remove passive [{passiveId}] from character [{character.DisplayName}] with id [{characterId}].");
        }
    }
}