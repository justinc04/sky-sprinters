using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CharacterSelection : MonoBehaviourPunCallbacks
{
    public GameObject[] characters;
    public int characterIndex;
    GameObject character;

    public Button leftButton;
    public Button rightButton;

    public GameObject characterSelectionPanel;
    public GameObject mapSelectionPanel;

    [Header("Online")]
    public GameObject readyButton;
    public GameObject waitingText;

    int readyPlayers = 0;
    bool canLoadLevel = true;

    private void Start()
    {
        characterIndex = PlayerPrefs.GetInt("characterIndex");
        character = Instantiate(characters[characterIndex], new Vector3(0f, 1.2f, 0f), Quaternion.identity);
    }

    private void Update()
    {
        if(canLoadLevel && readyPlayers == 2)
        {
            canLoadLevel = false;
            mapSelectionPanel.SetActive(true);
        }
    }

    public void OnClickLeft()
    {
        if(characterIndex == 0)
        {
            characterIndex = characters.Length - 1;
        }
        else
        {
            characterIndex--;
        }

        Destroy(character);
        character = Instantiate(characters[characterIndex], new Vector3(0f, 1.2f, 0f), Quaternion.identity);
    }

    public void OnClickRight()
    {
        if (characterIndex == characters.Length - 1)
        {
            characterIndex = 0;
        }
        else
        {
            characterIndex++;
        }

        Destroy(character);
        character = Instantiate(characters[characterIndex], new Vector3(0f, 1.2f, 0f), Quaternion.identity);
    }

    public void OnClickConfirm()
    {
        PlayerPrefs.SetInt("characterIndex", characterIndex);
        leftButton.interactable = false;
        rightButton.interactable = false;

        if (PhotonNetwork.IsConnected)
        {
            readyButton.SetActive(false);
            waitingText.SetActive(true);
            this.photonView.RPC("Ready", RpcTarget.MasterClient);          
        }
        else
        {
            mapSelectionPanel.SetActive(true);
        }
    }

    public void OnClickBack()
    {
        leftButton.interactable = true;
        rightButton.interactable = true;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            characterSelectionPanel.SetActive(false);
        }
    }

    public override void OnLeftRoom()
    {
        readyPlayers = 0;

        readyButton.SetActive(true);
        waitingText.SetActive(false);
        characterSelectionPanel.SetActive(false);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        readyPlayers = 0;

        leftButton.interactable = true;
        rightButton.interactable = true;
        readyButton.SetActive(true);
        waitingText.SetActive(false);
    }

    [PunRPC]
    void Ready()
    {
        readyPlayers++;
    }
}
