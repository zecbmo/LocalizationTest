using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using TMPro;

//update a text mesh pro object from a LocalizedText
[RequireComponent(typeof(TMP_Text))]
public class TextMeshProLocalizer : MonoBehaviour
{
    [SerializeField]
    LocalizedText localizedText = null;
    
    private void Awake()
    {
        TMP_Text text = GetComponent<TMP_Text>();

        if (text == null)
        {
            Debug.LogError("This text object should have TMP_Text item attacher: " + gameObject.name);
        }
        else
        {
            text.text = localizedText.GetValidatedLocalisedText();
        }
    }

#if UNITY_EDITOR
    [Button("Set english text from text object")]
    public void SetEnglishTextFromTextObject()
    {
        TMP_Text text = GetComponent<TMP_Text>();
        localizedText.SetLocalizedStringEnglishValue(text.text);
    }
#endif


}
