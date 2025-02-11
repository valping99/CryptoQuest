﻿using System;
using System.Collections.Generic;
using System.Net;
using CryptoQuest.Actions;
using CryptoQuest.Networking;
using CryptoQuest.System;
using Newtonsoft.Json;
using TinyMessenger;
using UniRx;
using UnityEngine;
using CryptoQuest.API;
using IndiGames.Core.Common;
using IndiGames.Core.Events;
using APIProfile = CryptoQuest.API.Profile;

namespace CryptoQuest.Sagas
{
    public class DebugLogin : SagaBase<DebugLoginAction>
    {
        [Serializable]
        private struct DebugBody
        {
            [JsonProperty("token")]
            public string Token;
        }

        [SerializeField] private string _debugToken =
            "c1CRi-qi8jfOJHJ5rjH2tO9xjSA_UUORQ1eRBt59BY8.sc6AO3PQnOrQV0hG4SoQ6mTeU8r1n4-WKuCuzrpnmw1";


        private TinyMessageSubscriptionToken _loginAction;

        protected override void HandleAction(DebugLoginAction ctx)
        {
            var restClient = ServiceProvider.GetService<IRestClient>();
            restClient
                .WithBody(new DebugBody { Token = _debugToken })
                .WithHeaders(new Dictionary<string, string> { { "DEBUG_KEY", APIProfile.DEBUG_KEY } })
                .Post<AuthResponse>(Accounts.DEBUG_LOGIN)
                .Subscribe(SaveCredentials, DispatchLoginFailed, DispatchLoginFinished);
        }

        private void SaveCredentials(AuthResponse response)
        {
            if (response.code != (int)HttpStatusCode.OK) return;
            ActionDispatcher.Dispatch(new InternalAuthenticateAction(response.data));
        }

        private void DispatchLoginFailed(Exception obj)
        {
            ActionDispatcher.Dispatch(new LoginFailedAction());
        }

        private void DispatchLoginFinished()
        {
            ActionDispatcher.Dispatch(new LoginFinishedAction());
        }
    }
}