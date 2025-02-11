using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CryptoQuest.Beast.Avatar;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace CryptoQuestEditor.CryptoQuestEditor.Scripts.Character.Avatar
{
    public class AvatarBeastSetData
    {
        [Name("beast_id")] public int BeastId { get; set; }
        [Name("class_id")] public int ClassId { get; set; }
        [Name("element_id")] public int ElementId { get; set; }
        [Name("image_name")] public string ImageName { get; set; }
    }

    [CustomEditor(typeof(BeastAvatarSO), true)]
    public class BeastAvatarSOEditor : Editor
    {
        private const string BEAST_AVATAR_PATH = "Assets/Arts/Beasts/";
        private BeastAvatarSO Target => (BeastAvatarSO)target;
        private SerializedProperty _avatarMappings;

        protected virtual void OnEnable()
        {
            _avatarMappings = serializedObject.FindProperty("_avatarMappings");
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            // add Load all data button
            root.Add(new Button(() =>
            {
                ImportCsv();
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            })
            {
                text = "Import CSV",
            });
            return root;
        }

        private void ImportCsv()
        {
            var path = EditorUtility.OpenFilePanel("Import CSV", "", "csv");
            if (string.IsNullOrEmpty(path)) return;

            EditorUtility.DisplayProgressBar("Importing CSV", "Importing CSV", 0);

            try
            {
                using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader stream = new StreamReader(fs);
                ReadStream(stream);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                EditorUtility.ClearProgressBar();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void ReadStream(StreamReader stream)
        {
            using CsvReader csvReader = new CsvReader(stream, CultureInfo.InvariantCulture);
            csvReader.Read();
            csvReader.ReadHeader();

            List<AvatarBeastSetData> rows = new List<AvatarBeastSetData>();

            while (csvReader.Read())
            {
                EditorUtility.DisplayProgressBar("Reading CSV", "Reading each row",
                    csvReader.Context.Row / (float)csvReader.Context.Record.Length);

                if (IgnoreRow(csvReader.Context)) continue;
                AvatarBeastSetData message = csvReader.GetRecord<AvatarBeastSetData>();

                if (message.BeastId == 0 || message.ClassId == 0 || message.ElementId == 0) continue;

                if (rows.Exists(x =>
                        x.BeastId == message.BeastId &&
                        x.ClassId == message.ClassId &&
                        x.ElementId == message.ElementId)) continue;

                rows.Add(message);
            }

            if (rows.Count == 0) return;
            _avatarMappings.ClearArray();

            FillData(rows);

            _avatarMappings.serializedObject.ApplyModifiedProperties();
            EditorUtility.ClearProgressBar();
        }

        private void FillData(List<AvatarBeastSetData> rows)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                _avatarMappings.InsertArrayElementAtIndex(i);
                SerializedProperty element = _avatarMappings.GetArrayElementAtIndex(i);

                string[] guids = AssetDatabase.FindAssets("t:sprite", new[] { BEAST_AVATAR_PATH });

                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    var assetRef = new AssetReferenceT<Sprite>(guid);
                    assetRef.SetEditorAsset(asset);
                    assetRef.SubObjectName = asset.name;

                    if (rows[i].ImageName == asset.name)
                    {
                        var beastData = new BeastAvatarData()
                        {
                            BeastId = rows[i].BeastId,
                            ClassId = rows[i].ClassId,
                            ElementId = rows[i].ElementId,
                            Image = assetRef,
                        };

                        element.boxedValue = beastData;
                        break;
                    }
                }
            }
        }

        protected virtual bool IgnoreRow(ReadingContext contextRawRecord)
        {
            return contextRawRecord.Row == 1;
        }
    }
}