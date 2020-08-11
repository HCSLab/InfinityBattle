using UnityEngine;
using System.Collections;

public class AlertBox : MonoBehaviour
{
    // 200x300 px window will apear in the center of the screen.
    private Rect windowRect = new Rect((Screen.width - 400) / 2, (Screen.height - 100) / 2, 400, 100);
    // Only show it if needed.
    private bool show = false;

    private string title, text, ok;

    void OnGUI()
    {
        if (show)
            windowRect = GUI.Window(20, windowRect, DialogWindow, title);
    }

    // This is the actual window.
    void DialogWindow(int windowID)
    {
        GUI.Label(new Rect(15, 25, windowRect.width, 20), text);

        if (GUI.Button(new Rect(300, 65, 80, 20), ok))
        {
            //Application.Quit();
            show = false;
        }
    }

    // To open the dialogue from outside of the script.
    public void Open(string title, string text, string ok)
    {
        this.title = title;
        this.text = text;
        this.ok = ok;
        show = true;
        
    }
}