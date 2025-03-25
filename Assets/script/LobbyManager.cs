using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField cr;
    public TMP_InputField jr;
    public GameObject namePanel;
    public GameObject LobbyPanel;
    public TMP_InputField NameIn;
    private void Start()
    {
        namePanel.SetActive(true);
        LobbyPanel.SetActive(false);
    }
    public void nextButton()
    {
        PhotonNetwork.NickName = NameIn.text;
        namePanel.SetActive(false);
        LobbyPanel.SetActive(true);
    }
    public void createRoom()
    {
        PhotonNetwork.CreateRoom(cr.text);
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(jr.text);
    }
  public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Game");
    }
    public void Exit()
    {
        Application.Quit();
    }
}
