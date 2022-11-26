using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine;
using System;
using UnityEngine.Localization.Tables;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditor.Localization;
using System.IO;
#endif


[Serializable]
public class LocalizedText
{
    [SerializeField]
    public long id; //the long KeyID of the entry within the StringTable - used to update the values if the string key is change within the editor 

    [SerializeField]
    public string englishValue; //helper value for the english value in the localized string

    [SerializeField]
    public LocalizedString localizedString = null; // unitys implementation

    //wrapper functions for localizedString
    public string GetValidatedLocalisedText()
    {
        return localizedString.GetValidatedLocalisedText();
    }

    public string GetLocalizedString()
    {
        return localizedString.GetLocalizedStringAsync().Result;
    }

    public bool IsEmpty
    {
        get {
            if (localizedString.IsEmpty || localizedString.GetLocalizedStringAsync().Result == "STRING_EMPTY")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

#if UNITY_EDITOR
    public void SetLocalizedStringEnglishValue(string value)
    {
        var table = LocalizationSettings.StringDatabase.GetTable("GENERATED", LocalizationSettings.ProjectLocale);
        var entry = table.AddEntryFromReference(localizedString.TableEntryReference, value);
        //var entry = table.GetEntryFromReference(localizedString.TableEntryReference);
        EditorUtility.SetDirty(table);
        EditorUtility.SetDirty(table.SharedData);
    }
#endif
}




