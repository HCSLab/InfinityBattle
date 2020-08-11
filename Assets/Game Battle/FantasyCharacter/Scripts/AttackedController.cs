using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackedController : MonoBehaviour {

    // Use this for initialization
    Animator anim;
    public GameObject canvas;
    public GameObject myself;

    public bool isFar = true;
	void Start () {
        anim = GetComponent<Animator>();
	}

    //Coroutine co;
    public void attacked(GameObject attacker, float amount)
    {
        //if(co != null)
        //{
        //    StopCoroutine(co);
        //}
        amount = Mathf.Min(amount, canvas.GetComponent<HealthBarControl>().CurrentBlood);
        
        if(attacker.tag == "Player")
        {
            //Debug.Log("Attacker: " + attacker);
            attacker.GetComponent<PlayerController>().AddAmountOfHarm(amount);
        }
        else if (attacker.tag == "Hero")
        {
            //Debug.Log("Attacker: " + attacker.transform.parent);
            attacker.transform.parent.GetComponent<PlayerController>().AddAmountOfHarm(amount);
        }
        else
        {
            Debug.Log(attacker.tag);
            Debug.Log(attacker);
            Debug.Log("Invalid Operation");
            return;
        }
        myself.GetComponent<PlayerController>().AddAmountOfBear(amount);
        anim.SetBool("Attacked", true);
        StartCoroutine(delay());
        Debug.Log(gameObject.name + "!!! Attack detect " + amount);
        if (canvas.GetComponent<HealthBarControl>().Injured(amount))
        {
            // this indicates die
            anim.SetBool("Death", true);
            Debug.Log(gameObject.name + "!!! Die");
            myself.GetComponent<PlayerController>().SetDeath();
            if (attacker.tag == "Player")
            {
                Debug.Log("Attacker: " + attacker);
                attacker.GetComponent<PlayerController>().AddKill();
            }
            else if (attacker.tag == "Hero")
            {
                Debug.Log("Attacker: " + attacker.transform.parent);
                attacker.transform.parent.GetComponent<PlayerController>().AddKill();
            }
        }
        
    }

    IEnumerator delay()
    {
        yield return new WaitForSeconds(3.0f);
        //co = null;
        anim.SetBool("Attacked", false);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
