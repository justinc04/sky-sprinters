using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public delegate void FinishRace();
    public static event FinishRace OnFinishLap;

    public Animator animator;
    public GameObject minimapIndicator;
    public GameObject selfIndicator;

    public bool finishedLap = false;
    int currentLap = 1;

    private void Start()
    {
        minimapIndicator.SetActive(true);

        if(PhotonNetwork.IsConnected && photonView.IsMine)
        {
            selfIndicator.SetActive(true);
        }
    }

    void Update()
    {
        if (!finishedLap)
        {
            return;
        }

        finishedLap = false;
        currentLap++;    

        if (OnFinishLap != null)
        {
            OnFinishLap();
        }

        if (currentLap > 3)
        {
            animator.SetBool("running", false);
            animator.SetBool("jumping", false);
        }
    }
}
