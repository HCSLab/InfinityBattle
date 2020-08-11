using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MatchingController : MonoBehaviour {
    int netport;
    P2PNetworkManager manager;
    Coroutine teamforming = null;
    Coroutine matchmaking = null;

    private void StopTeamForming()
    {
        string content = string.Empty;
        string url = @"http://127.0.0.1:" + (netport + 1) + "/endtf";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip;
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            content = reader.ReadToEnd();
        }
    }

    private void StopMatchMaking()
    {
        string content = string.Empty;
        string url = @"http://127.0.0.1:" + (netport + 1) + "/endmm";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip;
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            content = reader.ReadToEnd();
        }
    }

    private IEnumerator FindTeammate()
    {
        string content = string.Empty;
        string url = @"http://127.0.0.1:" + (netport + 1) + "/starttf";
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
            content = string.Empty;
            url = @"http://127.0.0.1:" + (netport + 1) + "/teammate";
            request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }
            Debug.Log(content);
            if (string.Compare(content, "") != 0)
            {
                Debug.Log("A connection find");
                if (manager.FinishTeamUp)
                {
                    break;
                }
                manager.Terminate();
                manager.Run(content.Split(':')[0], int.Parse(content.Split(':')[1]));
            }
            yield return new WaitForSeconds(10);
            if (manager.IsOccupied == false)
            {
                manager.Run();
            }
        }
    }

    private IEnumerator FindOpponent()
    {
        string content = string.Empty;
        string url = @"http://127.0.0.1:" + (netport + 1) + "/startmm";
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
            content = string.Empty;
            url = @"http://127.0.0.1:" + (netport + 1) + "/opponent";
            request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }
            Debug.Log(content);

            if (string.Compare(content, "") != 0)
            {
                Debug.Log("Opponenents find");
                if (manager.IsMatching)
                {
                    break;
                }
                manager.SendBattleRequest(content.Split(':')[0], int.Parse(content.Split(':')[1]));
            }
            yield return new WaitForSeconds(10);
        }
    }

    // Use this for initialization
    void Start () {
        Debug.Log("Now start");
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

        string ipaddr = GameObject.Find("IPAddress").GetComponentInChildren<InputField>().text;
        string port = GameObject.Find("Port").GetComponentInChildren<InputField>().text;
        if (ipaddr == "" || ipaddr == null || port == "" || port == null)
        {
            manager.Run();
            Debug.Log("Hello");
            teamforming = StartCoroutine(FindTeammate());
        }
        else
        {
            Debug.Log("Preconnect need");
            manager.Run(ipaddr, int.Parse(port));
        }
    }

    private void OnDestroy()
    {
        StopTeamForming();
        StopMatchMaking();
    }

    // Update is called once per frame
    void Update()
    {
        if (manager.FinishTeamUp && teamforming != null)
        {
            StopCoroutine(teamforming);
            teamforming = null;
            matchmaking = StartCoroutine(FindOpponent());
        }
        if (manager.IsMatching && matchmaking != null)
        {
            StopCoroutine(matchmaking);
            matchmaking = null;
        }
    }
}
