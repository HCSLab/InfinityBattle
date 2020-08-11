using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {

	public UnityEngine.UI.Image img_cover;
	private bool in_animate_flag = false;
	private float startTime = 0f;
	private float animate_duration = 2.5f;
	private float cover_r = 0.6f, cover_g = 0.5f, cover_b = 0.4f;

	// Use this for initialization
	void Start () {
		print ("Main menu started.");
		in_animate_flag = true;
		startTime = Time.time;
        Time.timeScale = 1;
	}
	
	// Update is called once per frame
	void Update () {
		float t;
		if (in_animate_flag) {
			t = (Time.time - startTime) / animate_duration;
			img_cover.color = new Color (Mathf.SmoothStep (img_cover.color.r, cover_r, t), Mathf.SmoothStep (img_cover.color.g, cover_g, t), Mathf.SmoothStep (img_cover.color.b, cover_b, t), Mathf.SmoothStep (1.0f, 0.0f, t));
			if (t > 1.0f) {
				in_animate_flag = false;
			}
		}
	}
}
