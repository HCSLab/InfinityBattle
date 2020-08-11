using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TurnedBasedNetworkController : MonoBehaviour {
    private GameObject[] Teammates;
    private GameObject[] Opponents;

    public int index = -1;
    private GameObject[] players;
    private GameObject me;

    private Socket socket;
    private Dictionary<IPEndPoint, int> destinations;
    private int NumberOfTeamPlayers;

    private int MatchID;
    private int Teamnum;
    private int Channel;

    private int ack;
    private string msg;
    private List<int> receivedlist;

    public int round;

    public int teamfires { private set; get; }
    private int opponentFires;

    private List<string> procedure = new List<string>();

    public void init(Socket socket, int id, int Channel, GameObject[] Teammates, GameObject[] Opponents, Dictionary<IPEndPoint, int> destinations)
    {
        Debug.Log("Initial");
        this.socket = socket;
        this.Channel = Channel;
        MatchID = id;
        Teamnum = Teammates[Channel].GetComponent<P2PNetworkIdentifier>().TeamNumber;
        this.Teammates = Teammates;
        this.Opponents = Opponents;
        me = Teammates[Channel];
        this.destinations = destinations;
        NumberOfTeamPlayers = Teammates.Length;
        players = new GameObject[NumberOfTeamPlayers * 2];
        ack = (new System.Random()).Next();
        round = 0;
        BanList = new List<string>();
    }

    private bool gameEnded;

    public void Setting()
    {
        Debug.Log("start setting...");
        gameEnded = false;

        foreach(var player in Teammates)
        {
            player.SetActive(true);
            player.GetComponent<PlayerController>().enabled = true;
            player.SetActive(false);
        }

        foreach(var player in Opponents)
        {
            player.SetActive(true);
            player.GetComponent<PlayerController>().enabled = true;
            player.SetActive(false);
        }

        if(Teamnum == 0)
        {
            for(int i = 0; i< NumberOfTeamPlayers; i++)
            {
                players[i * 2] = Teammates[i];
                players[i * 2 + 1] = Opponents[i];
            }
        }
        else
        {
            for(int i = 0; i < NumberOfTeamPlayers; i++)
            {
                
                players[i * 2] = Opponents[i];
                players[i * 2 + 1] = Teammates[i];
            }
        }

        //Debug.Log("Configuration done... ");
        NextOne();
    }

    private bool GameEnd()
    {
        if (gameEnded)
        {
            return true;
        }
        bool flag = true;
        Debug.Log("Checking for Opponents");
        foreach(var opponent in Opponents)
        {
            if (!opponent.GetComponent<PlayerController>().isDeath())
            {
                flag = false;
                break;
            }
        }
        if (flag)
        {
            foreach(var opponent in Opponents)
            {
                opponent.GetComponent<PlayerController>().loss();
            }
            foreach(var teammate in Teammates)
            {
                teammate.GetComponent<PlayerController>().victory();
            }
            EndGameBoard(true);
            return true;
        }
        flag = true;
        Debug.Log("Checking for Teammates");
        foreach (var teammate in Teammates)
        {
            if (!teammate.GetComponent<PlayerController>().isDeath())
            {
                flag = false;
                break;
            }
        }
        if (flag)
        {
            foreach (var opponent in Opponents)
            {
                opponent.GetComponent<PlayerController>().victory();
            }
            foreach (var teammate in Teammates)
            {
                teammate.GetComponent<PlayerController>().loss();
            }
            EndGameBoard(false);
            return true;
        }
        return false;
    }

    private void EndGameBoard(bool win)
    {
        GameObject.Find("Result").GetComponent<ResultBoardConfigure>().SetResult(win, getRecords());
        GameObject.Find("Result").GetComponent<ResultBoardConfigure>().SubmitResult(
            GameObject.Find("NetworkManager").GetComponent<P2PNetworkManager>().port, getMatchID(), getProcedure(),
            getRecords(true), getIPs());
    }

    private string[] getIPs()
    {
        string[] addrs = new string[destinations.Count];
        int i = 0;
        foreach(var addr in destinations.Keys)
        {
            addrs[i++] = addr.ToString();
        }
        return addrs;
    }

    private string[] getProcedure()
    {
        Debug.Log(procedure);
        return procedure.ToArray();
    }

    private List<string> BanList;
    public void NextOne()
    {
        index++;
        if (index == players.Length)
        {
            index = 0;
            round++;
            teamfires = Mathf.Min(Mathf.Max(5, round / 4), 8);
            opponentFires = Mathf.Min(Mathf.Max(5, round / 4), 8);
        }
        while (!players[index].GetComponent<PlayerController>().Operatable())
        {
            index++;
            if (index == players.Length)
            {
                index = 0;
                round++;
                teamfires = Mathf.Min(Mathf.Max(5, round / 4), 8);
                opponentFires = Mathf.Min(Mathf.Max(5, round / 4), 8);
            }
        }
        players[index].GetComponent<PlayerController>().SetToMyTurn();
        if(players[index] == me)
        {
            Debug.Log("Now this is my turn.. ");
            if(round == 0)
            {
                // select the hero
                players[index].GetComponent<PlayerController>().SelectHero(BanList);
                players[index].SetActive(true);
            }
            else
            {
                // attack
                foreach(var opponent in Opponents)
                {
                    opponent.GetComponent<PlayerController>().SetShowing(true);
                }
                players[index].GetComponent<PlayerController>().OperateAction(teamfires);
            }
        }
    }

    public string getRecords()
    {
        string records = "";
        records += "Teammates: \n";
        for(int i = 0; i < NumberOfTeamPlayers; i++)
        {
            records =  records + Teammates[i].GetComponent<P2PNetworkIdentifier>().NickName + "\t"
                + Teammates[i].GetComponent<PlayerController>().getRecord() + "\n";
        }
        records += "Opponents: \n";
        for(int i = 0; i < NumberOfTeamPlayers; i++)
        {
            records = records + Opponents[i].GetComponent<P2PNetworkIdentifier>().NickName + "\t" 
                + Opponents[i].GetComponent<PlayerController>().getRecord() + "\n";
        }
        return records;
    }

    public int getMatchID()
    {
        return MatchID;
    }

    public Dictionary<string, object> getRecords(bool showPubKey)
    {
        Dictionary<string, object> records = new Dictionary<string, object>();
        //records.Add("MatchID", MatchID);
        if(Teamnum == 0)
        {
            for (int i = 0; i < NumberOfTeamPlayers; i++)
            {
                records.Add(Encoding.ASCII.GetString(Teammates[i].GetComponent<P2PNetworkIdentifier>().PubKey),
                    Teammates[i].GetComponent<PlayerController>().getRecord(true));
            }
            for (int i = 0; i < NumberOfTeamPlayers; i++)
            {
                records.Add(Encoding.ASCII.GetString(Opponents[i].GetComponent<P2PNetworkIdentifier>().PubKey),
                    Opponents[i].GetComponent<PlayerController>().getRecord(true));
            }
        }
        else
        {
            for (int i = 0; i < NumberOfTeamPlayers; i++)
            {
                records.Add(Encoding.ASCII.GetString(Opponents[i].GetComponent<P2PNetworkIdentifier>().PubKey),
                    Opponents[i].GetComponent<PlayerController>().getRecord(true));
            }
            for (int i = 0; i < NumberOfTeamPlayers; i++)
            {
                records.Add(Encoding.ASCII.GetString(Teammates[i].GetComponent<P2PNetworkIdentifier>().PubKey),
                    Teammates[i].GetComponent<PlayerController>().getRecord(true));
            }
        }
        // temp use --- convert to json file
        //string json = JsonConvert.SerializeObject(records, Formatting.Indented);
        //string json = JsonUtility.ToJson(records, true);
        //System.IO.File.WriteAllText(me.GetComponent<P2PNetworkIdentifier>().NickName + ".result", json);
        // temp use ends
        return records;
    }

    public void ProcessBehavior(string msg, IPEndPoint sender)
    {
        int ack = int.Parse(msg.Split(' ')[1]);
        // FindGameobject(sender).GetComponent<P2PNetworkIdentifier>().AddWaitingCommand(ack, msg.Replace("Behavior " + ack + " ", ""));
        socket.SendTo(Encoding.ASCII.GetBytes("Confirmed " + ack), sender);

        if(round == 0)
        {
            // process selection
            if(FindGameobject(sender) == players[index])
            {
                Debug.Log("Processing selection...");
                string hero = msg.Split(' ')[2];
                if (BanList.Contains(hero))
                {
                    return;
                }
                players[index].GetComponent<PlayerController>().SelectHero(hero);
                BanList.Add(hero);
                players[index].SetActive(true);
                NextOne();
            }
        }
        else
        {
            // process real gaming behavior
            if(FindGameobject(sender) == players[index])
            {
                Debug.Log("Processing behavior...");
                int temp = FindPeople(sender);
                string action = msg.Split(' ')[2];
                int channel = int.Parse(msg.Split(' ')[3]);
                if(temp >= NumberOfTeamPlayers)
                {
                    // judge action
                    if(players[index].GetComponent<PlayerController>().getRole().GetComponent<HeroAttributes>().getConsumed(action) > opponentFires)
                    {
                        return;
                    }
                    opponentFires -= players[index].GetComponent<PlayerController>().getRole().GetComponent<HeroAttributes>().getConsumed(action);
                    players[index].GetComponent<PlayerController>().OperateAction(action, Teammates[channel], Teammates, Opponents);
                    procedure.Add(players[index].GetComponent<PlayerController>().getRole().name + " " + action + " " + 
                        Teammates[channel].GetComponent<PlayerController>().getRole().name);
                }
                else
                {
                    if (players[index].GetComponent<PlayerController>().getRole().GetComponent<HeroAttributes>().getConsumed(action) > teamfires)
                    {
                        return;
                    }
                    teamfires -= players[index].GetComponent<PlayerController>().getRole().GetComponent<HeroAttributes>().getConsumed(action);
                    players[index].GetComponent<PlayerController>().OperateAction(action, Opponents[channel], Opponents, Teammates);
                    procedure.Add(players[index].GetComponent<PlayerController>().getRole().name + " " + action + " " + 
                        Opponents[channel].GetComponent<PlayerController>().getRole().name);
                }
                NextOne();
            }
        }

    }

    public void SendBehavior(string msg)
    {
        if(players[index] != me)
        {
            return;
        }
        if(round == 0)
        {
            BanList.Add(msg);
        }
        else
        {
            foreach (var opponent in Opponents)
            {
                opponent.GetComponent<PlayerController>().SetShowing(false);
            }
            string action = msg.Split(' ')[0];
            int channel = int.Parse(msg.Split(' ')[1]);
            if (players[index].GetComponent<PlayerController>().getRole().GetComponent<HeroAttributes>().getConsumed(action) > teamfires)
            {
                return;
            }
            teamfires -= players[index].GetComponent<PlayerController>().getRole().GetComponent<HeroAttributes>().getConsumed(action);
            players[index].GetComponent<PlayerController>().OperateAction(action, Opponents[channel], Opponents, Teammates);
            procedure.Add(players[index].GetComponent<PlayerController>().getRole().name + " " + action + " " +
                Opponents[channel].GetComponent<PlayerController>().getRole().name);
        }
        foreach(var dest in destinations)
        {
            socket.SendTo(Encoding.ASCII.GetBytes("Behavior " + ack + " " + msg), dest.Key);
        }
        NextOne();
    }

    public void ProcessConfirmed(string msg, IPEndPoint sender)
    {

    }

    private int FindPeople(IPEndPoint person)
    {
        foreach (var dest in destinations)
        {
            if (dest.Key.Address.ToString().Equals(person.Address.ToString()) && dest.Key.Port == person.Port)
            {
                return dest.Value;
            }
        }
        return -1;
    }

    private GameObject FindGameobject(IPEndPoint person)
    {
        int channel = FindPeople(person);
        if (channel == -1)
        {
            return null;
        }
        else if (channel / NumberOfTeamPlayers == 0)
        {
            return Teammates[channel];
        }
        else
        {
            return Opponents[channel % NumberOfTeamPlayers];
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    private bool SettingDone = false;

	// Update is called once per frame
	void Update () {
		if(!SettingDone && SceneManager.GetSceneByName("BattleField").isLoaded)
        {
            SettingDone = true;
            Setting();
        }
        if (round != 0 && GameEnd())
        {
            players[0] = null;
            index = 0;
            gameEnded = true;
            return;
        }
    }
}
