using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Yarn.Unity;
using Object = UnityEngine.Object;

#if USE_UNITY_LOCALIZATION
using UnityEngine.Localization.Tables;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement;
#endif

namespace CryptoQuest.System.Dialogue.YarnManager
{
    public class UnityLocalisedLineProvider : LineProviderBehaviour
    {
#if USE_UNITY_LOCALIZATION
        // the string table asset that has all of our (hopefully) localised strings inside
        [SerializeField] public LocalizedStringTable StringsTable;
        [SerializeField] public LocalizedAssetTable AssetTable;

        // the runtime table we actually get our strings out of
        // this changes at runtime depending on the language
        private StringTable currentStringsTable;
        private AssetTable currentAssetTable;

        private List<AsyncOperationHandle<Object>> pendingLoadOperations = new List<AsyncOperationHandle<Object>>();
        private Dictionary<string, Object> loadedAssets = new Dictionary<string, Object>();

        public void SetStringTable(LocalizedStringTable newTable)
        {
            StringsTable = newTable;
            currentStringsTable = newTable.GetTable();

            StringsTable.TableChanged += OnAssetTableChanged;
        }

        private void OnAssetTableChanged(StringTable value)
        {
            currentStringsTable = value;
        }

        public override string LocaleCode => LocalizationSettings.SelectedLocale.Identifier.Code;

        public override bool LinesAvailable
        {
            get
            {
                // If a strings table wasn't set, we'll never have lines
                if (this.StringsTable.IsEmpty)
                {
                    return false;
                }

                // If we haven't finished loading the strings table, we don't
                // have lines yet
                if (this.currentStringsTable == null)
                {
                    return false;
                }

                // If we have an asset table, then we need to check some things
                // about it
                if (this.AssetTable.IsEmpty == false)
                {
                    // If the table hasn't finished loading yet, then lines
                    // aren't available yet
                    if (this.currentAssetTable == null)
                    {
                        return false;
                    }

                    // If we're pending the load of certain assets, then lines
                    // aren't available yet
                    if (pendingLoadOperations.Count > 0)
                    {
                        return false;
                    }
                }

                // We're good to go!
                return true;
            }
        }

        public override LocalizedLine GetLocalizedLine(Yarn.Line line)
        {
            var text = line.ID;
            if (currentStringsTable != null)
            {
                text =
                    currentStringsTable[line.ID]?.LocalizedValue ??
                    $"Error: Missing localisation for line {line.ID} in string table {currentStringsTable.LocaleIdentifier}";
            }

            // Construct the localized line
            LocalizedLine localizedLine = new LocalizedLine()
            {
                TextID = line.ID,
                RawText = text,
                Substitutions = line.Substitutions,
            };

            // Attempt to fetch metadata tags for this line from the string
            // table
            var metadata = currentStringsTable[line.ID]?.GetMetadata<LineMetadata>();

            if (metadata != null)
            {
                localizedLine.Metadata = metadata.tags;
            }

            // Attempt to fetch a loaded asset for this line
            var lineIDWithoutPrefix = line.ID.Replace("line:", "");

            // If we have a loaded asset associated with this line, return it
            if (loadedAssets.TryGetValue(lineIDWithoutPrefix, out var asset))
            {
                localizedLine.Asset = asset;
            }

            return localizedLine;
        }

        public override void PrepareForLines(IEnumerable<string> lineIDs)
        {
            if (AssetTable.IsEmpty != true)
            {
                // We have an asset table, so 1. ensure that the locale-specific asset table is loaded, and then 2. ensure that each asset that we care about has been loaded.

                if (currentAssetTable == null)
                {
                    // The asset table hasn't yet loaded, so get it
                    // asynchronously and then start preloading from it
                    RunAfterComplete(AssetTable.GetTableAsync(),
                        (loadedAssetTable) => { PreloadLinesFromTable(loadedAssetTable, lineIDs); });
                }
                else
                {
                    // The asset table has already loaded, so start preloading
                    // now
                    PreloadLinesFromTable(currentAssetTable, lineIDs);
                }
            }

            void PreloadLinesFromTable(AssetTable table, IEnumerable<string> lineIDs)
            {
                var lineIDsWithoutPrefix = new List<string>();
                foreach (var l in lineIDs)
                {
                    lineIDsWithoutPrefix.Add(l.Replace("line:", ""));
                }

                // Remove and release the lines that have been previously loaded
                // but aren't in this set of lines to expect - they're not
                // needed now
                var assetKeysToUnload = new HashSet<string>(loadedAssets.Keys);
                assetKeysToUnload.ExceptWith(lineIDsWithoutPrefix);
                foreach (var assetKeyToUnload in assetKeysToUnload)
                {
                    var entryToRelease = table.GetEntry(assetKeyToUnload);

                    if (entryToRelease != null)
                    {
                        table.ReleaseAsset(entryToRelease);

                        loadedAssets.Remove(assetKeyToUnload);
                    }
                }

                // Load all assets that we need
                foreach (var id in lineIDsWithoutPrefix)
                {
                    var entry = table.GetEntry(id);
                    if (entry == null)
                    {
                        // This ID doesn't exist in the asset table - nothing to
                        // load!
                        continue;
                    }

                    var loadOperation = table.GetAssetAsync<Object>(entry.KeyId);

                    if (loadOperation.IsDone == false)
                    {
                        // If the load operation has already completed, there's
                        // no need to wait - we can use its result now.
                        Debug.Log($"Asset for {id} was already loaded");
                        loadedAssets[id] = loadOperation.Result;
                    }
                    else
                    {
                        // Wait until this load operation completes, and then
                        // get its result.
                        pendingLoadOperations.Add(loadOperation);
                        loadOperation.Completed += (operation) =>
                        {
                            pendingLoadOperations.Remove(loadOperation);
                            if (operation.Status == AsyncOperationStatus.Succeeded)
                            {
                                loadedAssets[id] = operation.Result;
                            }
                            else
                            {
                                Debug.LogError($"Asset load operation for ID {id} failed!");
                            }
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Waits until an asynchronous operation has completed, and then calls
        /// a completion handler when it's done.
        /// </summary>
        /// <typeparam name="T">The type of object that <paramref
        /// name="operation"/> will return when it completes.</typeparam>
        /// <param name="operation">The asynchonous operation to wait
        /// for.</param>
        /// <param name="onComplete">A method to call when the operation
        /// completes successfully.</param>
        /// <param name="onFailure">A method to call when the operation
        /// fails.</param>
        private void RunAfterComplete<T>(AsyncOperationHandle<T> operation, Action<T> onComplete, Action onFailure
            =
            null)
        {
            if (onComplete is null)
            {
                throw new ArgumentNullException(nameof(onComplete));
            }

            StartCoroutine(RunAfterCompleteImpl(operation, onComplete));

            IEnumerator RunAfterCompleteImpl(AsyncOperationHandle<T> operation, Action<T> onComplete)
            {
                yield return operation;

                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    onComplete(operation.Result);
                }
                else
                {
                    onFailure?.Invoke();
                }
            }
        }
#else
        public override string LocaleCode => "error";

        public override void PrepareForLines(IEnumerable<string> lineIDs)
        {
            Debug.LogError(
                $"{nameof(UnityLocalisedLineProvider)} requires that the Unity Localization package is installed in the project. To fix this, install Unity Localization.");
        }

        public override bool LinesAvailable =>
            true; // likewise later we should check that it has actually loaded the string table

        public override void Start()
        {
            Debug.LogError(
                $"{nameof(UnityLocalisedLineProvider)} requires that the Unity Localization package is installed in the project. To fix this, install Unity Localization.");
        }

        public override LocalizedLine GetLocalizedLine(Yarn.Line line)
        {
            Debug.LogError(
                $"{nameof(UnityLocalisedLineProvider)}: Can't create a localised line for ID {line.ID} because the Unity Localization package is not installed in this project. To fix this, install Unity Localization.");

            return new LocalizedLine()
            {
                TextID = line.ID,
                RawText =
                    $"{line.ID}: Unable to create a localised line, because the Unity Localization package is not installed in this project.",
                Substitutions = line.Substitutions,
            };
        }
#endif
    }

#if USE_UNITY_LOCALIZATION
    public class LineMetadata : UnityEngine.Localization.Metadata.IMetadata
    {
        public string nodeName;
        public string[] tags;
    }
#endif
}