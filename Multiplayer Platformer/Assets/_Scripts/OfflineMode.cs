using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfflineMode : MonoBehaviour
{
    public GameObject selectionPanel;
    public GameObject mainPanel;

    public void OnClickTimeTrials()
    {
        selectionPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void OnClickBack()
    {
        mainPanel.SetActive(true);
    }
}
