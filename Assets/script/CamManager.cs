using Photon.Pun;
using UnityEngine;

public class CamManager : MonoBehaviourPunCallbacks
{
    void Update()   
    {
        if (!photonView.IsMine)
        {
            gameObject.SetActive(false);
        }
    }
}
