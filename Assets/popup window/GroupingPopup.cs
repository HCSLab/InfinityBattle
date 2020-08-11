using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Net;
using System.IO;
using System.Text;

public class GroupingInput : MonoBehaviour
{
    // 200x300 px window will apear in the center of the screen.
    private Rect windowRect = new Rect((Screen.width - 300) / 2, (Screen.height - 180) / 2, 300, 180);
    // Only show it if needed.
    private bool show = false;

    private string title, text, ok;

    void OnGUI()
    {
        if (show)
        {
            
            windowRect = GUI.Window(20, windowRect, DialogWindow, "PAIRUP");
            GUI.FocusWindow(20);
        }
            
    }

    int netport;
    P2PNetworkManager manager;
    Coroutine matchmaking = null;
    private void init()
    {
        Debug.Log("Now config for network component...");
        netport = GameObject.Find("PythonController").GetComponent<PythonController>().port;
        manager = GameObject.Find("NetworkManager").GetComponent<P2PNetworkManager>();

        manager.nickname = GameObject.Find("PythonController").GetComponent<PythonController>().username;
        manager.port = netport;

        string content = string.Empty;
        string url = @"http://127.0.0.1:" + (netport + 1) + "/pubkey";
        //Debug.Log(url);
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip;
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            content = reader.ReadToEnd();
        }
        manager.PubKey = Encoding.ASCII.GetBytes(content);
    }

    private string getTeammate()
    {
        string content = string.Empty;
        string url = @"http://127.0.0.1:" + (netport + 1) + "/teammate";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip;
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            content = reader.ReadToEnd();
        }

        return content;
    }

    private string getOpponent()
    {
        string content = string.Empty;
        string url = @"http://127.0.0.1:" + (netport + 1) + "/opponent";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip;
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            content = reader.ReadToEnd();
        }

        return content;
    }

    public string opponent, teammate;
    private IEnumerator MatchMaking()
    {
        // solo
        string content = string.Empty;
        string url = @"http://127.0.0.1:" + (netport + 1) + "/startgame";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip;
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            content = reader.ReadToEnd();
        }

        while (true)
        {
            if (manager.FinishTeamUp)
            {
                break;
            }

            teammate = getTeammate();
            Debug.Log(teammate);
            if (string.Compare(teammate, "") != 0)
            {
                Debug.Log("A connection find");
                manager.Terminate();
                manager.Run(teammate.Split(':')[0], int.Parse(teammate.Split(':')[1]) - 1);
                break;
            }
            yield return new WaitForSeconds(1);
            if (manager.IsOccupied == false)
            {
                manager.Run();
            }
        }

        while (true)
        {
            if (manager.IsMatching)
            {
                break;
            }

            opponent = getOpponent();
            Debug.Log(opponent);
            if (string.Compare(opponent, "") != 0)
            {
                Debug.Log("Opponent is found");
                yield return new WaitForSeconds(5);
                manager.SendBattleRequest(opponent.Split(':')[0], int.Parse(opponent.Split(':')[1]) - 1);
                break;
            }
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator MatchMaking(bool teamforming)
    {
        if (teamforming == false)
        {
            yield return MatchMaking();
        }
        // group
        string content = string.Empty;
        string url = @"http://127.0.0.1:" + (netport + 1) + "/startgames";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip;
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            content = reader.ReadToEnd();
        }

        while (true)
        {
            opponent = getOpponent();
            Debug.Log(opponent);
            if (string.Compare(opponent, "") != 0)
            {
                Debug.Log("Opponent is found");
                yield return new WaitForSeconds(5);
                if (manager.IsMatching)
                {
                    break;
                }
                manager.SendBattleRequest(opponent.Split(':')[0], int.Parse(opponent.Split(':')[1]) - 1);
                break;
            }
            yield return new WaitForSeconds(1);
        }
    }

    string ip = null, port = null;
    // This is the actual window.
    void DialogWindow(int windowID)
    {
        GUI.Label(new Rect(15, 25, windowRect.width, 20), "Dest. IP Address: ");
        ip = GUI.TextField(new Rect(130, 25, 150, 20), ip);
        GUI.Label(new Rect(15, 55, windowRect.width, 20), "Dest. Port: ");
        port = GUI.TextField(new Rect(130, 55, 150, 20), port);
        string text = "IP Address: " +
            GameObject.Find("NetworkManager").GetComponent<P2PNetworkManager>().GetIpAddress() + "\tPort: " +
            GameObject.Find("PythonController").GetComponent<PythonController>().port;
        GUI.Label(new Rect(15, 85, windowRect.width, 20), text);

        if (GUI.Button(new Rect(15, 110, 120, 20), "Solo"))
        {
            Debug.Log("Play solo");
            GameObject.Find("Button_BattleMode").GetComponentInChildren<Text>().text = "Searching for New Game...";
            GameObject.Find("Button_BattleMode").GetComponentInChildren<Button>().interactable = false;
            init();
            show = false;
            manager.Run();
            matchmaking = StartCoroutine(MatchMaking());
        }
        if (GUI.Button(new Rect(160, 110, 120, 20), "Group"))
        {
            Debug.Log("Play group");
            init();
            show = false;
            if (ip != null && ip != "" && port != null && port != "")
            {
                manager.Run(ip, int.Parse(port));
                GameObject.Find("Button_BattleMode").GetComponentInChildren<Text>().text = "Pairing...";
                GameObject.Find("Button_BattleMode").GetComponentInChildren<Button>().interactable = false;
            }
            else
            {
                Debug.Log("Waiting for teammates");
                manager.Run();
                GameObject.Find("Button_BattleMode").GetComponentInChildren<Text>().text = "Exit grouping...";
            }
        }
        if (GUI.Button(new Rect(15, 140, 265, 20), "Cancel"))
        {
            //Application.Quit();
            show = false;
            GameObject.Find("Button_BattleMode").GetComponentInChildren<Text>().text = "Play!";
        }
    }

    // To open the dialogue from outside of the script.
    public void Open()
    {
        show = true;
    }

    public void Hide()
    {
        show = false;
    }

    private void Update()
    {
        if(show)
        {
            GameObject.Find("Button_BattleMode").GetComponentInChildren<Button>().interactable = false;
            GameObject.Find("Button_Heroes").GetComponentInChildren<Button>().interactable = false;
            GameObject.Find("Button_Exit").GetComponentInChildren<Button>().interactable = false;
        }
        else
        {
            GameObject.Find("Button_BattleMode").GetComponentInChildren<Button>().interactable = true;
            GameObject.Find("Button_Heroes").GetComponentInChildren<Button>().interactable = true;
            GameObject.Find("Button_Exit").GetComponentInChildren<Button>().interactable = true;
        }
        if(manager != null && manager.FinishTeamUp && GameObject.Find("Button_BattleMode").GetComponentInChildren<Button>().interactable)
        {
            GameObject.Find("Button_BattleMode").GetComponentInChildren<Text>().text = "Searching for New Game...";
            GameObject.Find("Button_BattleMode").GetComponentInChildren<Button>().interactable = false;
        }
        if(manager != null && manager.FinishTeamUp && !manager.IsMatching)
        {
            GameObject.Find("Button_BattleMode").GetComponentInChildren<Text>().text = "Pairup finish... Recruiting opponent... ";
            if (matchmaking == null && ip != null && ip != "" && port != null && port != "")
            {
                matchmaking = StartCoroutine(MatchMaking(true));
            }
        }
        if (manager != null && manager.IsMatching)
        {
            GameObject.Find("Button_BattleMode").GetComponentInChildren<Text>().text = "Opponent found... Preparing for gaming... ";
        }
        if (manager != null && manager.IsMatching)
        {
            Destroy(this);
        }
        if (GameObject.Find("Button_BattleMode").GetComponentInChildren<Text>().text == "Play!")
        {
            Destroy(this);
        }
    }
}