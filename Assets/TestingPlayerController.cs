using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestingPlayerController : MonoBehaviour {

    public bool performance;
    public GameObject circle;
    
    public GameObject ActionPanel;
    public GameObject Dashboard;

    private string PendingAction = null;
    public void ActionClick(string action)
    {
        ActionPanel.SetActive(false);
        PendingAction = action;
    }

    public GameObject Role { private set; get; }

    // Use this for initialization
    void Start () {
        var actions = ActionPanel.GetComponentsInChildren<Button>();
        foreach (var action in actions)
        {
            action.onClick.AddListener(() => ActionClick(action.name));
        }
        var heroes = gameObject.GetComponentsInChildren<Transform>();
        foreach(var hero in heroes)
        {
            if(hero.tag == "Hero")
            {
                Role = hero.gameObject;
            }
        }
        Debug.Log(Role.name);
    }

    public bool show;

    private void OnMouseOver()
    {
        if (show)
        {
            circle.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        circle.SetActive(false);
    }

    public void SetShowing(bool show)
    {
        this.show = show;
    }

    public bool MyTurn;

    // Update is called once per frame
    void Update () {
        if (performance && PendingAction != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log("Button Click...");
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                Debug.Log("hello");
                if(Physics.Raycast(ray, out hit, 100.0f))
                {
                    Debug.Log(PendingAction + "   " + hit.transform.name);
                    OperationAction(PendingAction, hit.transform.gameObject);
                    PendingAction = null;
                }
            }
        }
        if (MyTurn && !ActionPanel.activeSelf && PendingAction == null)
        {
            ActionPanel.SetActive(true);
        }
	}

    //Coroutine co;
    public void OperationAction(string ActionName, GameObject Target)
    {
        //if (co != null)
        //{
        //    StopCoroutine(co);
        //}
        Debug.Log("Prepare to operate... ");
        // Judge whether this is a far action
        //Role.GetComponent<Animator>().SetBool("Run", true);
        //Vector3 origin = transform.position;
        //RunTo(Target.transform.position);

        //Role.GetComponent<HeroAttributes>().Target = Target.GetComponent<TestingPlayerController>().Role;
        //Role.GetComponent<Animator>().SetBool(ActionName, true);
        //StartCoroutine(delay(ActionName));

        //Debug.Log(origin);
        //RunTo(origin);
        //Role.GetComponent<Animator>().SetBool("Run", false);
        Role.GetComponent<HeroAttributes>().Target = Target.GetComponent<TestingPlayerController>().Role;
        if (!Role.GetComponent<HeroAttributes>().isFar(ActionName))
        {
            Role.GetComponent<Animator>().SetBool("Run", true);
            StartCoroutine(delay(ActionName, Target.transform.position));
        }
        else
        {
            Role.GetComponent<Animator>().SetBool(ActionName, true);
            StartCoroutine(delay(ActionName));
        }
    }

    /// <summary>
    /// For the purpose of long attack
    /// </summary>
    /// <param name="ActionName"></param>
    /// <returns></returns>
    IEnumerator delay(string ActionName)
    {
        int frame = Role.GetComponent<HeroAttributes>().getExpectedFrame(ActionName);
        Debug.Log("waiting frame: " + frame);
        for (int i = 0; i < frame + 20; i++)
        {
            yield return null;
        }
        //Debug.Log("hello");
        yield return new WaitForSeconds(3.0f);
        //Debug.Log("bye!");
        // yield return new WaitForSeconds(1.0f);
        Role.GetComponent<Animator>().SetBool(ActionName, false);
    }

    /// <summary>
    /// For the purpose of short attack
    /// </summary>
    /// <param name="ActionName"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    IEnumerator delay(string ActionName, Vector3 dest)
    {
        Vector3 origin = transform.position;
        yield return delayRun(dest);

        Role.GetComponent<Animator>().SetBool(ActionName, true);
        yield return delay(ActionName);
        //Role.GetComponent<Animator>().SetBool(ActionName, false);

        yield return delayRunBack(origin);
        Role.GetComponent<Animator>().SetBool("Run", false);
    }

    IEnumerator delayRun(Vector3 dest)
    {
        while(Vector3.Distance(transform.position, dest) > 1f)
        {
            //Debug.Log(transform.position + " " + dest);
            yield return null;
            float step = 3 * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, dest, step);
        }
    }

    IEnumerator delayRunBack(Vector3 dest)
    {
        while (Vector3.Distance(transform.position, dest) > 0.0001f)
        {
            //Debug.Log(transform.position + " " + dest);
            yield return null;
            float step = 3 * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, dest, step);
        }
    }
}
