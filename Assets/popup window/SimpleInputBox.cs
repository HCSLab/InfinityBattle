using UnityEngine;
using System.Collections;

public class InputBox : MonoBehaviour
{
    // 200x300 px window will apear in the center of the screen.
    private Rect windowRect = new Rect((Screen.width - 400) / 2, (Screen.height - 120) / 2, 400, 120);
    // Only show it if needed.
    private bool show = false;

    private string title, text, ok;

    void OnGUI()
    {
        if (show)
            windowRect = GUI.Window(20, windowRect, DialogWindow, "Matching Opponent");
    }

    string ip = null, port = null;
    // This is the actual window.
    void DialogWindow(int windowID)
    {
        GUI.Label(new Rect(15, 25, windowRect.width, 20), "IP Address: ");
        ip = GUI.TextField(new Rect(100, 25, 200, 20), ip);
        GUI.Label(new Rect(15, 55, windowRect.width, 20), "Port: ");
        port = GUI.TextField(new Rect(100, 55, 200, 20), port);
        if (GUI.Button(new Rect(200, 90, 80, 20), "Submit"))
        {
            //Application.Quit();
            if(ip != null && port != null)
            {
                GameObject.Find("NetworkManager").GetComponent<P2PNetworkManager>().SendBattleRequest(ip, int.Parse(port));
            }
            show = false;
        }
        if (GUI.Button(new Rect(300, 90, 80, 20), "Cancel"))
        {
            //Application.Quit();
            show = false;
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
}