using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndGameUIScript : MonoBehaviour
{
    private MyNetworkManager netManager;
    [SerializeField] private GameObject bnwVfx;
    [SerializeField] private GameObject winObj;
    [SerializeField] private GameObject loseObj;
    [SerializeField] private TextMeshProUGUI endText;
    [SerializeField] private GameObject endGameButton;

    public void ShowWin(string name)
    {
        winObj.gameObject.SetActive(true);
        endText.gameObject.SetActive(true);
        if (!endGameButton.activeSelf)
            endGameButton.SetActive(true);
        endText.text = $"{name} have won the game!";
    }

    public void ShowLose(string name)
    {
        loseObj.gameObject.SetActive(true);
        endText.gameObject.SetActive(true);
        endGameButton.SetActive(true);
        bnwVfx.SetActive(true);
        endText.text = $"Sorry {name}, better luck next time!";
    }

    public void UI_ExitGame()
    {
        netManager.Disconnect();
    }

    private void Start()
    {
        if (!(netManager = FindObjectOfType<MyNetworkManager>()))
        {
            Debug.LogError($"{ToString()}: netManager not found");
        }
    }
}