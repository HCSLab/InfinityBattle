using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonController : MonoBehaviour {
    public string username;
    public int port;
    private Process process;

    private void OnEnable()
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = System.Environment.CurrentDirectory + "\\PoPSystem\\Python\\python.exe";
        string directory = System.Environment.CurrentDirectory + "\\PoPSystem\\start.py";
        // UnityEngine.Debug.Log(username);
        start.Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\"", directory, username, port);
        UnityEngine.Debug.Log("Username: " + username);
        UnityEngine.Debug.Log("Port: " + port);
        start.UseShellExecute = false;
        start.CreateNoWindow = true;
        //start.RedirectStandardOutput = true;
        //start.RedirectStandardError = true;
        process = Process.Start(start);
        //StreamReader reader = process.StandardOutput;
        //string stderr = process.StandardError.ReadToEnd();
        //string result = reader.ReadToEnd();
        //UnityEngine.Debug.Log(stderr);
        //UnityEngine.Debug.Log(result);
    }

    private void OnDisable()
    {
        process.Kill();
        //process.CloseMainWindow();
    }

    // Use this for initialization
    void Start () {
        DontDestroyOnLoad(this);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
