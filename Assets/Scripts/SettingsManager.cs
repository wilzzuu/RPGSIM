using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public SceneSwitch sceneSwitch;

    public void SettingsResetProgress()
    {
        PlayerManager.Instance.ResetProgress();
        SceneManager.LoadScene(1);
    }
}
