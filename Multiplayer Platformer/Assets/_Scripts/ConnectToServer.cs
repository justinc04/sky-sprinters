using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public InputField usernameInput;
    public GameObject connectPanel;
    public Button connectButton;
    public Button backButton;
    public Text buttonText;

    public void OnClickRace()
    {
        connectPanel.SetActive(true);
        usernameInput.text = PlayerPrefs.GetString("playerName");
    }

    public void OnClickBack()
    {
        connectPanel.SetActive(false);
    }

    public void OnClickConnect()
    {
        if(usernameInput.text.Length >= 1)
        {
            PlayerPrefs.SetString("playerName", usernameInput.text);
            PhotonNetwork.NickName = usernameInput.text;
            connectButton.interactable = false;
            backButton.interactable = false;
            buttonText.text = "Connecting...";
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("Lobby");
    }
}
