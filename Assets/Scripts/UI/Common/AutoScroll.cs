using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CryptoQuest.UI.Common
{
    /// <summary>
    /// Vertical auto scroll on select some gameObject inside scroll
    /// </summary>
    public class AutoScroll : MonoBehaviour
    {
        [SerializeField] protected ScrollRect _scrollRect;

        [Tooltip("Template item of the scroll view which will be instantiated on runtime")]
        [SerializeField] private RectTransform _singleItemRect;

        [Tooltip("Enable this flag to run the auto-scroll logic in the Update()")]
        [SerializeField] private bool _useUpdate = true;

        protected RectTransform _viewport;
        private float _verticalOffset;
        private float _lowerBound;
        private float _upperBound;

        private void Awake()
        {
            _verticalOffset = _singleItemRect.rect.height;
            _viewport = _scrollRect.viewport;

            CalculateBoundariesByHeight();
        }

        private void Update()
        {
            if (_useUpdate) Scroll();
        }

        protected virtual float GetViewportY() => _viewport.position.y;

        private float NormalizePositionFromDifferentPivots(Rect rect)
        {
            var yPos = GetViewportY();
            if (_viewport.pivot.y == 0)
                return yPos + (rect.height / 2);
            if (Math.Abs(_viewport.pivot.y - 0.5) < 0.1)
                return yPos;
            return yPos - (rect.height / 2);
        }

        private const int OFFSET = 2; 
        private void CalculateBoundariesByHeight()
        {
            var rect = _viewport.rect;
            float yPosViewport = NormalizePositionFromDifferentPivots(rect);

            _lowerBound = yPosViewport - rect.height / 2 + _verticalOffset / 2;
            _lowerBound = Mathf.Round(_lowerBound);
            _upperBound = yPosViewport + rect.height / 2 + _verticalOffset / 2;
            _upperBound = Mathf.Round(_upperBound) - OFFSET;
        }

        /// <summary>
        /// Check if the currently selected items belong to the scroll view that you want to use.
        /// </summary>
        private bool IsSelectedChildOfScrollRect(GameObject selectedGameObject)
        {
            return selectedGameObject.transform.parent == _scrollRect.content;
        }

        public void Scroll()
        {
            var current = EventSystem.current.currentSelectedGameObject;
            if (current == null) return;

            if (IsSelectedChildOfScrollRect(current) == false) return;

            var selectedRowPositionY = CalculatedSelectedObjectY(current);
            // Sometime the difference between position and bound are so small
            // and cause auto scroll not working, so I round it
            selectedRowPositionY = Mathf.Round(selectedRowPositionY);
            ScrollUpIfOutOfLowerBound(selectedRowPositionY);
            ScrollDownIfOutOfUpperBound(selectedRowPositionY);
        }

        protected virtual float CalculatedSelectedObjectY(GameObject currentSelected)
            => currentSelected.transform.position.y;

        public void ScrollToTop()
        {
            _scrollRect.verticalNormalizedPosition = 1f;
        }

        private void ScrollUpIfOutOfLowerBound(float selectedRowPositionY)
        {
            if (selectedRowPositionY <= _lowerBound)
                _scrollRect.content.anchoredPosition += Vector2.up * _verticalOffset;
        }

        private void ScrollDownIfOutOfUpperBound(float selectedRowPositionY)
        {
            if (selectedRowPositionY >= _upperBound)
                _scrollRect.content.anchoredPosition += Vector2.down * _verticalOffset;
        }
    }
}