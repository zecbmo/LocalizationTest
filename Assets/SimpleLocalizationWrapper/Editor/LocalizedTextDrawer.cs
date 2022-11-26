using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;
using System.Linq;

[CustomPropertyDrawer(typeof(LocalizedText))]
public class LocalizedTextDrawer : PropertyDrawer
{  
    static int keyIDIncrimentor =0;


    //using the on GUI functions we will create & update the localized string 
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        //get properties we need
        SerializedProperty localizedStringProperty = property.FindPropertyRelative("localizedString");
        LocalizedText localizedText = (LocalizedText)localizedStringProperty.GetParent();

        //get localization item we need
        var table = LocalizationSettings.StringDatabase.GetTable("GENERATED", LocalizationSettings.ProjectLocale);
        var collection = LocalizationEditorSettings.GetStringTableCollection("GENERATED");

        //just make sure something ain't broken - found in testing that entries may still be found in a table in code - but not showing in editor
        table.CheckForMissingSharedTableDataEntries(MissingEntryAction.RemoveEntriesFromTable);

        //we need this to check the lozalized values in editor - LocalizedString will return null in editor
        StringTableEntry tableEntry = table.GetEntry(localizedText.id);


        //if empty, this is a new instance, create the table and add the key
        if (localizedText.localizedString.TableReference.ReferenceType == UnityEngine.Localization.Tables.TableReference.Type.Empty)
        {
            //create and add a string value as this is a new component
            string id = System.DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + keyIDIncrimentor.ToString();

            StringTableEntry newEntry = table.AddEntry(id, "STRING_EMPTY");
            localizedText.englishValue = "STRING_EMPTY";
            localizedText.localizedString = new LocalizedString("GENERATED", id);
            localizedText.id = newEntry.KeyId;
            keyIDIncrimentor++;

            EditorUtility.SetDirty(table);
            EditorUtility.SetDirty(table.SharedData);

            //updated this based on the new values
            tableEntry = table.GetEntry(localizedText.id);
        }
        else if(table.GetEntry(localizedText.localizedString.TableEntryReference.Key) == null)
        {
            //Code getting here means the localiazation has already been intialized but may have been modified else
            //i.e. it may have had the string key changed within a table or it has been removed

            //try to see if the entry exists using the long id and then it was just the string key that had been updated

            if (tableEntry != null)
            {
                //The value still exists - but the string key may have been changed just update the tableEntryReference to match the key
                localizedText.localizedString.TableEntryReference = tableEntry.Key;
                localizedText.englishValue = tableEntry.Value;
           
                Debug.LogWarning("Key readded to the generated localization table. This may have had it's human ID updated.");
            }
            else
            {
                //the key had been delated from the table and needs readded - do it this way and as we will be using the same key
                StringTableEntry newEntry = table.AddEntry(localizedText.localizedString.TableEntryReference.Key, "STRING_EMPTY");
                localizedText.englishValue = "STRING_EMPTY";
                localizedText.id = newEntry.KeyId;
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.SharedData);
                Debug.LogWarning("Key readded to the generated localization table. This may have previously been emptry or flagged as STRING_EMPTY when cleaning the table from the tool bar.");

                //updated this based on the new values
                tableEntry = table.GetEntry(localizedText.id);
            }
        }


        //Make sure the english value shows the localized text
        if (tableEntry != null && localizedText.englishValue != tableEntry.Value)
        {
            Debug.Log("MisMatch: " + localizedText.localizedString.GetLocalizedString());
            Debug.Log("MisMatch: " + localizedText.englishValue);
            localizedText.englishValue = tableEntry.Value;
        }
        else if(tableEntry == null)
        {
            Debug.LogError("Table Entry is null"); //it should never be null here
        }

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        float buttonWidth = 40;
        float margin = 5;

        // Calculate rects
        var stringRect = new Rect(position.x, position.y, position.width -(buttonWidth*2), position.height);
        var buttonRect = new Rect(position.x + position.width - (buttonWidth*2) + margin, position.y,  (buttonWidth)-margin, position.height);
        var newButtonRect = new Rect(position.x + position.width - buttonWidth + margin, position.y,  buttonWidth-margin, position.height);
            

         // Custom style - add a text area for the english value to be edited
         GUIStyle myStyle = new GUIStyle(EditorStyles.textArea);
        myStyle.fontSize = 14;
        myStyle.wordWrap = true;   

        EditorGUI.BeginChangeCheck();
        SerializedProperty myTextProperty = property.FindPropertyRelative("englishValue");
        myTextProperty.stringValue = EditorGUI.TextArea(stringRect, myTextProperty.stringValue, myStyle);
        
        //if the english value has changed
        if (EditorGUI.EndChangeCheck())
        {
            //update localization here            
            table.AddEntryFromReference(localizedText.localizedString.TableEntryReference, myTextProperty.stringValue);
            EditorUtility.SetDirty(table);
            EditorUtility.SetDirty(table.SharedData);
        }    

        //draw the buttons - this button opens up a window to edit the full localization settings
        if (GUI.Button(buttonRect, "?"))
        {
            LocalizationTextSettingsWindow.ShowWindow(property, label);
        }

        if (GUI.Button(newButtonRect, "New"))
        {
            //create and add a string value as this is a new component
            string id = System.DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + keyIDIncrimentor.ToString();

            StringTableEntry newEntry = table.AddEntry(id, "STRING_EMPTY");
            localizedText.englishValue = "STRING_EMPTY";
            localizedText.localizedString = new LocalizedString("GENERATED", id);
            localizedText.id = newEntry.KeyId;
            keyIDIncrimentor++;

            EditorUtility.SetDirty(table);
            EditorUtility.SetDirty(table.SharedData);
            
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {    
        GUIStyle myStyle = new GUIStyle(EditorStyles.textField);
        myStyle.fontSize = 14;
        myStyle.wordWrap = true;        
        SerializedProperty myTextProperty = property.FindPropertyRelative("englishValue");
        GUIContent guiContent = new GUIContent(myTextProperty.stringValue);
        float height = myStyle.CalcHeight(guiContent, 200);
        
        return height;
    }    
}

public class LocalizationTextSettingsWindow : EditorWindow
{
    SerializedProperty incomingProperty = null;
    GUIContent incomingLabel = null;


    public static void ShowWindow(SerializedProperty incomingProperty, GUIContent incomingLabel)
    {
        //Show existing window instance. If one doesn't exist, make one.
        LocalizationTextSettingsWindow window = (LocalizationTextSettingsWindow)EditorWindow.GetWindow(typeof(LocalizationTextSettingsWindow));
        window.incomingProperty = incomingProperty;
        window.incomingLabel = incomingLabel;
    }

    void OnGUI()
    {
       
        GUILayout.Label(incomingProperty.displayName, EditorStyles.boldLabel);
        //myString = EditorGUILayout.TextField("Text Field", myString);     

        EditorGUI.BeginChangeCheck();
        SerializedProperty localizedStringProperty = incomingProperty.FindPropertyRelative("localizedString");        

        EditorGUILayout.PropertyField(localizedStringProperty);
        if (EditorGUI.EndChangeCheck())
        {
            localizedStringProperty.serializedObject.ApplyModifiedProperties();
            //SerializedProperty myTextProperty = incomingProperty.FindPropertyRelative("englishValue");
            var table = LocalizationSettings.StringDatabase.GetTable("GENERATED", LocalizationSettings.ProjectLocale);

            LocalizedText textObject = (LocalizedText)localizedStringProperty.GetParent();
            textObject.englishValue = "THIS CHANGED";
            var entry = table.GetEntryFromReference(textObject.localizedString.TableEntryReference);
            textObject.id = entry.KeyId;
        }
        
    }
}
