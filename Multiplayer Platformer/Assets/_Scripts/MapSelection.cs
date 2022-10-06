using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MapSelection : MonoBehaviourPun
{
    int mapNumber;

    public void OnClickMap(int _mapNumber)
    {
        mapNumber = _mapNumber;

        if (PhotonNetwork.IsConnected)
        {
            if (("Map " + mapNumber).Equals(SceneManager.GetActiveScene().name))
            {
                this.photonView.RPC("NextRace", RpcTarget.All);
            }
            else
            {
                PhotonNetwork.LoadLevel("Map " + mapNumber);
            }
        }
        else
        {
            SceneManager.LoadScene("Map " + mapNumber);
        }
    }

    [PunRPC]
    void NextRace()
    {
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
    }
}
