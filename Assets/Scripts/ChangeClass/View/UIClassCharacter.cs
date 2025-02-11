using System;
using System.Collections;
using System.Collections.Generic;
using CryptoQuest.ChangeClass.ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CryptoQuest.ChangeClass.View
{
    public class UIClassCharacter : MonoBehaviour
    {
        public Action<UIOccupation> OnSelected;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private UIOccupation _characterClassObject;
        private List<Button> _listButton = new();

        private void OnItemSelected(UIOccupation item)
        {
            OnSelected?.Invoke(item);
        }

        public void RenderClassToChange(List<ChangeClassSO> characterClasses)
        {
            CleanUpScrollView();
            foreach (var character in characterClasses)
            {
                var newClass = Instantiate(_characterClassObject, _scrollRect.content);
                newClass.OnItemSelected += OnItemSelected;
                newClass.ConfigureCell(character);
            }
            StartCoroutine(SelectDefaultButton());
        }

        private IEnumerator SelectDefaultButton()
        {
            yield return null;
            if (_scrollRect.content.childCount == 0) yield break;
            var firstItemGO = _scrollRect.content.GetChild(0).gameObject.GetComponent<UIOccupation>();
            EventSystem.current.SetSelectedGameObject(firstItemGO.gameObject);
            OnSelected?.Invoke(firstItemGO);
            GetListButton();
        }

        private void GetListButton()
        {
            _listButton.Clear();
            foreach (Transform child in _scrollRect.content)
            {
                _listButton.Add(child.GetComponent<Button>());
            }
        }

        private void CleanUpScrollView()
        {
            foreach (Transform child in _scrollRect.content)
            {
                Destroy(child.gameObject);
            }
        }

        public void EnableInteractable(bool isEnable)
        {
            foreach (var button in _listButton)
            {
                button.interactable = isEnable;
            }
        }
    }
}