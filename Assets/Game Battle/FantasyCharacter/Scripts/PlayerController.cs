using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : P2PNetworkBehavior {
    // Basic UI: configure users' setting
    private GameObject SelectPanel;
    private GameObject ActionPanel;
    private GameObject FinalResultPanel;
    private GameObject DashBoard;
    public GameObject circle;
    private Text PersonalRecords;
    private Text Teamfire;

    private bool select;
    private bool CameraSet;
    // private bool PositionSet;
    private GameObject Role;
    private HealthBarControl hbc;
    private Transform canvas;

    // some necessary parameters
    public bool MyTurn;
    public int killed = 0;
    public int death = 0;
    public float AmountOfHarm;
    public float AmountOfBear;
    public float AmountOfRescue;

    public int DeathLock = 0;
    private ScoreBoard scoreBoard;

    public void SetToMyTurn()
    {
        MyTurn = true;
    }

    public bool Operatable()
    {
        if(DeathLock == 0)
        {
            return true;
        }
        else
        {
            DeathLock--;
            if(DeathLock == 0)
            {
                reborn();
            }
            return false;
        }
    }

    private bool show;

    private void OnMouseOver()
    {
        if (show && !isDeath())
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
        if (isDeath())
        {
            show = false;
        }
        this.show = show;
    }

    // Update is called once per frame
    void Update () {
        //Debug.Log(Role + " " + canvas + " " + SelectPanel + " " + ActionPanel + " " + DashBoard);
        if (!IsLocalPlayer)
        {
            return;
        }
		if(selectable != null && !select)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.mouseScrollDelta.y > 0)
            {
                Role.SetActive(false);
                Role = selectable[(selectable.IndexOf(Role) - 1 + selectable.Count) % selectable.Count];
                Role.SetActive(true);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.mouseScrollDelta.y < 0)
            {
                Role.SetActive(false);
                Role = selectable[(selectable.IndexOf(Role) + 1) % selectable.Count];
                Role.SetActive(true);
            }
        }
        if(PersonalRecords != null && Teamfire != null)
        {
            PersonalRecords.text = "Killed: " + killed.ToString() + "\t" + "Death: " + death.ToString();
            Teamfire.text = "Team Fires: " + GameObject.Find("NetworkManager").GetComponent<TurnedBasedNetworkController>().teamfires;
        }
        if(scoreBoard != null)
        {
            scoreBoard.UpdateText(GameObject.Find("NetworkManager").GetComponent<TurnedBasedNetworkController>().getRecords());
        }
        if(PendingAction != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log("Button Click...");
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                Debug.Log("hello");
                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    Debug.Log(PendingAction + "   " + hit.transform.name);
                    if (hit.transform.gameObject.GetComponent<PlayerController>().DeathLock == 0)
                    {
                        GameObject.Find("NetworkManager").GetComponent<TurnedBasedNetworkController>().SendBehavior(PendingAction + " "
                        + hit.transform.gameObject.GetComponent<P2PNetworkIdentifier>().Channel);
                        PendingAction = null;
                    }
                }
            }
        }
	}

    private bool isWinner;

    public void SetDeath()
    {
        death++;
        DeathLock = death * 2 + 3;
        if (IsLocalPlayer)
        {
            ActionPanel.SetActive(false);
        }
        if (MyTurn)
        {
            MyTurn = false;
            GameObject.Find("NetworkManager").GetComponent<TurnedBasedNetworkController>().NextOne();
        }
        Debug.Log(Role.name + " Death detect!!! stop for " + DeathLock + " rounds. ");
    }

    public void loss()
    {
        Debug.Log("Loss");
        isWinner = false;
        if (IsLocalPlayer)
        {
            SelectPanel.SetActive(false);
            ActionPanel.SetActive(false);
            DashBoard.SetActive(false);
            FinalResultPanel.SetActive(true);
            //GameObject.Find("Result").GetComponent<ResultBoardConfigure>().SetResult(false,
            //    GameObject.Find("NetworkManager").GetComponent<TurnedBasedNetworkController>().getRecords(),
            //    GameObject.Find("NetworkManager").GetComponent<TurnedBasedNetworkController>().getMatchID(), null);
            ////GameObject.Find("NetworkManager").GetComponent<TurnedBasedNetworkController>().getRecords(true));
            //GameObject.Find("NetworkManager").GetComponent<TurnedBasedNetworkController>().getRecords(true);
        }
    }

    public void victory()
    {
        Debug.Log("Victory");
        isWinner = true;
        Role.GetComponent<Animator>().SetBool("Victory", true);
        if (IsLocalPlayer)
        {
            SelectPanel.SetActive(false);
            ActionPanel.SetActive(false);
            DashBoard.SetActive(false);
            FinalResultPanel.SetActive(true);
            //GameObject.Find("Result").GetComponent<ResultBoardConfigure>().SetResult(true,
            //    GameObject.Find("NetworkManager").GetComponent<TurnedBasedNetworkController>().getRecords(),
            //    GameObject.Find("NetworkManager").GetComponent<TurnedBasedNetworkController>().getMatchID(), null);
            ////GameObject.Find("NetworkManager").GetComponent<TurnedBasedNetworkController>().getRecords(true));
            //GameObject.Find("NetworkManager").GetComponent<TurnedBasedNetworkController>().getRecords(true);
        }
    }

    public void AddKill()
    {
        killed++;
    }

    public void AddAmountOfHarm(float amount)
    {
        AmountOfHarm = AmountOfHarm + amount;
    }

    public void AddAmountOfBear(float amount)
    {
        AmountOfBear = AmountOfBear + amount;
    }

    public void AddAmountOfRescue(float amount)
    {
        AmountOfRescue = AmountOfRescue + amount;
    }

    public string getRecord()
    {
        return "Killed: " + killed.ToString() + "\t" + "Death: " + death.ToString();
    }

    public Dictionary<string, object> getRecord(bool grade)
    {
        //float rating = 0;
        //if (isWinner)
        //{
        //    rating = (float)killed / (death + 1) * 1.6f;
        //}
        //else
        //{
        //    rating = (float)killed / (death + 1);
        //}
        //return "Killed:" + killed.ToString() + " " + "Death:" + death.ToString() + " " + "Grade:" + mark.ToString() 
        //    + " " + "IsWinner:" + isWinner;
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("Role", Role.name);
        dict.Add("Killed", killed);
        dict.Add("Death", death);
        dict.Add("IsWinner", isWinner);
        //dict.Add("Rating", rating);
        dict.Add("AmountOfHarm", AmountOfHarm);
        dict.Add("AmountOfBear", AmountOfBear);
        dict.Add("AmountOfRescue", AmountOfRescue);
        return dict;
    }

    public GameObject getRole()
    {
        return Role;
    }

    public void SelectHero(string name)
    {
        Debug.Log(name);
        if (!select)
        {
            select = true;
            MyTurn = false;
            var heroes = gameObject.GetComponentsInChildren<Transform>(true);
            foreach (var hero in heroes)
            {
                if ((hero.tag.CompareTo("Hero") == 0) && (hero.name.CompareTo(name) == 0))
                {
                    Role = hero.gameObject;
                    canvas.GetComponent<HealthBarControl>().CurrentBlood = Role.GetComponent<HeroAttributes>().FilledBlood;
                    canvas.GetComponent<HealthBarControl>().FilledBlood = Role.GetComponent<HeroAttributes>().FilledBlood;
                    Debug.Log("I find the hero...");
                    Role.SetActive(true);
                    break;
                }
            }
        }
    }

    List<GameObject> selectable;
    public void SelectHero(List<string> BanList)
    {
        Debug.Log("Please select hero");
        selectable = new List<GameObject>();
        var heroes = gameObject.GetComponentsInChildren<Transform>(true);
        foreach(var hero in heroes)
        {
            if((hero.tag == "Hero") && (!BanList.Contains(hero.name)))
            {
                selectable.Add(hero.gameObject);
                //Debug.Log(hero.name + " " + hero.gameObject.name);
            }
        }
        Role = selectable[0];
        //Debug.Log(Role.name);
        Role.SetActive(true);
        //GameObject selectPanel = GameObject.Find("SelectHero");
        SelectPanel.SetActive(true);
    }

    public void SelectClick()
    {
        select = true;
        MyTurn = false;

        var children = gameObject.GetComponentsInChildren<Transform>();
        HealthBarControl hbc = canvas.GetComponent<HealthBarControl>();
        HeroAttributes ha = Role.GetComponent<HeroAttributes>();
        hbc.CurrentBlood = ha.FilledBlood;
        hbc.FilledBlood = ha.FilledBlood;
        //GameObject selectPanel = GameObject.Find("SelectHero");
        SelectPanel.SetActive(false);
        selectable = null;
        GameObject.Find("NetworkManager").GetComponent<TurnedBasedNetworkController>().SendBehavior(Role.name);
    }

    //Coroutine co;
    public void OperateAction(string ActionName, GameObject Target, GameObject[] Opponents, GameObject[] Teammates)
    {
        //if(co != null)
        //{
        //    StopCoroutine(co);
        //}
        MyTurn = false;
        Role.GetComponent<HeroAttributes>().Target = Target.GetComponent<PlayerController>().getRole();
        if (!Role.GetComponent<HeroAttributes>().isFar(ActionName))
        {
            Role.GetComponent<Animator>().SetBool("Run", true);
            StartCoroutine(delay(ActionName, Target.transform.position, Opponents, Teammates));
        }
        else
        {
            Role.GetComponent<Animator>().SetBool(ActionName, true);
            StartCoroutine(delay(ActionName, Opponents, Teammates));
        }
    }

    IEnumerator delay(string ActionName, GameObject[] Opponents, GameObject[] Teammates)
    {
        int frame = Role.GetComponent<HeroAttributes>().getExpectedFrame(ActionName);
        for (int i = 0; i < frame; i++)
        {
            yield return null;
        }
        //yield return new WaitForSeconds(0.25f);
        yield return new WaitForSeconds(3.0f);
        // co = null;
        Role.GetComponent<Animator>().SetBool(ActionName, false);
        if (Role.GetComponent<HeroAttributes>().getAttackHarmToAll(ActionName))
        {
            foreach(var opponent in Opponents)
            {
                opponent.GetComponent<PlayerController>().attacked(Role, Role.GetComponent<HeroAttributes>().getAttackAmount(ActionName));
            }
        }
        recovered(gameObject, Role.GetComponent<HeroAttributes>().getRecoverToSelf(ActionName));
        foreach(var teammate in Teammates)
        {
            teammate.GetComponent<PlayerController>().recovered(gameObject, Role.GetComponent<HeroAttributes>().getRecoverToAll(ActionName));
        }
    }

    IEnumerator delay(string ActionName, Vector3 dest, GameObject[] Opponents, GameObject[] Teammates)
    {
        Vector3 origin = transform.position;
        yield return delayRun(dest);

        Role.GetComponent<Animator>().SetBool(ActionName, true);
        yield return delay(ActionName, Opponents, Teammates);

        yield return delayRunBack(origin);
        Role.GetComponent<Animator>().SetBool("Run", false);
    }

    IEnumerator delayRun(Vector3 dest)
    {
        while (Vector3.Distance(transform.position, dest) > 1f)
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

    public void OperateAction(int teamfire)
    {
        Debug.Log("Please select an action");
        var actions = ActionPanel.GetComponentsInChildren<Button>();
        foreach(var action in actions)
        {
            Debug.Log(action.name + " " + teamfire + " " + Role.GetComponent<HeroAttributes>().getConsumed(action.name) +
                " " + Role.GetComponent<HeroAttributes>().getAttackAmount(action.name) + " " + 
                Role.GetComponent<HeroAttributes>().getRecoverToSelf(action.name) + 
                " " + Role.GetComponent<HeroAttributes>().getRecoverToAll(action.name));
            if(teamfire < Role.GetComponent<HeroAttributes>().getConsumed(action.name))
            {
                Debug.Log("Inactive for reason 1");
                action.interactable = false;
            }
            else if(Role.GetComponent<HeroAttributes>().getAttackAmount(action.name) == 0 && 
                Role.GetComponent<HeroAttributes>().getRecoverToSelf(action.name) == 0 &&
                Role.GetComponent<HeroAttributes>().getRecoverToAll(action.name) == 0)
            {
                Debug.Log("Inactive for reason 2");
                action.interactable = false;
            }
            else
            {
                action.interactable = true;
            }
        }
        ActionPanel.SetActive(true);
    }

    private string PendingAction = null;
    public void ActionClick(string action)
    {
        ActionPanel.SetActive(false);
        PendingAction = action;
    }

    public void SetPendingAction(string action)
    {
        PendingAction = action;
    }

    public void ScoreBoardClick()
    {
        scoreBoard.Open();
    }

    public bool isDeath()
    {
        if(Role != null)
        {
            Debug.Log(Role.name + " Death Lock: " + DeathLock);
        }
        
        return DeathLock > 0;
    }

    public void attacked(GameObject attacker, float amount)
    {
        if (isDeath() || amount == 0)
        {
            return;
        }
        Role.GetComponent<AttackedController>().attacked(attacker, amount);
    }

    public void reborn()
    {
        var children = gameObject.GetComponentsInChildren<Transform>();
        foreach (var child in children)
        {
            if (child.name.CompareTo("Canvas") == 0)
            {
                canvas = child;
            }
        }
        Debug.Log("canvas: " + canvas);

        canvas.GetComponent<HealthBarControl>().CurrentBlood = canvas.GetComponent<HealthBarControl>().FilledBlood;
        Role.GetComponent<Animator>().SetBool("Death", false);
    }

    public void recovered(GameObject player, float amount)
    {
        if (isDeath() || amount == 0)
        {
            Debug.Log(Role.name + " is dead. Recover is not valid. ");
            return;
        }
        amount = Mathf.Min(amount, canvas.GetComponent<HealthBarControl>().FilledBlood - canvas.GetComponent<HealthBarControl>().CurrentBlood);
        player.GetComponent<PlayerController>().AddAmountOfRescue(amount);
        canvas.GetComponent<HealthBarControl>().Recover(amount);
    }

    private bool configured = false;
    private void OnEnable()
    {
        if (configured)
        {
            return;
        }

        //while (!SceneManager.GetSceneByName("BattleField").isLoaded);
        //Debug.Log("Load scene finish.. ");
        transform.position = GameObject.Find("Team" + TeamNumber + "_Player" + Channel + "_Position").transform.position;

        // controlling the name and the blood bar
        var children = gameObject.GetComponentsInChildren<Transform>(true);
        selectable = null;
        foreach (var child in children)
        {
            if (child.name.CompareTo("Canvas") == 0)
            {
                canvas = child;
            }
        }

        Debug.Log("canvas: " + canvas);
        canvas.GetComponentInChildren<Text>().text = NickName;
        hbc = canvas.GetComponent<HealthBarControl>();
        scoreBoard = gameObject.AddComponent<ScoreBoard>();

        // special setting for opponent
        // position & healthbar
        if (IsOpponent)
        {
            if(TeamNumber == 0)
            {
                transform.Rotate(0, 0, 0);
                canvas.Rotate(0, 180, 0);
            }
            else
            {
                transform.Rotate(0, 180, 0);
                canvas.Rotate(0, 180, 0);
            }
            canvas.GetComponent<HealthBarControl>().SetHealthBar(true, 100);
        }
        else
        {
            if (TeamNumber == 0)
            {
                transform.Rotate(0, 0, 0);
                canvas.Rotate(0, 0, 0);
            }
            else
            {
                transform.Rotate(0, 180, 0);
                canvas.Rotate(0, 0, 0);
            }
            canvas.GetComponent<HealthBarControl>().SetHealthBar(false, 100);
        }

        if (IsLocalPlayer)
        {
            canvas.Find("NameTag").GetComponent<Text>().color = Color.white;
        }
        
        GameObject camera = null;
        configured = true;
        select = false;
        Debug.Log("Arrived here..." + TeamNumber + Channel);
        //for local player: initialize scene camera / panels
        if (IsLocalPlayer)
        {
            Debug.Log("I have local player!!! " + "Team" + TeamNumber.ToString() + "_MainCamera" + Channel);
            camera = GameObject.Find("Camera");
            if(camera != null)
            {
                camera.transform.Find(TeamNumber.ToString()).gameObject.SetActive(true);
                camera.transform.Find(TeamNumber.ToString()).gameObject.tag = "MainCamera";
            }

            DashBoard = GameObject.Find("Dashboard");
            Debug.Log("Dashboard: " + DashBoard);
            // Dashboard Setting
            var texts = DashBoard.GetComponentsInChildren<Text>();
            foreach(var t in texts)
            {
                switch (t.name)
                {
                    case "Records": PersonalRecords = t; break;
                    case "TeamFire": Teamfire = t; break;
                }
            }
            DashBoard.GetComponentInChildren<Button>().onClick.AddListener(ScoreBoardClick);
            DashBoard.SetActive(false);

            SelectPanel = GameObject.Find("SelectHero");
            Debug.Log("SelectPanel: " + SelectPanel);
            SelectPanel.GetComponentInChildren<Button>().onClick.AddListener(SelectClick);
            SelectPanel.SetActive(false);

            ActionPanel = GameObject.Find("Action");
            Debug.Log("ActionPanel: " + ActionPanel);
            var actions = ActionPanel.GetComponentsInChildren<Button>();
            foreach(var action in actions)
            {
                action.onClick.AddListener(() => ActionClick(action.name));
            }
            ActionPanel.SetActive(false);

            FinalResultPanel = GameObject.Find("ResultBoard");
            Debug.Log("FinalResultPanel: " + FinalResultPanel);
            FinalResultPanel.SetActive(false);
        }
    }
}
