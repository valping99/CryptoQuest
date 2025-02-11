﻿using System;
using System.Collections.Generic;
using System.Linq;
using CryptoQuest.Gameplay.Loot;
using CryptoQuest.Gameplay.Reward.Events;
using CryptoQuest.Quest.Authoring;
using CryptoQuest.Quest.Events;
using CryptoQuest.Quest.Sagas;
using IndiGames.Core.Common;
using IndiGames.Core.Events;
using UnityEngine;

namespace CryptoQuest.Quest.Components
{
    [AddComponentMenu("Quest System/Quest Manager")]
    [DisallowMultipleComponent]
    public class QuestManager : IQuestManager
    {
        [Header("Quest Events"), SerializeField]
        private QuestEventChannelSO _triggerQuestEventChannel;

        [SerializeField] private QuestEventChannelSO _giveQuestEventChannel;
        [SerializeField] private QuestEventChannelSO _removeQuestEventChannel;
        [SerializeField] private RewardLootEvent _rewardEventChannel;

        [Header("Quest Save Data"), SerializeField]
        private QuestSaveSO _saveData;

        public QuestSaveSO SaveData => _saveData;

        [Header("Quests"), SerializeField, HideInInspector]
        private QuestSO _currentQuestData;

        [SerializeReference, HideInInspector] private List<QuestInfo> _currentQuestInfos = new();
        [NonSerialized] private Dictionary<string, QuestInfo> _questInfoDict = new();

        [SerializeField] private QuestCompletionHandler _questCompletionHandler;

        private bool _isCacheDirty;
        private IQuestConfigure _questConfigure;

        private Dictionary<string, QuestInfo> QuestInfoLookup
        {
            get
            {
                if (!_isCacheDirty) return _questInfoDict;
                _isCacheDirty = false;
                _questInfoDict.Clear();
                foreach (var data in _currentQuestInfos)
                {
                    _questInfoDict.Add(data.Guid, data);
                }

                return _questInfoDict;
            }
        }

        private void Awake()
        {
            ServiceProvider.Provide<IQuestManager>(this);
        }

        private void MarkCacheDirty()
        {
            _isCacheDirty = true;
        }

        protected void OnEnable()
        {
            OnConfigureQuest += ConfigureQuestHolder;
            OnRemoveProgressingQuest += RemoveProgressingQuest;
            OnQuestCompleted += QuestCompleted;

            _triggerQuestEventChannel.EventRaised += TriggerQuest;
            _giveQuestEventChannel.EventRaised += GiveQuest;
            _removeQuestEventChannel.EventRaised += RemoveProgressingQuest;
        }

        protected void OnDisable()
        {
            OnConfigureQuest -= ConfigureQuestHolder;
            OnRemoveProgressingQuest -= RemoveProgressingQuest;
            OnQuestCompleted -= QuestCompleted;

            _triggerQuestEventChannel.EventRaised -= TriggerQuest;
            _giveQuestEventChannel.EventRaised -= GiveQuest;
            _removeQuestEventChannel.EventRaised -= RemoveProgressingQuest;
        }

        private void OnDestroy()
        {
            foreach (var info in _currentQuestInfos) info.Release();
        }

        public override void TriggerQuest(QuestSO questData)
        {
            if (IsQuestTriggered(questData))
            {
                Debug.Log($"<color=green>QuestManager::TriggerQuest::Already triggered: {questData.QuestName}</color>");
                return;
            }

            if (QuestInfoLookup.TryGetValue(questData.Guid, out var questInfo))
            {
                Debug.Log($"<color=green>QuestManager::TriggerQuest::Triggered: {questData.QuestName}</color>");
                questInfo.TriggerQuest();
                questInfo.Release();
                _currentQuestInfos.Remove(questInfo);
                MarkCacheDirty();
            }
            else
            {
                Debug.Log(
                    $"<color=red>QuestManager::TriggerQuest::Triggered:QuestInfoIsNotCurrentQuest {questData.QuestName}</color>");
            }
        }

        private bool IsQuestCompleted(QuestSO questData)
        {
            if (questData != null && _saveData.CompletedQuests.Count() > 0)
            {
                return _saveData.CompletedQuests.Any(questInfo => questData.Guid == questInfo);
            }

            return false;
        }

        public override void GiveQuest(QuestSO questData)
        {
            if (IsQuestTriggered(questData))
            {
                Debug.Log($"<color=green>QuestManager::GiveQuest::Already triggered: {questData.QuestName}</color>");
                return;
            }


            if (_saveData.InProgressQuest.Any(questInfo => questInfo == questData.Guid))
            {
                Debug.Log($"<color=green>QuestManager::GiveQuest::Already inprogress: {questData.QuestName}</color>");
                if (_currentQuestInfos.Any(questInfo => questInfo.Guid == questData.Guid)) return;

                var info = questData.CreateQuest();
                _currentQuestInfos.Add(info);
                MarkCacheDirty();
                info.GiveQuest();

                return;
            }

            if (!IsQuestCompleted(questData))
            {
                Debug.Log($"<color=green>QuestManager::GiveQuest::Give: {questData.QuestName}</color>");
                var info = questData.CreateQuest();
                _currentQuestInfos.Add(info);
                MarkCacheDirty();
                info.GiveQuest();
                _saveData.AddInProgressQuest(info.Guid);
            }
            else
            {
                Debug.Log($"<color=green>QuestManager::GiveQuest::Already completed: {questData.QuestName}</color>");
            }

            _currentQuestData = questData;
            questData.OnRewardReceived += RewardReceived;
        }

        private void RewardReceived(List<LootInfo> loots)
        {
            _rewardEventChannel.RaiseEvent(loots);
            _currentQuestData.OnRewardReceived -= RewardReceived;
        }

        private void UpdateQuestProgress(string questInfo)
        {
            _saveData.AddCompleteQuest(questInfo);
        }

        private void QuestCompleted(QuestSO data)
        {
            foreach (var progressQuestInfo in _saveData.InProgressQuest)
            {
                if (progressQuestInfo != data.Guid) continue;
                UpdateQuestProgress(progressQuestInfo);
                break;
            }

            _questCompletionHandler.GrantCompletionRewards(data);
            _questCompletionHandler.HandleNextAction(data);
            ActionDispatcher.Dispatch(new QuestTriggeredAction(data));
            ActionDispatcher.Dispatch(new QuestFinishRequestAction(data.QuestName));
        }

        private bool IsQuestTriggered(QuestSO questSo)
        {
            return _saveData.CompletedQuests.Any(quest => quest == questSo.Guid);
        }

        private void ConfigureQuestHolder(IQuestConfigure questConfigure)
        {
            _questConfigure = questConfigure;
            foreach (var questSo in questConfigure.QuestsToTrack)
            {
                var matchingQuestCompleted = _saveData.CompletedQuests.Where(quest => quest == questSo.Guid).ToList();
                var questCompleted = matchingQuestCompleted.FirstOrDefault();

                questConfigure.Configure(IsQuestTriggered(questSo), questCompleted);
            }
        }

        private void RemoveProgressingQuest(QuestSO quest)
        {
            Debug.Log($"<color=green>QuestManager::RemoveProgressingQuest:: {quest.QuestName}</color>");

            foreach (var questInfo in _currentQuestInfos.ToList())
            {
                if (questInfo.Guid != quest.Guid) continue;
                _currentQuestInfos.Remove(questInfo);
            }

            MarkCacheDirty();

            foreach (var inProgressQuest in _saveData.InProgressQuest.ToList())
            {
                if (inProgressQuest != quest.Guid) continue;
                _saveData.RemoveInProgressQuest(inProgressQuest);
            }
        }
    }
}