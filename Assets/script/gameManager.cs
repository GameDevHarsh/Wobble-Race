using Photon.Pun;
using Photon.Realtime;
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
    [SerializeField] private TextMeshProUGUI textRef;
    [SerializeField] private GameObject panel;
    public TextMeshProUGUI WinnerName;
    public GameObject namePanel;
    private float timeCount = 0;
    public Button startButton;
    public bool move = false;
    public static gameManager instance;
    private bool startCountdown = false;
    PhotonView photonView;
    private bool cursorLocked = true; // Track cursor state
    private GameObject player;
    private PlayerController controller;
    void Start()
    {
        instance = this;
        namePanel.SetActive(false);
        panel.SetActive(true);
        startButton.interactable = false;
        textRef.gameObject.SetActive(false);

        // Spawn player at a random spawn point
        spawnpointIndex = Random.Range(0, spawnPoints.Count);
        player=PhotonNetwork.Instantiate(plprefab.name, spawnPoints[spawnpointIndex].position, Quaternion.identity);
        controller=player.GetComponentInChildren<PlayerController>();
        photonView=GetComponent <PhotonView>();
        spawnPoints.RemoveAt(spawnpointIndex);
    }

    private void Update()
    {
        startButton.interactable = PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1;

        if (startCountdown)
        {
            timeCount += Time.deltaTime;
            textRef.gameObject.SetActive(true);
            textRef.text = timeCount.ToString("0");

            if (timeCount > 3)
            {
                // Call RPC to disable the panel and allow movement for all players
                photonView.RPC("DisablePanelForAll", RpcTarget.All);

                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false; // Prevent new players from joining
                }

                startCountdown = false;
            }
        }
        HandleCursorLock();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        startButton.interactable = PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        startButton.interactable = PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1;
    }

    public void StartCount()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Start countdown for all players
            photonView.RPC("StartCountdownRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    private void StartCountdownRPC()
    {
        startCountdown = true;
        timeCount = 1;  // Reset the countdown
        textRef.gameObject.SetActive(true);
    }

    [PunRPC]
    private void DisablePanelForAll()
    {
        panel.SetActive(false);
        move = true;
        cursorLocked = false;
    }

    public void Restart()
    {
        SceneManager.LoadScene("Lobby");
    }
    private void HandleCursorLock()
    {
        // Lock the cursor when clicking on the game
        if (Input.GetMouseButtonDown(0) && !cursorLocked)
        {
            LockCursor();
        }

        // Unlock the cursor when pressing Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        controller.curserlocked = true;
        cursorLocked = true;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        controller.curserlocked = false;
        cursorLocked = false;
    }
    private void OnApplicationQuit()
    {
        Destroy(player);
    }
}
