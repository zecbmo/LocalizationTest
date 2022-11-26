using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.VersionControl;
using UnityEngine.SocialPlatforms;

public class LocalizationWindow : MonoBehaviour
{

    [MenuItem("Localization/Clear Empty Table Values")]
    static void ClearEmptyTableValues()
    {
        if (EditorUtility.DisplayDialog("Clear Empty Values from Generated String table?",
                "Are you sure you want to clear the empty values?\nThis will remove any key with the English value \"STRING_EMPTY\" and those that are null or whitespace ",
                "Clear Empty Values", "Cancel"))
        {
            var table = LocalizationSettings.StringDatabase.GetTable("GENERATED", LocalizationSettings.ProjectLocale);

            List<StringTableEntry> entriesToRemove = new List<StringTableEntry>();

            foreach (var key in table.Values)
            {
                if (key.LocalizedValue == "STRING_EMPTY" || string.IsNullOrWhiteSpace(key.LocalizedValue))
                {
                    entriesToRemove.Add(key);
                }
            }

            var collection = LocalizationEditorSettings.GetStringTableCollection("GENERATED");

            foreach (var key in entriesToRemove)
            {
                collection.RemoveEntry(key.Key);
            }

            EditorUtility.SetDirty(table);
            EditorUtility.SetDirty(table.SharedData);

        }

    }

    [MenuItem("Localization/Generate Audio Tables")]
    static void GenerateAudioTables()
    {
        if (EditorUtility.DisplayDialog("Generate Audio Tables From String Tables?",
                "This will generate audio tables based off of the string value tables in the 'GENERATED' string table.\n\nEXISTING DATA WILL BE WIPED!\n\nThis Process will hang the editor for a few minutes.",
                "Generate Audio Tables", "Cancel"))
        {

            string[] audioGUIDS = AssetDatabase.FindAssets("t:AudioClip");
            Dictionary<string, AudioClip> audioClipsInProject = new Dictionary<string, AudioClip>();

            foreach (string guid in audioGUIDS)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) return;
                AudioClip clip = (AudioClip)AssetDatabase.LoadAssetAtPath<Object>(path);
                if (clip != null)
                {
                    if (audioClipsInProject.ContainsKey(clip.name))
                    {
                        Debug.LogWarning("Duplicate Audio Name found. The following file has not been accepted, please rename it or its equivilant:\n\n " + AssetDatabase.GetAssetPath(clip));
                    }
                    else
                    {
                        audioClipsInProject.Add(clip.name, clip);
                    }
                }

            }


            //parse through all the the keys
            var assetCollection = LocalizationEditorSettings.GetAssetTableCollection("GENERATED_AUDIO");
            var stringCollection = LocalizationEditorSettings.GetStringTableCollection("GENERATED");
            List<string> localIDs = new List<string>();


            //clear the audio tables 
            foreach (AssetTable allTables in assetCollection.AssetTables)
            {
                allTables.Clear();
                allTables.SharedData.Clear();
                EditorUtility.SetDirty(allTables);
                EditorUtility.SetDirty(allTables.SharedData);

                localIDs.Add(allTables.LocaleIdentifier.Code);
            }

            var table = LocalizationSettings.StringDatabase.GetTable("GENERATED", LocalizationSettings.ProjectLocale);

            foreach (string localID in localIDs)
            {
                foreach (StringTableEntry key in table.Values)
                {
                    string audioID = key.Key + "_" + localID;
                    //Debug.Log("Searching for key: " + audioID);

                    if (audioClipsInProject.ContainsKey(audioID))
                    {
                        //we have found an audio file
                        //we need to add the audio to the addressibles before doing annything
                        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
                        string assetPath = AssetDatabase.GetAssetPath(audioClipsInProject[audioID]);
                        string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
                        settings.CreateAssetReference(assetGUID);

                        var assetTable = (AssetTable)assetCollection.GetTable(localID);

                        //add the audiofile to the laungaue table
                        AssetTableEntry assetEntry = assetTable.GetEntry(key.Key);
                        assetTable.AddEntry(key.Key, assetGUID);
                        
                        EditorUtility.SetDirty(assetTable);
                        EditorUtility.SetDirty(assetTable.SharedData);
                    }
                }
            }
        }
    }
}
