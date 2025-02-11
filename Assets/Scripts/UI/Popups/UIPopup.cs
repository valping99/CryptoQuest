﻿using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace CryptoQuest.UI.Popups
{
    public class UIPopup : MonoBehaviour
    {
        [SerializeField] private LocalizeStringEvent _headerString;
        [SerializeField] private LocalizeStringEvent _bodyString;
        [SerializeField] private TMP_Text _headerText;
        [SerializeField] private TMP_Text _bodyText;

        public UIPopup WithHeader(LocalizedString header)
        {
            _headerString.StringReference = header;
            return this;
        }

        public UIPopup WithBody(LocalizedString body)
        {
            // Prevent change to same message when multiple show up
            LocalizedString stringReference = new LocalizedString
            {
                TableReference = body.TableReference,
                TableEntryReference = body.TableEntryReference,
                Arguments = body.Arguments
            };

            _bodyString.StringReference = stringReference;
            return this;
        }

        public UIPopup WithBody(string body)
        {
            _bodyText.text = body;
            return this;
        }

        public UIPopup SetHeaderColor(Color color)
        {
            _headerText.color = color;
            return this;
        }

        public UIPopup SetBodyColor(Color color)
        {
            _bodyText.color = color;
            return this;
        }
    }
}