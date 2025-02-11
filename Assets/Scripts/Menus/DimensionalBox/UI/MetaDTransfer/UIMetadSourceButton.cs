﻿using CryptoQuest.Menu;
using CryptoQuest.UI.Menu;
using UnityEngine;
using System;
using CryptoQuest.Inventory.Currency;

namespace CryptoQuest.Menus.DimensionalBox.UI.MetaDTransfer
{
    public class UIMetadSourceButton : MonoBehaviour
    {
        public event Action<CurrencySO> SelectedCurrency;
        public event Action<CurrencySO> Inpspected;

        [field: SerializeField] public UICurrency CurrencyUI { get; private set; }
        [field: SerializeField] public MultiInputButton Button { get; private set; }
        
        public bool Interactable 
        {
            get => Button.interactable;
            set 
            {
                Button.interactable = value;
            }
        }

        private void OnEnable()
        {
            Button.Selected += OnSelected;
            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(() => SelectedCurrency?.Invoke(CurrencyUI.Currency));
        }

        private void OnDisable()
        {
            Button.Selected -= OnSelected;
        }

        public void OnSelected()
        {
            Inpspected?.Invoke(CurrencyUI.Currency);
        }
    }
}