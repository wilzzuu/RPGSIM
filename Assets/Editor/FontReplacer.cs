using UnityEditor;
using UnityEngine;
using TMPro; // For TextMeshPro support
using UnityEngine.UI;

public class FontReplacer : EditorWindow
{
    private Font newFont; // Standard Unity font
    private TMP_FontAsset newTMPFont; // TextMeshPro font

    [MenuItem("Tools/Replace Fonts")]
    public static void ShowWindow()
    {
        GetWindow<FontReplacer>("Replace Fonts");
    }

    void OnGUI()
    {
        GUILayout.Label("Replace Fonts in UI", EditorStyles.boldLabel);

        newFont = (Font)EditorGUILayout.ObjectField("New Font (Text)", newFont, typeof(Font), false);
        newTMPFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New Font (TMP)", newTMPFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Replace Fonts"))
        {
            ReplaceFontsInScene();
        }
    }

    private void ReplaceFontsInScene()
    {
        // Replace Unity UI Text fonts
        Text[] textComponents = FindObjectsByType<Text>(FindObjectsSortMode.None);
        foreach (Text text in textComponents)
        {
            Undo.RecordObject(text, "Replace Font");
            text.font = newFont;
        }

        // Replace TextMeshPro fonts
        TMP_Text[] tmpTextComponents = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        foreach (TMP_Text tmpText in tmpTextComponents)
        {
            Undo.RecordObject(tmpText, "Replace TMP Font");
            tmpText.font = newTMPFont;
        }

        Debug.Log("Font replacement complete!");
    }
}
