using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarControl : MonoBehaviour {

    public Image TeammateFilled;
    public Image OpponentFilled;
    public Image Bar;
    public float FilledBlood;
    public float CurrentBlood;

    public void SetHealthBar(bool isOpponent, float FilledBlood)
    {
        if (isOpponent)
        {
            Bar = OpponentFilled;
        }
        else
        {
            Bar = TeammateFilled;
        }
        Debug.Log(transform.parent.gameObject.name + " " +Bar);
        Bar.gameObject.SetActive(true);
        this.FilledBlood = FilledBlood;
        this.CurrentBlood = FilledBlood;
    }

    // return whether the prefab is death
    public bool Injured(float amount)
    {
        if(amount <= 0)
        {
            return false;
        }
        CurrentBlood -= amount;
        if(CurrentBlood <= 0)
        {
            CurrentBlood = 0;
            return true;
        }
        return false;
    }

    public void Recover(float amount)
    {
        if(amount <= 0)
        {
            return;
        }
        CurrentBlood = Mathf.Min(CurrentBlood + amount, FilledBlood);
    }

	// Use this for initialization
	void Start () {
        // Bar = TeammateFilled;
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(transform.parent.gameObject.name + " " + Bar);
        Bar.fillAmount = CurrentBlood / FilledBlood;
	}
}
