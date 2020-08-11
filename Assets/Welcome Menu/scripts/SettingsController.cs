using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Text;
using SFB;
using Newtonsoft.Json;
using System.Net;

public class SettingsController : MonoBehaviour
{

    /* ==== Settings ==== */
    private float min_b = 0.5f, max_b = 1.5f;
    [Range(0.5f, 1.5f)]
    public float brightness = 1.0f;

    private float min_s = 0f, max_s = 2f;
    [Range(0f, 2f)]
    public float saturation = 1.0f;

    private float min_c = 0.5f, max_c = 1.5f;
    [Range(0.5f, 1.5f)]
    public float contrast = 1.0f;

    private float min_vol = 0f, max_vol = 1f;
    [Range(0f, 1f)]
    public float volume = 1f;
    private new AudioSource audio;
    /* ==== Saving Game ==== */
    private string username;
    private string pubKey;
    private string port;

    public Scrollbar scrollbar_b, scrollbar_s, scrollbar_c, scrollbar_vol;
    public string menu_scene = "MenuScene",
                  battle_scene = "MultiMenu";

    private Camera main_camera;
    public static bool exist_flag = false;

    void Awake()
    {
        /*if (exist_flag) {
			print ("Settings controller has been existed. Destroy the new one.");
			GameObject.Destroy (this.gameObject);
			return;
		}*/
        print("New settings controller.");
        exist_flag = true;
        //GameObject.DontDestroyOnLoad (this);
        LoadSettings();
        ApplySettings();
        RefreshBars();
        audio = GetComponent<AudioSource>();
        
    }

    public void RefreshBars()
    {
        Debug.Log("REFRESH BARS.");
        scrollbar_b = GameObject.Find("Scrollbar_Brightness").GetComponent<UnityEngine.UI.Scrollbar>();
        scrollbar_s = GameObject.Find("Scrollbar_Saturation").GetComponent<UnityEngine.UI.Scrollbar>();
        scrollbar_c = GameObject.Find("Scrollbar_Contrast").GetComponent<UnityEngine.UI.Scrollbar>();
        scrollbar_vol = GameObject.Find("Scrollbar_Volume").GetComponent<UnityEngine.UI.Scrollbar>();
        scrollbar_b.value = (brightness - min_b) / (max_b - min_b);
        scrollbar_s.value = (saturation - min_s) / (max_s - min_s);
        scrollbar_c.value = (contrast - min_c) / (max_c - min_c);
        scrollbar_vol.value = (volume - min_vol) / (max_vol - min_vol);
        SetVolume(volume);
    }

    public void SetBrightness(float t)
    {
        brightness = min_b + t * (max_b - min_b);
        main_camera.GetComponent<BasicPostProcessing>().brightness = brightness;
    }

    public void SetSaturation(float t)
    {
        saturation = min_s + t * (max_s - min_s);
        main_camera.GetComponent<BasicPostProcessing>().saturation = saturation;
    }

    public void SetContrast(float t)
    {
        contrast = min_c + t * (max_c - min_c);
        main_camera.GetComponent<BasicPostProcessing>().contrast = contrast;
    }

    public void SetVolume(float t)
    {
        volume = min_vol + t * (max_vol - min_vol);
        FindAllSounds();
    }

    public void ResetSettings()
    {
        audio.Play();
        brightness = 1f;
        saturation = 1f;
        contrast = 1f;
        volume = 1f;
        RefreshBars();
    }

    private void FindCameras()
    {
        //GameObject t;
        main_camera = Camera.main;
        /*t = GameObject.Find("CameraBegin");
        if (t != null)
        {
            camera_beg = t.GetComponent<Camera>();
            Debug.Log("!!! FIND BEG CAM");
        }
        else camera_beg = null;
        t = GameObject.Find("CameraBoss");
        if (t != null)
        {
            camera_boss = t.GetComponent<Camera>();
            Debug.Log("!!! FIND Boss CAM");
        }
        else camera_boss = null;*/
    }

    private void ApplyToCamera(Camera c)
    {
        if (c == null) return;
        c.GetComponent<BasicPostProcessing>().brightness = brightness;
        c.GetComponent<BasicPostProcessing>().saturation = saturation;
        c.GetComponent<BasicPostProcessing>().contrast = contrast;
    }

    public void ApplySettings()
    {
        FindCameras(); //Please make sure that the main camera is tagged as MainCamera
        Debug.Log("Settings applied.");

        ApplyToCamera(main_camera);
        SaveSettings();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("brightness", brightness);
        PlayerPrefs.SetFloat("saturation", saturation);
        PlayerPrefs.SetFloat("contrast", contrast);
        PlayerPrefs.SetFloat("volume", volume);
    }

    private void LoadSettings()
    {
        brightness = PlayerPrefs.GetFloat("brightness", brightness);
        saturation = PlayerPrefs.GetFloat("saturation", saturation);
        contrast = PlayerPrefs.GetFloat("contrast", contrast);
        volume = PlayerPrefs.GetFloat("volume", volume);
        // configurationFile = PlayerPrefs.GetString("configurationFile", configurationFile);
        // GameObject.Find("FileSelection").GetComponentInChildren<Text>().text = configurationFile;
        ApplySettings();
    }

    public void GotoMenu()
    {
        audio.Play();
        SceneManager.LoadScene(menu_scene);
    }

    public void GotoBattle()
    {
        audio.Play();
        port = GameObject.Find("NameInput").GetComponentInChildren<InputField>().text;
        SceneManager.LoadScene(battle_scene);
    }

    public void QuitGame()
    {
        audio.Play();
        SaveSettings();
        if (GameObject.Find("Button_BattleMode").GetComponentInChildren<Text>().text != "Play!")
        {
            GameObject.Find("NetworkManager").GetComponent<P2PNetworkManager>().Terminate();
        }
        Application.Quit();
    }

    public void FindAllSounds()
    {

        //1.find the hero's animation component
        object[] sounds = GameObject.FindObjectsOfType<AudioSource>();
        //assigned the array of hero's model 
        GameObject[] SoundsModels = new GameObject[sounds.Length];
        //test
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i] is AudioSource)//check the type
            {

                SoundsModels[i] = (sounds[i] as AudioSource).gameObject;
                SoundsModels[i].GetComponent<AudioSource>().volume = volume;
            }
        }
    }

    public void ShowHeroes()
    {
        AlertBox gm = gameObject.AddComponent<AlertBox>();
        gm.Open("IP Address", "IP Address: " + 
            GameObject.Find("NetworkManager").GetComponent<P2PNetworkManager>().GetIpAddress() + "\tPort: " + 
            GameObject.Find("PythonController").GetComponent<PythonController>().port, "Get it");
        //GameMenu.Open();
    }


    private void Update()
    {
        //if (GameObject.Find("NetworkManager").GetComponent<P2PNetworkManager>().FinishTeamUp)
        //{
        //    //GameObject.Find("Button_BattleMode").GetComponentInChildren<Text>().text = "Play!";
        //    GameObject.Find("Button_BattleMode").GetComponentInChildren<Button>().interactable = false;
        //    GameObject.Find("Button_Exit").GetComponentInChildren<Button>().interactable = false;
        //    if (!GameObject.Find("NetworkManager").GetComponent<P2PNetworkManager>().IsMatching)
        //    {
        //        if (gameObject.GetComponent<InputBox>() == null)
        //        {
        //            Debug.Log("Find Opponents now... ");
        //            InputBox input = gameObject.AddComponent<InputBox>();
        //            input.Open();
        //        }
        //    }
        //    else
        //    {
        //        if (gameObject.GetComponent<InputBox>() != null)
        //        {
        //            gameObject.GetComponent<InputBox>().Hide();
        //            Destroy(gameObject.GetComponent<InputBox>());

        //        }
        //    }
        //}
    }

    private GameObject ButtonSetting;

    public void PlayControl()
    {
        if (GameObject.Find("Button_BattleMode").GetComponentInChildren<Text>().text == "Play!")
        {
            GameObject.Find("Button_BattleMode").GetComponentInChildren<Text>().text = "Exit grouping...";
           gameObject.AddComponent<GroupingInput>().Open();
        }
        else
        {
            GameObject.Find("Button_BattleMode").GetComponentInChildren<Text>().text = "Play!";
            //if (gameObject.GetComponent<MatchingController>() != null)
            //{
            //    Destroy(gameObject.GetComponent<MatchingController>());
            //}
            GameObject.Find("NetworkManager").GetComponent<P2PNetworkManager>().Terminate();
        }
    }
}
