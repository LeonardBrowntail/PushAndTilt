using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using TMPro;
public class UITest
{
    //Load the scene to test
    [OneTimeSetUp]
    public void setUp()
    {
        SceneManager.LoadScene("Menu");
    }

    //UI Test Scenario
    //1. Click Play Button
    //2. Start a game as host
    //3. Player spawned
    [UnityTest]
    public IEnumerator UITestWithEnumeratorPasses()
    {
        //1.
        var playButtonObject = GameObject.Find("Play Button");
        var playButton = playButtonObject.GetComponent<Button>();
        playButton.onClick.Invoke();
        yield return null;

        //2.
        var hostOptionHostName = GameObject.Find("HostOptionHostName_");
        var hostOptionHostNameText = hostOptionHostName.GetComponentInChildren<TextMeshProUGUI>();
        hostOptionHostNameText.text = "dummy";
        Assert.AreEqual("dummy", hostOptionHostNameText.text);
        var startGameButtonObject = GameObject.Find("HostOption_startHostButton");
        var startGameButton = startGameButtonObject.GetComponent<Button>();
        startGameButton.onClick.Invoke();
        yield return new WaitForSeconds(3);

        //3.
        var player = GameObject.Find("Player [connId=0]");
        Assert.IsNotNull(player);
    }
}
