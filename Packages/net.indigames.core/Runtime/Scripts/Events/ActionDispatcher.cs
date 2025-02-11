﻿using System;
using TinyMessenger;
using UnityEngine;

namespace IndiGames.Core.Events
{
    public class LogErrorHandler : ISubscriberErrorHandler
    {
        public class ErrorAction : ActionBase
        {
            public Exception Exception { get; }

            public ErrorAction(Exception exception)
            {
                Exception = exception;
            }
        }
        public void Handle(ITinyMessage message, Exception exception)
        {
            Debug.LogWarning($"{message} {exception}");
            ActionDispatcher.Dispatch(new ErrorAction(exception));
        }
    }
    public abstract class ActionBase : ITinyMessage
    {
        public object Sender { get; } = null;
    }

    public static class ActionDispatcher
    {
        private static readonly ITinyMessengerHub MessengerHub =
            new TinyMessengerHub(new LogErrorHandler());

        public static void Dispatch<T>(T action) where T : ActionBase => MessengerHub.Publish(action);

        public static TinyMessageSubscriptionToken Bind<T>(Action<T> callback) where T : ActionBase =>
            MessengerHub.Subscribe<T>(callback);

        public static void Unbind(TinyMessageSubscriptionToken token) => MessengerHub.Unsubscribe(token);
    }
}