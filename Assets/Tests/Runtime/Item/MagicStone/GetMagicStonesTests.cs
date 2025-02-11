﻿using System.Collections;
using System.Linq;
using CryptoQuest.Actions;
using CryptoQuest.Item.MagicStone;
using CryptoQuest.Sagas.MagicStone;
using CryptoQuest.Sagas.Objects;
using IndiGames.Core.Common;
using IndiGames.Core.Events;
using IndiGames.Core.Events.ScriptableObjects;
using IndiGames.Core.SceneManagementSystem.Events.ScriptableObjects;
using NSubstitute;
using NUnit.Framework;
using TinyMessenger;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CryptoQuest.Tests.Runtime.Item.MagicStone
{
    [TestFixture]
    public class GetMagicStonesTests
    {
        private MagicStoneInventory _stoneInventory;
        private VoidEventChannelSO _sceneLoadedEvent;
        private TinyMessageSubscriptionToken _getStoneToken;
        private TinyMessageSubscriptionToken _getStoneFailedToken;
        private TinyMessageSubscriptionToken _fillInventoryToken;
        private MagicStonesResponse _response;

        private bool _sceneLoaded;
        private bool _finishTest;

        [SetUp]
        public void Setup()
        {
            _finishTest = false;
            _stoneInventory = AssetDatabase.LoadAssetAtPath<MagicStoneInventory>(
                "Assets/ScriptableObjects/Inventories/StoneInventory.asset");
            _sceneLoadedEvent = AssetDatabase.LoadAssetAtPath<VoidEventChannelSO>(
                "Assets/ScriptableObjects/Events/SceneManagement/SceneLoadedEventChannel.asset");
        }

        [UnityTest, Category("Integration")]
        public IEnumerator FetchProfileCharactersAction_FetchCorrectStoneIntoClient()
        {
            _sceneLoaded = false;

            LogAssert.ignoreFailingMessages = true;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode
            ("Assets/Scenes/Maps/ElrodsCastle/ElrodsCastleTown_TheTavernF1.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
        
            _sceneLoadedEvent.EventRaised += SetSceneLoaded;
            yield return new WaitUntil(() => _sceneLoaded);

            _getStoneToken = ActionDispatcher.Bind<GetStonesResponsed>((ctx) => 
            {
                ActionDispatcher.Unbind(_getStoneToken);
                _response = ctx.Response;
            });

            _getStoneFailedToken = ActionDispatcher.Bind<GetStonesFailed>((ctx) => 
            {
                ActionDispatcher.Unbind(_getStoneFailedToken);
                _finishTest = true;
            });


            _fillInventoryToken = ActionDispatcher.Bind<StoneInventoryFilled>((ctx) => 
            {
                ActionDispatcher.Unbind(_fillInventoryToken);
                
                Assert.NotNull(_response);

                var ingameStonesResponseCount = _response.data.stones
                    .Where(s => s.inGameStatus == (int)EMagicStoneStatus.InGame).Count();
                Assert.AreEqual(_stoneInventory.MagicStones.Count, ingameStonesResponseCount);
                _finishTest = true;
            });
            ActionDispatcher.Dispatch(new FetchProfileMagicStonesAction());
            yield return new WaitUntil(() => _finishTest);
        }

        private void SetSceneLoaded()
        {
            Debug.Log($"Scene loaded");
            _sceneLoaded = true;
        }

        [TearDown]
        public void TearDown()
        {
            _sceneLoadedEvent.EventRaised -= SetSceneLoaded;
            _sceneLoaded = false;
            if (_getStoneToken != null)
                ActionDispatcher.Unbind(_getStoneToken);
            if (_fillInventoryToken != null)
                ActionDispatcher.Unbind(_fillInventoryToken);
            if (_getStoneFailedToken != null)
                ActionDispatcher.Unbind(_getStoneFailedToken);
        }
    }
}