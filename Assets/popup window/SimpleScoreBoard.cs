using UnityEngine;
using System.Collections;

public class ScoreBoard : MonoBehaviour
{
    // 200x300 px window will apear in the center of the screen.
    private Rect windowRect = new Rect((Screen.width - 400) / 2, (Screen.height - 400) / 2, 400, 400);
    // Only show it if needed.
    private bool show = false;

    private string text;

    void OnGUI()
    {
        if (show)
            windowRect = GUI.Window(20, windowRect, DialogWindow, "Score Board");
    }

    // This is the actual window.
    void DialogWindow(int windowID)
    {
        // TODO: SHOW Victory / Defeat
        GUI.Label(new Rect(15, 25, windowRect.width, 320), text);

        if (GUI.Button(new Rect(300, 360, 80, 20), "Get It"))
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

    public void UpdateText(string text)
    {
        this.text = text;
    }
}