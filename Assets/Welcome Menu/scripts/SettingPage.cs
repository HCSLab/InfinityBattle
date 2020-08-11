using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingPage : MonoBehaviour {

	//private Canvas cvs;
	private Canvas cvs_elements;
	private UnityEngine.UI.Image img_bg;

	public GameObject Settings_Controller;
	public float animate_duration = 1f;
	public bool debug = false;

	private bool in_animate_flag = false;
	private bool out_animate_flag = false;
	private float startTime = 0.0f;
	private int bottom_min = -2000, bottom_max = 0; // elements moving from bottom_min to bottom_max

	void Awake() {
		Debug.Log ("Settings awake.");
		cvs_elements = this.GetComponentsInChildren <Canvas> ()[1]; // [0] means the Canvas_Settings itself, [1] is the elements canvas
		img_bg = this.GetComponentInChildren <UnityEngine.UI.Image> ();
		cvs_elements.GetComponent<RectTransform> ().offsetMax = new Vector2 (cvs_elements.GetComponent<RectTransform> ().offsetMax.x, bottom_min);
		if (!debug) {
			this.enabled = false;
		}
	}

	// Use this for initialization
	void Start () {
		Debug.Log ("Settings started.");
		//cvs_elements.transform.localPosition = new Vector3 ();
	}

	void OnEnable(){
		Debug.Log ("Settings enabled.");
		in_animate_flag = true;
		img_bg.raycastTarget = true;
		//startTime = Time.time;
        startTime = Time.realtimeSinceStartup;
        if (Settings_Controller!=null) {
            Settings_Controller.GetComponent<SettingsController>().RefreshBars();
            Settings_Controller.GetComponent<SettingsController>().ApplySettings();
        }
	}

	void OnDisable(){
		cvs_elements.GetComponent<RectTransform> ().offsetMax = new Vector2 (cvs_elements.GetComponent<RectTransform> ().offsetMax.x, bottom_min);
		img_bg.color = new Color (1f, 1f, 1f, 0f);
		Debug.Log ("Settings disabled.");
	}
	
	// Update is called once per frame
	void LateUpdate () {
        if (in_animate_flag)
        {
            //float t = (Time.time - startTime) / animate_duration;
            float t = (Time.realtimeSinceStartup - startTime) / animate_duration;
            //Debug.Log ("Setting Page");
            //cvs_elements.GetComponent<RectTransform> ().offsetMax = new Vector2 (cvs_elements.GetComponent<RectTransform> ().offsetMax.x, Mathf.SmoothStep (-900, 0, Time.time));
            cvs_elements.GetComponent<RectTransform>().offsetMax = new Vector2(cvs_elements.GetComponent<RectTransform>().offsetMax.x, Mathf.SmoothStep(bottom_min, bottom_max, t));
            img_bg.color = new Color(1f, 1f, 1f, Mathf.SmoothStep(0.0f, 1.0f, t));
            if (t > 1.0f)
            {
                in_animate_flag = false;
            }
        }
        else if (out_animate_flag)
        {
            //float t = (Time.time - startTime) / animate_duration;
            float t = (Time.realtimeSinceStartup - startTime) / animate_duration;
            cvs_elements.GetComponent<RectTransform>().offsetMax = new Vector2(cvs_elements.GetComponent<RectTransform>().offsetMax.x, Mathf.SmoothStep(bottom_max, bottom_min, t));
            img_bg.color = new Color(1f, 1f, 1f, Mathf.SmoothStep(1.0f, 0.0f, t));
            if (t > 1.0f)
            {
                out_animate_flag = false;
                img_bg.raycastTarget = false;
                this.enabled = false;
            }
        }
		
	}

	public void Back(){
		out_animate_flag = true;
        //		startTime = Time.time;
        startTime = Time.realtimeSinceStartup;

    }
}
