using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using System.IO;

public class ResultBoardConfigure : MonoBehaviour {

    public GameObject victory;
    public GameObject defeat;
    public Button GetIt;
    public Text results;
    private Dictionary<string, object> full_result;
    private int MatchID;

    // Use this for initialization
    void Start () {
        int width = Screen.width;
        int height = Screen.height;
        Debug.Log(width);
        Debug.Log(height);
        //cup.GetComponent<RectTransform>().localPosition = new Vector3(-width / 2 + 180, 0);
        
        //results.GetComponent<RectTransform>().localPosition = new Vector3(width / 2 - 180, height / 2 - 180);
        //GetIt.GetComponent<RectTransform>().localPosition = new Vector3(width / 2 - 150, -height / 2 + 50);
        GetIt.onClick.AddListener(() =>
        {
            GameObject.Find("NetworkManager").GetComponent<P2PNetworkManager>().Terminate();
            SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
        });
        //GameObject.Find("Cup").transform.position = gameObject.GetComponent<RectTransform>().
        //GetComponent<Button>().onClick.AddListener(() =>
        //{
        //    SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
        //    GameObject.Find("NetworkManager").GetComponent<P2PNetworkManager>().Terminate();
        //});
    }
	
    public void SetResult(bool isVictory, string final_result)
    {
        if (isVictory)
        {
            victory.SetActive(true);
            defeat.SetActive(false);
        }
        else
        {
            defeat.SetActive(true);
            victory.SetActive(false);
        }
        results.text = final_result;
        //this.full_result = full_result;
        //this.MatchID = MatchID;
        // write the result to a file

    }

    public void SubmitResult(int port, int ID, string[] procedure, Dictionary<string, object> result, string[] ips)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["ID"] = ID;
        dict["Procedure"] = procedure;
        dict["Result"] = result;
        dict["IP"] = ips;
        
        // port+2
        string url = "http://127.0.0.1:" + (port + 2) + "/NewMatch";
        Debug.Log(url);
        var request = (HttpWebRequest)WebRequest.Create(url);

        var postData = JsonConvert.SerializeObject(dict);
        Debug.Log(postData);

        var data = Encoding.ASCII.GetBytes(postData);

        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = data.Length;

        using (var stream = request.GetRequestStream())
        {
            stream.Write(data, 0, data.Length);
        }

        var response = (HttpWebResponse)request.GetResponse();
        if (response.StatusCode == HttpStatusCode.OK)
        {
            Debug.Log("Get it");
        }
        Debug.Log("Error");
        //var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
    }

	// Update is called once per frame
	void Update () {
		
	}
}
