using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public GameObject settingsPanel;
    public Toggle autoRunToggle;

    private void Start()
    {
        autoRunToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("autoRun") == 1 ? true : false);
    }

    public void OnClickSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void OnClickBack()
    {
        settingsPanel.SetActive(false);
    }

    public void OnToggleAutoRun()
    {
        PlayerPrefs.SetInt("autoRun", PlayerPrefs.GetInt("autoRun") == 1 ? 0 : 1);
    }
}
