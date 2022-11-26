using System.Diagnostics;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public static class LocalizationUtils
{
    public static string GetValidatedLocalisedText(this LocalizedString localizedString)
    {
#if UNITY_EDITOR
        if (localizedString.IsEmpty)
        {
            return "Missing Loc";
        }
#endif
        if (localizedString.GetLocalizedStringAsync().Result == null)
        {
            //Comment out between these lines to see the issue replicating
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            UnityEngine.Debug.Log("String Localization is null");
            var table = LocalizationSettings.StringDatabase.GetTable("GENERATED", LocalizationSettings.ProjectLocale);

            if (table)
            {
                var entry = table.GetEntryFromReference(localizedString.TableEntryReference);

                if (entry != null)
                {
                    return entry.Value;
                }
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return "Loc is Null";
        }

        return localizedString.IsEmpty ? string.Empty : localizedString.GetLocalizedStringAsync().Result;
    }
}
