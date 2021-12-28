using TMPro;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Net;

public class ServerBrowserScript : MonoBehaviour
{
    private MainMenu menu;
    [SerializeField] private GameObject directConnectUI;
    [SerializeField] private GameObject directConnectButton;
    [SerializeField] private GameObject buttonTemplate;
    private Dictionary<string, GameObject> registered;

    [HideInInspector] public ServerBrowserButtonScript CurrentSelected;

    /// <summary>
    /// Button function that starts a client
    /// </summary>
    public void StartClient()
    {
        var clientField = GameObject.FindGameObjectWithTag("clientNameField").GetComponent<TMP_InputField>();
        if (clientField.text.Length > 1)
        {
            if (CurrentSelected)
            {
                var netManager = FindObjectOfType<MyNetworkManager>();
                netManager.networkAddress = CurrentSelected.Address;
                netManager.HostName = CurrentSelected.HostName;
                netManager.LocalPlayerName = clientField.text;
                netManager.StartClient();
            }
            else
            {
                Debug.Log("ServerBrowserScript.cs/StartClient(): nothing is currently selected!");
            }
        }
        else
        {
            Debug.Log("ServerBrowserScript.cs/StartClient(): client name is not set!");
            clientField.transform.parent.gameObject.GetComponent<Animator>().Play("Flash");
        }
    }

    /// <summary>
    /// Button function that starts a host
    /// </summary>
    public void StartHost()
    {
        var clientField = GameObject.FindGameObjectWithTag("clientNameField").GetComponent<TMP_InputField>();
        var serverField = GameObject.FindGameObjectWithTag("hostNameField").GetComponent<TMP_InputField>();
        if (clientField.text.Length > 0)
        {
            if (serverField.text.Length > 0)
            {
                if (!NetworkServer.active && !NetworkClient.isConnected)
                {
                    var netManager = GameObject.FindGameObjectWithTag("networkManager").GetComponent<MyNetworkManager>();
                    if (netManager)
                    {
                        netManager.gameObject.GetComponent<MyNetworkDiscovery>().StopDiscovery();
                        netManager.HostName = GameObject.FindGameObjectWithTag("hostNameField").GetComponent<TMP_InputField>().text;
                        netManager.LocalPlayerName = clientField.text;
                        netManager.StartHost();
                    }
                    else
                    {
                        Debug.LogError("ServerBrowserScript.cs/StartHost(): network manager is not found!");
                    }
                }
            }
            else
                serverField.transform.parent.gameObject.GetComponent<Animator>().Play("Flash");
        }
        else
        {
            Debug.Log("ServerBrowserScript.cs/StartHost(): client name is not set!");
            clientField.transform.parent.gameObject.GetComponent<Animator>().Play("Flash");
        }
    }

    /// <summary>
    /// Sets a new server entry in the server browser
    /// </summary>
    /// <param name="response">response data which contains hostname and player count</param>
    /// <param name="address">response sender's address</param>
    public void SetData(DiscoveryResponse response, string address)
    {
        if (!registered.ContainsKey(address))
        {
            var content = GetComponent<ScrollRect>().content.transform;
            var button = Instantiate(buttonTemplate, content);
            button.GetComponent<ServerBrowserButtonScript>().SetButtonInfo(response.hostName, address, response.connectedCount);
            registered.Add(address, button.gameObject);
        }
        else
        {
            registered.TryGetValue(address, out GameObject button);
            button.GetComponent<ServerBrowserButtonScript>().UpdatePlayerCount(response.connectedCount);
        }
    }

    /// <summary>
    /// Clears old entries
    /// </summary>
    public void ClearBrowserList()
    {
        registered = new Dictionary<string, GameObject>();
        var content = GetComponent<ScrollRect>().content.transform;
        if (content.childCount != 0)
        {
            for (int i = content.childCount; i > 0; i--)
            {
                Destroy(content.GetChild(i - 1).gameObject);
            }
        }
    }

    public void UI_ShowDirectConnect()
    {
        if (!directConnectUI.activeSelf)
        {
            directConnectUI.SetActive(true);
            FindObjectOfType<MainMenu>().directConnectActive = true;
        }
    }

    public void UI_HideDirectConnect()
    {
        if (directConnectUI.activeSelf)
        {
            directConnectUI.SetActive(false);
        }
    }

    public void UI_DirectConnect()
    {
        var address = "20.124.130.179";
        IPAddress ip;
        if (IPAddress.TryParse(address, out ip))
        {
            var clientField = GameObject.FindGameObjectWithTag("clientNameField").GetComponent<TMP_InputField>();
            if (clientField.text.Length > 1)
            {
                var netManager = FindObjectOfType<MyNetworkManager>();
                netManager.networkAddress = address;
                netManager.LocalPlayerName = clientField.text;
                netManager.StartClient();
            }
            else
            {
                Debug.Log("ServerBrowserScript.cs/StartClient(): client name is not set!");
                clientField.transform.parent.gameObject.GetComponent<Animator>().Play("Flash");
            }
        }
        else
        {
            Debug.LogWarning($"{ToString()}: address is not valid");
        }
    }

    private void Update()
    {
        if (directConnectUI.activeSelf)
        {
            if (NetworkClient.isConnecting)
            {
                if (directConnectButton.GetComponent<Button>().interactable)
                {
                    directConnectButton.GetComponent<Button>().interactable = false;
                    directConnectButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Connecting...";
                }
            }
            else
            {
                if (!directConnectButton.GetComponent<Button>().interactable)
                {
                    directConnectButton.GetComponent<Button>().interactable = true;
                    directConnectButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Connect";
                }
            }
        }
    }

    private void Start()
    {
        if (!(menu = FindObjectOfType<MainMenu>()))
        {
            Debug.LogError($"{ToString()}: menu not found");
        }
        ClearBrowserList();
        FindObjectOfType<MyNetworkDiscovery>().StartDiscovery();
    }
}