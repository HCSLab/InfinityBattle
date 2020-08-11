using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class P2PNetworkIdentifier : MonoBehaviour {
    public bool IsLocalPlayer { get; private set; }
    public int TeamNumber;
    public int Channel { get; private set; }
    public string NickName { get; private set; }
    public byte[] PubKey { get; private set; }
    private float Timer;
    public bool IsOpponent { get; private set; }
    public bool SettingDone { get; private set; }
    private Queue<int> waitingList = new Queue<int>();
    private Dictionary<int, string> action = new Dictionary<int, string>();

    GameObject[] Opponents;
    GameObject[] Teammates;

    public void GeneralSetting(GameObject[] Opponents, GameObject[] Teammates)
    {
        this.Opponents = Opponents;
        this.Teammates = Teammates;
    }

    public void LocalPlyaerSetting(int channel, string nickname, byte[] pubkey)
    {
        IsLocalPlayer = true;
        Channel = channel;
        NickName = nickname;
        PubKey = pubkey;
        Timer = float.MaxValue;
        gameObject.SetActive(false);
        IsOpponent = false;
        SettingDone = true;
        DontDestroyOnLoad(gameObject);
    }

    public void CreateNonLocalPlayer(int channel, byte[] pubkey)
    {
        IsLocalPlayer = false;
        Channel = channel;
        PubKey = pubkey;
        Timer = 10;
        IsOpponent = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// TODO: Improve security, required to provide signature instead of pubkey
    /// </summary>
    /// <param name="sign">Currently this field is for its pubkey</param>
    /// <param name="nickname"></param>
    public bool NonLocalPlayerSetting(byte[] sign, string nickname) 
    {
        if(PubKey.SequenceEqual(sign))
        {
            NickName = nickname;
            Timer = float.MaxValue;
            DontDestroyOnLoad(gameObject);
            SettingDone = true;
            return true;
        }
        Debug.Log("Matching failed.. ");
        return false;
    }

    public void OpponentSetting(int channel, string nickname, byte[] pubkey)
    {
        IsLocalPlayer = false;
        Channel = channel;
        NickName = nickname;
        PubKey = pubkey;
        Timer = float.MaxValue;
        gameObject.SetActive(false);
        IsOpponent = true;
        SettingDone = true;
        DontDestroyOnLoad(gameObject);
    }

    public void AddWaitingCommand(int ack, string msg)
    {
        if(action.ContainsKey(ack))
        {
            return;
        }
        waitingList.Enqueue(ack);
        action.Add(ack, msg);
    }

    public string GetCommand()
    {
        return action[waitingList.Dequeue()];
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Timer -= Time.deltaTime;
        if(Timer < 0)
        {
            Destroy(gameObject);
        }
        //hhh = IsLocalPlayer;
        //IsLocalPlayer = hhh;
        //Debug.Log(TeamNumber + " " + Channel + " " + IsLocalPlayer);
	}
}
