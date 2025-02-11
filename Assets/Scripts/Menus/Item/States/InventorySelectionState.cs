﻿using CryptoQuest.Input;
using CryptoQuest.Inventory.ScriptableObjects.Item.Type;
using CryptoQuest.Menus.Item.UI;
using CryptoQuest.UI.Menu;

namespace CryptoQuest.Menus.Item.States
{
    public class InventorySelectionState : ItemStateBase
    {
        private readonly InputMediatorSO _input;
        private bool _onStateEnter;

        public InventorySelectionState(UIConsumableMenuPanel consumablePanel) : base(consumablePanel)
        {
            _input = _consumablePanel.Input;
        }

        public override void OnEnter()
        {
            _consumablePanel.EnableAllHeroButtons(false);
            _consumablePanel.Focusing += SelectFirstTab;

            _input.MenuCancelEvent += HandleCancel;
            _input.TabChangeEvent += ChangeTab;

            UIConsumableItem.Using += UseItem;
            
            SelectFirstTab();
        }

        public override void OnExit()
        {
            _consumablePanel.Focusing -= SelectFirstTab;

            _input.MenuCancelEvent -= HandleCancel;
            _input.TabChangeEvent -= ChangeTab;

            UIConsumableItem.Using -= UseItem;
        }

        private void SelectFirstTab()
        {
            _onStateEnter = true;
            _consumablePanel.Interactable = true;

            _consumablePanel.ShowItemsWithType(EConsumableType.Consumable);
        }

        private void HandleCancel()
        {
            _onStateEnter = false;
            _consumablePanel.Interactable = false;

            UIMainMenu.OnBackToNavigation();
        }

        private void ChangeTab(float direction)
        {
            if (!_onStateEnter) return;
            _consumablePanel.ChangeTab(direction);
        }

        private void UseItem(UIConsumableItem selectedConsumableItem)
        {
            fsm.RequestStateChange(ItemMenuStateMachine.ItemConsume);
        }
    }
}