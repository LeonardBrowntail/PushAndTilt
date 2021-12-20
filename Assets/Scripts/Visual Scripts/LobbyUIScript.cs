using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Mirror;

public class LobbyUIScript : MonoBehaviour
{
    private GameManager gameManager;
    private MyNetworkManager netManager;
    private LobbyManager manager;

    [SerializeField] private GameObject playerJoinPrefab;
    [SerializeField] private GameObject errorLabelPrefab;
    [SerializeField] private TextMeshProUGUI hostNameLabel;
    [SerializeField] private TextMeshProUGUI statusLabel;
    [SerializeField] private TextMeshProUGUI countDownNumberLabel;

    public void UI_ShowJoined(string name)
    {
        var temp = Instantiate(playerJoinPrefab, gameObject.transform);
        temp.GetComponent<TextMeshProUGUI>().text = $"{name} has joined the game!";
    }

    public void UI_ShowError(string msg)
    {
        var label = Instantiate(errorLabelPrefab, gameObject.transform);
        label.GetComponent<TextMeshProUGUI>().text = msg;
    }

    public void Button_SetReady()
    {
        manager.CmdSetPlayerReadyState(!manager.LocalReady);
    }

    public void CW_numUpdate(float time)
    {
        countDownNumberLabel.text = time.ToString("0.0");
    }

    public void CW_numEmpty()
    {
        countDownNumberLabel.text = string.Empty;
    }

    private void Awake()
    {
        if (!(netManager = FindObjectOfType<MyNetworkManager>()))
        {
            Debug.LogError($"{ToString()}: netManager not found");
        }
        if (!(gameManager = FindObjectOfType<GameManager>()))
        {
            Debug.LogError($"{ToString()}: gameManager not found");
        }
        if (!(manager = FindObjectOfType<LobbyManager>()))
        {
            Debug.LogError($"{ToString()}: lobbyManager not found");
        }
    }

    private void Start()
    {
        hostNameLabel.text = "Host Name: " + netManager.HostName;
        statusLabel.text = "Waiting for players...";
        CW_numEmpty();
    }
}