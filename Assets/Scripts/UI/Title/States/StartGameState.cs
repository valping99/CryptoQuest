﻿using CryptoQuest.Actions;
using CryptoQuest.System.SaveSystem;
using IndiGames.Core.Common;
using IndiGames.Core.Events;
using TinyMessenger;

namespace CryptoQuest.UI.Title.States
{
    public class StartGameState : IState
    {
        private UIStartGame _startGamePanel;
        private UITitleSetting _titleSetting;
        private TitleStateMachine _stateMachine;
        private TinyMessageSubscriptionToken _walletConnectCompleted;

        public void OnEnter(TitleStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            stateMachine.TryGetComponentInChildren(out _startGamePanel);
            stateMachine.TryGetComponentInChildren(out _titleSetting);
            _startGamePanel.gameObject.SetActive(true);
            _startGamePanel.StartGameBtn.Select();
            _startGamePanel.StartGameBtn.onClick.AddListener(StartGameButtonPressed);
            _titleSetting.SettingButton.onClick.AddListener(SettingButtonPressed);
            _walletConnectCompleted = ActionDispatcher.Bind<ConnectWalletCompleted>(OnWalletConnectCompleted);
        }

        public void OnExit(TitleStateMachine stateMachine)
        {
            _startGamePanel.StartGameBtn.onClick.RemoveListener(StartGameButtonPressed);
            _titleSetting.SettingButton.onClick.RemoveListener(SettingButtonPressed);
            _startGamePanel.gameObject.SetActive(false);
            ActionDispatcher.Unbind(_walletConnectCompleted);
        }

        private void SettingButtonPressed()
        {
            _stateMachine.ChangeState(new SettingState());
        }

        private void StartGameButtonPressed()
        {
            _stateMachine.ChangeState(new CheckConnectWalletState());
        }

        private void OnWalletConnectCompleted(ConnectWalletCompleted ctx)
        {
            if (!ctx.IsSuccess) _stateMachine.ChangeState(new WalletLoginFailed());
        }
    }
}