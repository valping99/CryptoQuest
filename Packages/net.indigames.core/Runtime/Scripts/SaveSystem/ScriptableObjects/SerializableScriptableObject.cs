﻿using IndiGames.Core.EditorTools.Attributes.ReadOnlyAttribute;
using UnityEditor;
using UnityEngine;

namespace IndiGames.Core.SaveSystem.ScriptableObjects
{
    public class SerializableScriptableObject : ScriptableObject
    {
        [SerializeField, ReadOnly] private string _guid;
        public string Guid => _guid;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(this);
            _guid = AssetDatabase.AssetPathToGUID(assetPath);
        }
#endif
    }
}