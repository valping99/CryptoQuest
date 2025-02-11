using System.IO;
using System.Collections;
using System.Collections.Generic;
using CryptoQuest;
using CryptoQuest.Character;
using UnityEditor;
using UnityEngine;

namespace CryptoQuestEditor.NPC
{
    public class NPC_CreateAnimation : EditorWindow
    {
        public enum Npc_Action
        {
            Idle_Down = 0,
            Idle_Left = 1,
            Idle_Top = 2,
            Idle_Right = 3,
            Walking_Down = 4,
            Walking_Left = 5,
            Walking_Top = 6,
            Walking_Right = 7
        }
        private string _spriteLocation;
        private string _spriteName;
        private string _assetPath;
        private string _exportPath;
        private Npc_Action _npcAction;
        private GameObject _currentNPC;
        private SpriteRenderer _currentSprite;
        private List<Sprite> _listSprite = new();

        [MenuItem("Window/NPC/Animation")]
        public static void ShowWindow()
        {
            GetWindow<NPC_CreateAnimation>("Animation");
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Current NPC", EditorStyles.label);
            _currentNPC = (GameObject)EditorGUILayout.ObjectField(_currentNPC, typeof(GameObject), true, GUILayout.MaxWidth(264));
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Export Path", EditorStyles.label);
            _exportPath = EditorGUILayout.TextField(_exportPath, GUILayout.MaxWidth(264));
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (GUILayout.Button("Create Animation Clip", GUILayout.Height(60)))
            {
                InitializedSetup();
            }
        }

        private void InitializedSetup()
        {
            if (_currentNPC.GetComponent<NpcMovement>() == null) _currentNPC.AddComponent<NpcMovement>();
            _currentSprite = _currentNPC.GetComponent<SpriteRenderer>();
            string spritePath = AssetDatabase.GetAssetPath(_currentSprite.sprite);
            string spriteName = _currentSprite.sprite.name;
            _assetPath = Path.GetDirectoryName(spritePath);
            _spriteName = spriteName.TrimEnd('_').Remove(spriteName.LastIndexOf("_") + 1);
            _spriteLocation = spriteName.TrimEnd('_').Remove(spriteName.LastIndexOf("_"));
            if (!Directory.Exists(Path.Combine(_exportPath, _currentNPC.name)))
            {
                AssetDatabase.CreateFolder(_exportPath, _currentNPC.name);
            }
            GetSprites();
        }

        private void GetSprites()
        {
            UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(Path.Combine(_assetPath, _spriteLocation + ".png"));
            if (_listSprite != null) _listSprite.Clear();
            for (int i = 0; i < sprites.Length; i++)
            {
                foreach (UnityEngine.Object sprite in sprites)
                {
                    if (sprite.name == _spriteName + i)
                    {
                        AddSpriteToList(i, sprite as Sprite);
                    }
                }
            }
        }

        private void AddSpriteToList(int currentSprite, Sprite sprite)
        {
            switch (currentSprite)
            {
                case >= 0 and <= 3:
                    _npcAction = (Npc_Action)currentSprite;
                    _listSprite.Add(sprite);
                    CreateClip(_npcAction.ToString(), _listSprite);
                    _listSprite.Clear();
                    break;
                case > 3 and <= 7:
                    _listSprite.Add(sprite);
                    if (_listSprite.Count >= 4)
                    {
                        CreateClip(Npc_Action.Walking_Down.ToString(), _listSprite);
                        _listSprite.Clear();
                    }
                    break;
                case > 7 and <= 11:
                    _listSprite.Add(sprite);
                    if (_listSprite.Count >= 4)
                    {
                        CreateClip(Npc_Action.Walking_Left.ToString(), _listSprite);
                        _listSprite.Clear();
                    }
                    break;
                case > 11 and <= 15:
                    _listSprite.Add(sprite);
                    if (_listSprite.Count >= 4)
                    {
                        CreateClip(Npc_Action.Walking_Top.ToString(), _listSprite);
                        _listSprite.Clear();
                    }
                    break;
                case > 15 and <= 19:
                    _listSprite.Add(sprite);
                    if (_listSprite.Count >= 4)
                    {
                        CreateClip(Npc_Action.Walking_Right.ToString(), _listSprite);
                        _listSprite.Clear();
                    }
                    break;
            }
        }

        private void CreateClip(string clipName, List<Sprite> listSprite)
        {
            AnimationClip newClip = new();
            AnimationClipSettings newSettings = new();
            newSettings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(newClip, newSettings);
            EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            float interval = 1 / 4f;
            ObjectReferenceKeyframe[] orks = new ObjectReferenceKeyframe[listSprite.Count + 1];
            for (int i = 0; i < orks.Length; i++)
            {
                if (i == orks.Length - 1)
                {
                    orks[i].time = i * interval;
                    orks[i].value = listSprite[0];
                }
                else
                {
                    orks[i].time = i * interval;
                    orks[i].value = listSprite[i];
                }
            }

            AnimationUtility.SetObjectReferenceCurve(newClip, binding, orks);
            AssetDatabase.CreateAsset(newClip, Path.Combine(_exportPath, _currentNPC.name, $"{clipName}.anim"));
            AssetDatabase.SaveAssets();
        }
    }
}