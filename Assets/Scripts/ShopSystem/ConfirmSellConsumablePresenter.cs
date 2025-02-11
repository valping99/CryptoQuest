﻿using System.Collections;
using CryptoQuest.ShopSystem.Helpers;
using CryptoQuest.ShopSystem.Sagas;
using CryptoQuest.UI.Dialogs.ChoiceDialog;
using IndiGames.Core.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;

namespace CryptoQuest.ShopSystem
{
    public class ConfirmSellConsumablePresenter : MonoBehaviour
    {
        [SerializeField] private UIQuantityDialog _quantityConfigDialog;
        [SerializeField] private UIShopItemPool<UIConsumableShopItem> _pool;
        [SerializeField] private LocalizedString _confirmString = new("ShopUI", "DIALOG_SELL_CONFIRM");
        [SerializeField] private SellPanel _sellPanel;
        [SerializeField] private UIInventoryItemList<UIConsumableShopItem> _sellingList;
        [SerializeField] private TransactionResultPanel _resultPanel;

        private UIChoiceDialog _confirmDialog;

        private void Awake()
        {
            ChoiceDialogController.Instance.InstantiateAsync(dialog => _confirmDialog = dialog);
        }

        private void OnEnable()
        {
            _sellingList.ItemSelected += ConfigQuantity;
        }

        private void OnDisable()
        {
            _sellingList.ItemSelected -= ConfigQuantity;
        }

        private void ConfigQuantity(UIConsumableShopItem item)
        {
            EventSystem.current.SetSelectedGameObject(null);
            _sellPanel.DisableInput();
            var currentScrollRect = GetComponentInChildren<ScrollRect>();
            var selectables = currentScrollRect.GetComponentsInChildren<Selectable>();
            foreach (var selectable in selectables) selectable.interactable = false;

            _quantityConfigDialog.gameObject.SetActive(true);
            _quantityConfigDialog
                .WithQuantityChangedCallback(quantity =>
                {
                    _confirmString["PRICE"] = new StringVariable { Value = $"{item.Price * quantity}G" };
                    _confirmDialog.SetMessage(_confirmString);
                })
                .Show(item.Info.Quantity);

            _confirmDialog
                .WithNoCallback(() => StartCoroutine(item.gameObject.CoDelaySelect()))
                .WithYesCallback(() => OnConfirmSell(item))
                .WithHideCallback(() =>
                {
                    _quantityConfigDialog.gameObject.SetActive(false);
                    _quantityConfigDialog.Hide();
                    _sellPanel.EnableInput();
                    foreach (var selectable in selectables) selectable.interactable = true;
                })
                .SetMessage(_confirmString)
                .Show();

            var button = item.GetComponent<Button>();
            button.image.overrideSprite = button.spriteState.pressedSprite;
        }

        private void OnConfirmSell(UIConsumableShopItem item)
        {
            var selectedQuantity = _quantityConfigDialog.CurrentQuantity;
            ActionDispatcher.Dispatch(new SellConsumableAction(item.Info.Data, selectedQuantity));
            item.Render(item.Info);
            ShowSellSuccess(item);
        }

        private void ShowSellSuccess(UIConsumableShopItem item)
        {
            var transformParent = item.transform.parent;
            var childCount = transformParent.childCount;
            var itemIndex = item.transform.GetSiblingIndex();

            _resultPanel
                .AddHideCallback(() =>
                {
                    if (item.Info.Quantity <= 0) _pool.Release(item);
                    else
                    {
                        EventSystem.current.SetSelectedGameObject(item.gameObject);
                        return;
                    }

                    if (transformParent.childCount == 0) return;

                    var childToSelect = itemIndex == childCount - 1
                        ? transformParent.GetChild(itemIndex - 1).gameObject
                        : transformParent.GetChild(itemIndex).gameObject;

                    EventSystem.current.SetSelectedGameObject(childToSelect);
                })
                .ShowSuccess();
        }
    }
}