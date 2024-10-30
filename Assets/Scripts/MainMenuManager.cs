using UnityEngine;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        string path = Path.Combine(Application.persistentDataPath, "SaveData");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}