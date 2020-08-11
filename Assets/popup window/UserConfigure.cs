using UnityEngine;
using System.Collections;

public class UserConfiguration : MonoBehaviour
{
    // 200x300 px window will apear in the center of the screen.
    private Rect windowRect = new Rect((Screen.width - 400) / 2, (Screen.height - 300) / 2, 400, 300);
    // Only show it if needed.
    private bool show = false;

    private string title, text, ok;

    void OnGUI()
    {
        if (show)
            windowRect = GUI.Window(20, windowRect, DialogWindow, "Player Configuration");
    }

    string ip = null, port = null;
    // This is the actual window.
    void DialogWindow(int windowID)
    {
        GUI.Label(new Rect(15, 25, windowRect.width, 20), "Nick Name: ");
        ip = GUI.TextField(new Rect(150, 25, 150, 20), ip);
        GUI.Label(new Rect(15, 55, windowRect.width, 20), "External Game Port: ");
        port = GUI.TextField(new Rect(150, 55, 150, 20), port);
        GUI.Label(new Rect(15, 85, windowRect.width, 20), "Internal Game Port: ");
        port = GUI.TextField(new Rect(150, 85, 150, 20), port);
        GUI.Label(new Rect(15, 115, windowRect.width, 20), "Blockchain Port: ");
        port = GUI.TextField(new Rect(150, 115, 150, 20), port);
        GUI.Label(new Rect(15, 145, windowRect.width, 20), "API Port: ");
        port = GUI.TextField(new Rect(150, 145, 150, 20), port);
        if (GUI.Button(new Rect(200, 180, 80, 20), "Submit"))
        {
            //Application.Quit();
            if (ip != null && port != null)
            {
                GameObject.Find("NetworkManager").GetComponent<P2PNetworkManager>().SendBattleRequest(ip, int.Parse(port));
            }
            show = false;
        }
        if (GUI.Button(new Rect(300, 180, 80, 20), "Cancel"))
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