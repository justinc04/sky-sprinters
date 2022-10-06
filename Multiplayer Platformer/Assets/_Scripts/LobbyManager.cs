using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public Button createButton;
    public Button backButton;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public GameObject background;

    public RoomItem roomItemPrefab;
    List<RoomItem> roomItemsList = new List<RoomItem>();
    public Transform roomItemParent;

    public float timeBetweenUpdates = 1f;
    float nextUpdateTime;

    private void Start()
    {
        PhotonNetwork.JoinLobby();
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnClickCreate()
    {
        PhotonNetwork.CreateRoom(PhotonNetwork.NickName, new RoomOptions() { MaxPlayers = 2, BroadcastPropsChangeToAll = true });
        createButton.interactable = false;
        backButton.interactable = false;
    }

    public void OnClickBack()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Main Menu");
    }

    public override void OnJoinedLobby()
    {
        createButton.interactable = true;
        backButton.interactable = true;
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        background.SetActive(false);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (Time.time >= nextUpdateTime)
        { 
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }
    }

    void UpdateRoomList(List<RoomInfo> list)
    {
        foreach(RoomItem item in roomItemsList)
        {
            Destroy(item.gameObject);
        }
        roomItemsList.Clear();

        foreach(RoomInfo room in list)
        {
            if(room.RemovedFromList)
            {
                return;
            }

            RoomItem newRoom = Instantiate(roomItemPrefab, roomItemParent);
            newRoom.SetRoomName(room.Name);
            roomItemsList.Add(newRoom);
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnLeftRoom()
    {
        lobbyPanel.SetActive(true);
        background.SetActive(true);
    }
}