using UnityEngine;

namespace CryptoQuest.UI.Utilities
{
    public static class ComponentUtils
    {
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            return component.TryGetComponent<T>(out var foundComponent)
                ? foundComponent
                : component.gameObject.AddComponent<T>();
        }

        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            return go.TryGetComponent<T>(out var foundComponent)
                ? foundComponent
                : go.AddComponent<T>();
        }
    }
}