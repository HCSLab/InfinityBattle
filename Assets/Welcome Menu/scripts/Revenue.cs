using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System;

public class Revenue : MonoBehaviour {
    public Text text;
    private bool updated;

    private IEnumerator coroutine;

    // Use this for initialization
    void Start () {
        coroutine = WaitFive();
        StartCoroutine(coroutine);
	}

    // Update is called once per frame
    void Update () {
        
	}

    private IEnumerator WaitFive()
    {
        while (true)
        {
            Debug.Log("Updating Text...");
            UpdateText();
            yield return new WaitForSeconds(10);
        }
    }

    private void UpdateText()
    {
        string content = string.Empty;
        int port = GameObject.Find("PythonController").GetComponent<PythonController>().port;
        string uname = GameObject.Find("PythonController").GetComponent<PythonController>().username;
        Dictionary<string, int> revenue = new Dictionary<string, int>();
        try
        {
            string url = @"http://127.0.0.1:" + (port + 1) + "/revenue";
            //Debug.Log(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }
            revenue = JsonConvert.DeserializeObject<Dictionary<string, int>>(content);
        }
        catch
        {
            revenue.Add("Confirmed", 0);
            revenue.Add("Unconfirmed", 0);
        }
        text.text = "Hi, " + uname + "!        Confirmed Deposit: " + revenue["Confirmed"] + "        Unconfirmed Deposit: " + revenue["Unconfirmed"];
    }
}
