using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BGMContro : MonoBehaviour
{
    // private GameObject Setting;
    // Use this for initialization
    void Awake()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] objs = GameObject.FindGameObjectsWithTag("BGM");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        if (scene.name == "GameScene")
        {
            foreach (GameObject i in objs)
            {
                Destroy(i.gameObject);
            }

        }
        DontDestroyOnLoad(this.gameObject);
        // Setting = GameObject.Find("SettingsController");
    }

    // Update is called once per frame
    void Update()
    {
        Scene scene = SceneManager.GetActiveScene();
        //     GetComponent<AudioSource>().volume = Setting.GetComponent<SettingsController>().volume;
        if (scene.name == "GameScene")
        {
            Destroy(gameObject);
        }
    }
}
