using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class gameManager : MonoBehaviourPunCallbacks
{
    public List<Transform> spawnPoints;
    private int spawnpointIndex;
    public GameObject plprefab;
    [SerializeField]
    private TextMeshProUGUI textRef;
    [SerializeField]
    private GameObject panel;
    public TextMeshProUGUI WinnerName;
    public GameObject namePanel;
    private float timeCount = 1;
    public Button startButon;
    public bool move = false;
    public static gameManager instance;
    private bool startCountdown = false;
    void Start()
    {
        instance = this;
        namePanel.SetActive(false);
        panel.SetActive(true);
        spawnpointIndex = Random.Range(0, spawnPoints.Count - 1);
        PhotonNetwork.Instantiate(plprefab.name, spawnPoints[spawnpointIndex].transform.position, Quaternion.identity);
        spawnPoints.RemoveAt(spawnpointIndex);
        startButon.interactable = false;
        textRef.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (PhotonNetwork.CountOfPlayers > 1)
        {
            startButon.interactable = true;
        }
        if(startCountdown)
        {
            StartCountDown();
        }
    }
    public void startCount()
    {
        startCountdown = true;
    }
    public void StartCountDown()
    {
        timeCount += 1*Time.deltaTime;
        textRef.gameObject.SetActive(true);
        textRef.text = timeCount.ToString("0");
        if (timeCount > 3)
        {
            panel.SetActive(false);
            move = true;
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false; // Prevent new players from joining
            }
            startCountdown = false;
        }
        
    }
    public void Restart()
    {
        SceneManager.LoadScene("Lobby");
    }
}
