using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginController : MonoBehaviour {
    public GameObject revenue;
    public GameObject Button_Heroes;
    public GameObject Button_BattleMode;
    public GameObject Button_Exit;
    public GameObject SettingsController;

    public GameObject PythonController;
    public GameObject NetworkManager;

    public GameObject PlayerName;
    public GameObject PortNumber;
    public GameObject Button_Login;
    
    public void Login()
    {
        string username = GameObject.Find("UserName").GetComponent<InputField>().text;
        string portnum = GameObject.Find("portnum").GetComponent<InputField>().text;
        //Debug.Log("Username: " + username);
        //Debug.Log("Port: " + portnum);
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.SetString("portnum", portnum);

        PythonController.GetComponent<PythonController>().username = username;
        PythonController.GetComponent<PythonController>().port = int.Parse(portnum);

        revenue.SetActive(true);
        Button_Heroes.SetActive(true);
        Button_BattleMode.SetActive(true);
        Button_Exit.SetActive(true);
        SettingsController.SetActive(true);

        PythonController.SetActive(true);
        NetworkManager.SetActive(true);
        
        PlayerName.SetActive(false);
        PortNumber.SetActive(false);
        Button_Login.SetActive(false);
    }

    public void Logout()
    {
        revenue.SetActive(false);
        Button_Heroes.SetActive(false);
        Button_BattleMode.SetActive(false);
        Button_Exit.SetActive(false);
        SettingsController.SetActive(false);

        PythonController.SetActive(false);
        NetworkManager.SetActive(false);

        PlayerName.SetActive(true);
        PortNumber.SetActive(true);
        Button_Login.SetActive(true);
        GameObject.Find("UserName").GetComponent<InputField>().text = PlayerPrefs.GetString("username");
        GameObject.Find("portnum").GetComponent<InputField>().text = PlayerPrefs.GetString("portnum");
    }

	// Use this for initialization
	void Start () {
        Debug.Log("hello, main start. ");
        if (GameObject.Find("PythonController") != null)
        {
            revenue.SetActive(true);
            Button_Heroes.SetActive(true);
            Button_BattleMode.SetActive(true);
            Button_Exit.SetActive(true);
            SettingsController.SetActive(true);

            PythonController = GameObject.Find("PythonController");
            NetworkManager = GameObject.Find("NetworkManager");

            PlayerName.SetActive(false);
            PortNumber.SetActive(false);
            Button_Login.SetActive(false);
        }
        else
        {
            GameObject.Find("UserName").GetComponent<InputField>().text = PlayerPrefs.GetString("username");
            GameObject.Find("portnum").GetComponent<InputField>().text = PlayerPrefs.GetString("portnum");
        }
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
