using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    ExitGames.Client.Photon.Hashtable playerTime = new ExitGames.Client.Photon.Hashtable();

    GameObject player;
    PlayerMovement playerMovement;
    PlayerManager playerManager;

    public GameObject[] playerPrefabs;
    public Transform[] spawnpoints;
    Transform spawnPos;

    public Timer timer;
    public Text countdownText;

    public GameObject pausePanel;

    public GameObject resultsPanel;
    public Text finalTimeText;
    public GameObject nextRaceButton;
    public GameObject mapSelectionPanel;

    public Text winLossText;
    public GameObject opponentTimeLabel;
    public Text opponentTimeText;

    public Text lapText;
    public Text speedText;

    public Image[] abilityDisplay;

    int currentLap = 1;
    int readyPlayers;
    bool playerFinishedLap;
    float opponentTime;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (!PhotonNetwork.IsConnected)
        {
            OfflineSpawn();
            StartCoroutine(RaceCountdown());
        }
        else
        {
            OnlineSpawn();
        }

        playerMovement = player.GetComponent<PlayerMovement>();
        playerManager = player.GetComponent<PlayerManager>();
    }

    private void Update()
    {
        if (readyPlayers == 2 && PhotonNetwork.IsMasterClient)
        {
            readyPlayers = 0;
            this.photonView.RPC("StartCountdown", RpcTarget.AllViaServer);
        }

        lapText.text = currentLap + "/3";
        speedText.text = Mathf.RoundToInt(playerMovement.currentSpeed).ToString();

        for (int i = 0; i < playerMovement.abilityCount; i++)
        {
            abilityDisplay[i].enabled = true;
        }

        for (int i = playerMovement.abilityCount; i < abilityDisplay.Length; i++)
        {
            abilityDisplay[i].enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(PhotonNetwork.IsConnected)
            {
                return;
            }

            pausePanel.SetActive(!pausePanel.activeInHierarchy);
            Cursor.lockState = pausePanel.activeInHierarchy ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    new private void OnEnable()
    {
        PlayerManager.OnFinishLap += FinishLap;
    }

    new private void OnDisable()
    {
        PlayerManager.OnFinishLap -= FinishLap;
    }

    void OfflineSpawn()
    {
        spawnPos = spawnpoints[0];
        player = Instantiate(playerPrefabs[PlayerPrefs.GetInt("characterIndex")], spawnPos.position, spawnPos.rotation);
    }

    void OnlineSpawn()
    {
        this.photonView.RPC("Ready", RpcTarget.All);
        PhotonNetwork.AddCallbackTarget(this);

        spawnPos = spawnpoints[PhotonNetwork.IsMasterClient ? 0 : 1];
        player = PhotonNetwork.Instantiate(playerPrefabs[PlayerPrefs.GetInt("characterIndex")].name, spawnPos.position, spawnPos.rotation);

        if(!PhotonNetwork.IsMasterClient)
        {
            nextRaceButton.SetActive(false);
        }

        winLossText.gameObject.SetActive(true);
    }

    void FinishLap()
    {
        if (currentLap < 3)
        {
            currentLap++;
            player.transform.position = spawnPos.position;

            if (playerFinishedLap)
            {
                playerMovement.AddAbility();
            }

            if (PhotonNetwork.IsConnected)
            {
                this.photonView.RPC("PlayerFinishedLap", RpcTarget.All);
            }
        }
        else
        {
            FinishRace();
        }
    }

    [PunRPC]
    void PlayerFinishedLap()
    {
        playerFinishedLap = !playerFinishedLap;
    }

    void FinishRace()
    {
        timer.timerIsRunning = false;
        Cursor.lockState = CursorLockMode.None;

        resultsPanel.SetActive(true);
        playerMovement.canMove = false;

        playerTime["time"] = timer.time;
        PhotonNetwork.SetPlayerCustomProperties(playerTime);

        DisplayTime(timer.time, finalTimeText);

        if (opponentTime != 0f)
        {
            DisplayWinner();
        }
        else
        {
            winLossText.text = "VICTORY!";
        }
    }

    void DisplayTime(float time, Text text)
    {
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);
        float milliSeconds = (time % 1) * 1000;
        text.text = string.Format("{0:0}:{1:00}.{2:000}", minutes, seconds, milliSeconds);
    }

    void DisplayWinner()
    {
        if(timer.time < opponentTime)
        {
            winLossText.text = "VICTORY!";
        }
        else
        {
            winLossText.text = "DEFEAT!";
        }
    }

    public void OnClickReset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnClickNextRace()
    {
        mapSelectionPanel.SetActive(true);
    }

    public void OnClickQuit()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("Lobby");
        }
        else
        {
            SceneManager.LoadScene("Main Menu");
        }
    }

    [PunRPC]
    void Ready()
    {
        readyPlayers++;
    }

    [PunRPC]
    void StartCountdown()
    {
        StartCoroutine(RaceCountdown());
    }

    IEnumerator RaceCountdown()
    {
        yield return new WaitForSeconds(1.5f);
        countdownText.gameObject.SetActive(true);
        countdownText.text = "3";
        yield return new WaitForSeconds(1f);
        countdownText.text = "2";
        yield return new WaitForSeconds(1f);
        countdownText.text = "1";
        yield return new WaitForSeconds(1f);
        countdownText.text = "GO!";

        StartRace();
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }

    void StartRace()
    {
        playerMovement.canMove = true;
        timer.timerIsRunning = true;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer != PhotonNetwork.LocalPlayer)
        {
            opponentTime = (float)targetPlayer.CustomProperties["time"];

            if (opponentTimeText != null)
            {
                opponentTimeLabel.SetActive(true);
                opponentTimeText.gameObject.SetActive(true);
                DisplayTime(opponentTime, opponentTimeText);
                DisplayWinner();
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Lobby");
    }
}

