using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// This P2P network is only applicable for two teams only
/// </summary>
public class P2PNetworkManager : MonoBehaviour {
    public GameObject PlayerPrefab;
    public int NumberOfTeamPlayers;
    public int port = 7700;
    public string nickname;
    //[Scene]
    public string scene;

    public byte[] PubKey; // encrypt after base64
    private int Channel;
    
    private GameObject[] Teammates;
    private GameObject[] Opponents;

    /// <summary>
    /// Get IP Address 
    /// Default should be external ip address
    /// TODO: If the network is within LAN, using UPnP IGD
    /// </summary>
    /// <returns>IP Address</returns>
    public IPAddress GetIpAddress()
    {
        string hostname = Dns.GetHostName();
        IPHostEntry myIP = Dns.GetHostEntry(hostname);
        IPAddress[] addresses = myIP.AddressList;

        foreach (IPAddress ip in addresses)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork && !IsPrivateNetwork(ip))
            {
                return ip;
            }
        }

        foreach (IPAddress ip in addresses)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }
        }

        return null;
    }

    private uint ConvertIPToInt(IPAddress ip, int length = 32)
    {
        length = Math.Min(length, 32);
        byte[] bytes = ip.GetAddressBytes();
        Array.Reverse(bytes);
        uint address = BitConverter.ToUInt32(bytes, 0);
        return address >> (32 - length);
    }

    private bool IsPrivateNetwork(IPAddress ip)
    {
        if (ConvertIPToInt(ip, 8) == ConvertIPToInt(IPAddress.Parse("10.0.0.0"), 8))
        {
            return true;
        }
        if (ConvertIPToInt(ip, 16) == ConvertIPToInt(IPAddress.Parse("169.254.0.0"), 16))
        {
            return true;
        }
        if (ConvertIPToInt(ip, 12) == ConvertIPToInt(IPAddress.Parse("172.16.0.0"), 12))
        {
            return true;
        }
        if (ConvertIPToInt(ip, 16) == ConvertIPToInt(IPAddress.Parse("192.168.0.0"), 16))
        {
            return true;
        }
        return false;
    }
    // ---- End of searching IP Address ----

    private Socket socket;
    private Dictionary<IPEndPoint, int> destinations;
    private byte[] buffer = new byte[8192];
    private EndPoint sender;
    public bool IsOccupied { get; private set; } //check whether finding potential team
    public bool FinishTeamUp { get; private set; }

    /// <summary>
    /// Initiate all required variables
    /// </summary>
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Teammates = new GameObject[NumberOfTeamPlayers];
        Opponents = new GameObject[NumberOfTeamPlayers];
    }

    /// <summary>
    /// Pre-connect to others is not required
    /// </summary>
    public void Run()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Any, port));
        socket.SendTimeout = 10 * 1000;
        socket.ReceiveTimeout = 10 * 1000;
        destinations = new Dictionary<IPEndPoint, int>();
        IsOccupied = true;
        FinishTeamUp = false;
        Channel = 0;
        Teammates[Channel] = Instantiate(PlayerPrefab);
        Debug.Log(Teammates[Channel].GetComponent<P2PNetworkIdentifier>());
        Teammates[Channel].GetComponent<P2PNetworkIdentifier>().LocalPlyaerSetting(Channel, nickname, PubKey);
        SceneChanged = false;
        MessageList = new Queue<Message>();
        Receivable = true;
        candidatePubKey = null;
        IsMatching = false;
        opRep = null;
        Receive();
    }

    /// <summary>
    /// Pre-connect is required
    /// </summary>
    /// <param name="ipaddr">Destination IP Address</param>
    /// <param name="port">Destination port</param>
    public void Run(string ipaddr, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Any, this.port));
        socket.SendTimeout = 10 * 1000;
        socket.ReceiveTimeout = 10 * 1000;
        destinations = new Dictionary<IPEndPoint, int>();
        IsOccupied = true;
        FinishTeamUp = false;
        SceneChanged = false;
        IsMatching = false;
        opRep = null;
        candidatePubKey = null;
        try
        {
            socket.SendTo(Encoding.ASCII.GetBytes("JoinRequest ").Concat(PubKey).ToArray(), new IPEndPoint(IPAddress.Parse(ipaddr), port));
            Debug.Log("sent join request...");
            buffer = new byte[8192];
            sender = new IPEndPoint(IPAddress.Any, 0);
            int bufferLength = socket.ReceiveFrom(buffer, ref sender);
            if(((IPEndPoint)sender).Address.ToString().CompareTo(ipaddr) == 0 && 
                ((IPEndPoint)sender).Port == port &&
                EstablishConnection(Encoding.ASCII.GetString(buffer, 0, bufferLength))) 
            {
                // Consistent with the one who receive the request
                // Accepted the request from others
                MessageList = new Queue<Message>();
                Receivable = true;
                Receive();
                
            }
            else
            {
                Debug.Log("Reject due to inconsistent IP address");
                Terminate();
            }
        }
        catch (Exception e)
        {
            // failed to connect to others 
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
            Terminate();
        }
    }

    private bool EstablishConnection(string msg)
    {
        Debug.Log(msg);
        string[] seg = msg.Split(' ');
        if (seg[0].CompareTo("Reject") == 0) // You cannot join the group
        {
            return false;
        }
        // Initiate local player
        Channel = int.Parse(seg[1]);
        Teammates[Channel] = Instantiate(PlayerPrefab);
        Teammates[Channel].GetComponent<P2PNetworkIdentifier>().LocalPlyaerSetting(Channel, nickname, PubKey);
        // Initiate non-local players
        for(int i = 2; i < seg.Length; i = i + 4)
        {
            string ipaddr = seg[i];
            int port = int.Parse(seg[i + 1]);
            int channel = int.Parse(seg[i + 2]);
            byte[] pubkey = Encoding.ASCII.GetBytes(seg[i + 3]);
            IPEndPoint receiver = new IPEndPoint(IPAddress.Parse(ipaddr), port);
            try
            {
                destinations.Add(receiver, channel);
                Teammates[channel] = Instantiate(PlayerPrefab);
                Teammates[channel].GetComponent<P2PNetworkIdentifier>().CreateNonLocalPlayer(channel, pubkey);
                SendGreetingMessage(receiver);
                buffer = new byte[8192];
                sender = new IPEndPoint(IPAddress.Any, 0);
                int bufferLength = socket.ReceiveFrom(buffer, ref sender);
                if (((IPEndPoint)sender).Address.ToString().CompareTo(ipaddr) != 0 || ((IPEndPoint)sender).Port != port)
                {
                    return false;
                }
                if(!ProcessGreetingMessage(Encoding.ASCII.GetString(buffer, 0, bufferLength)))
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                destinations.Remove(receiver);
                return false;
            }
        }
        return true;
    }

    private void SendGreetingMessage(EndPoint receiver)
    {
        socket.SendTo(Encoding.ASCII.GetBytes("Greet " + Channel + " " + nickname + " ").Concat(PubKey).ToArray(), receiver);
    }

    private bool ProcessGreetingMessage(string msg)
    {
        Debug.Log("ProcessGreetingMessage(string msg)");
        string[] seg = msg.Split(' ');
        if(seg[0].CompareTo("Greet") != 0)
        {
            return false;
        }
        int channel = int.Parse(seg[1]);
        string nickname = seg[2];
        byte[] pubkey = Encoding.ASCII.GetBytes(seg[3]);
        if(Teammates[channel] == null)
        {
            Debug.Log("HERE HAVE PROBLEM...");
            return false;
        }
        candidatePubKey = null;
        //NewPlayerRequest.Enqueue("UpdateTeammate " + channel + " " + nickname + " " + Encoding.ASCII.GetString(pubkey));
        return Teammates[channel].GetComponent<P2PNetworkIdentifier>().NonLocalPlayerSetting(pubkey, nickname);
        //return true;
    }

    public void Terminate()
    {
        if(destinations == null)
        {
            return;
        }
        
        // Inform all other nodes
        foreach(var dest in destinations)
        {
            socket.SendTo(Encoding.ASCII.GetBytes("Exit"), dest.Key);
        }
        foreach(var teammate in Teammates)
        {
            if (teammate != null)
            {
                Destroy(teammate);
            }
        }
        foreach(var opponent in Opponents)
        {
            if(opponent != null)
            {
                Destroy(opponent);
            }
        }
        Receivable = false;
        if(GetComponent<TurnedBasedNetworkController>() != null)
        {
            Destroy(GetComponent<TurnedBasedNetworkController>());
        }
        socket.Close();
        IsOccupied = false;
        FinishTeamUp = false;
    }
    // ---- End of Initiation ----

    private void Receive()
    {
        buffer = new byte[8192];
        sender = new IPEndPoint(IPAddress.Any, 0);
        socket.BeginReceiveFrom(buffer, 0, buffer.Length, 0, ref sender, new AsyncCallback(ReceiveData), socket);
    }

    private void ReceiveData(IAsyncResult ar)
    {
        try
        {
            sender = new IPEndPoint(IPAddress.Any, 0);
            int bufferLength = socket.EndReceiveFrom(ar, ref sender);
            Debug.Log("ReceivePoint" + Encoding.ASCII.GetString(buffer, 0, bufferLength) + " " + 
                ((IPEndPoint)sender).Address.ToString() + " " + ((IPEndPoint)sender).Port);
            MessageList.Enqueue(new Message(Encoding.ASCII.GetString(buffer, 0, bufferLength), sender));
            
            buffer = new byte[8192];
            sender = new IPEndPoint(IPAddress.Any, 0);
            socket.BeginReceiveFrom(buffer, 0, buffer.Length, 0, ref sender, new AsyncCallback(ReceiveData), socket);
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
            Debug.Log("error raises...");
        }
    }

    // ---- End of Receive ----

    // Each time, only one candidate will be processed in current network
    private byte[] candidatePubKey = null;
    private float candidate_ttl;
    private EndPoint candidate_dest;
    private string candidate_response;
    private int candidate_channel;

    //private Queue<string> NewPlayerRequest = new Queue<string>();

    private bool Receivable = false;
    private class Message
    {
        public string msg;
        public IPEndPoint sender;
        public Message(string msg, EndPoint sender)
        {
            this.msg = msg;
            this.sender = new IPEndPoint(IPAddress.Parse(((IPEndPoint)sender).Address.ToString()), ((IPEndPoint)sender).Port);
        }
    }
    private Queue<Message> MessageList = new Queue<Message>();

    private void ProcessMessage()
    {
        if (!Receivable)
        {
            return;
        }
        while(MessageList.Count > 0)
        {
            Message message = MessageList.Dequeue();
            string msg = message.msg;
            EndPoint sender = message.sender;
            Debug.Log(msg + " " + ((IPEndPoint)sender).Address.ToString() + " " + ((IPEndPoint)sender).Port);
            //Debug.Log("Before...");
            switch (msg.Split(' ')[0])
            {
                case "JoinRequest": ProcessJoinRequest(msg, sender); break;
                case "NewComer": ProcessNewcomer(msg, sender); break;
                case "Greet": ProcessGreetingMessage(msg, sender); break;
                case "Exit": ProcessExitMessage((IPEndPoint)sender); break;
                case "AcceptRequest": case "RejectRequest": ProcessDecisionOnRequest(msg, (IPEndPoint)sender); break;
                case "PendingBattle": case "AcceptBattle": case "RejectBattle": ProcessDecisionOnBattle(msg, (IPEndPoint)sender); break;
                case "AcceptOpponents": case "RejectOpponents": ProcessDecisionOnOpponents(msg, (IPEndPoint)sender); break;
                case "EvaluateOpponents": ProcessEvaluateOpponents(msg, (IPEndPoint)sender); break;
                case "GreetOpponent": ProcessGreetOpponent(msg, (IPEndPoint)sender); break;
                case "EstablishConnection": ProcessEstablishConnection(msg, (IPEndPoint)sender); break;
                case "BattleRequest": ProcessBattleRequest(msg, (IPEndPoint)sender); break;
                case "Behavior": case "Confirmed": ProcessBehaviorAndConfirmed(msg, (IPEndPoint)sender); break;
            }
            //Debug.Log("Done....");
        }
    }

    /// <summary>
    /// In this part, it will process the the command "JoinRequest"
    /// It will send out command "NewComer" 
    /// TODO: Avoid two same pubkey enter
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="sender"></param>
    private void ProcessJoinRequest(string msg, EndPoint sender)
    {
        byte[] pubkey = Encoding.ASCII.GetBytes(msg.Split(' ')[1]);
        if(!EvaluatePeople(pubkey) || FinishTeamUp || candidatePubKey != null)
        {
            socket.SendTo(Encoding.ASCII.GetBytes("Reject"), sender);
            return;
        }
        int i = 0;
        for(; i < NumberOfTeamPlayers; i++)
        {
            if(Teammates[i] == null)
            {
                //Debug.Log(i);
                Teammates[i] = Instantiate(PlayerPrefab);
                Teammates[i].GetComponent<P2PNetworkIdentifier>().CreateNonLocalPlayer(i, pubkey);
                //NewPlayerRequest.Enqueue("AddTeammate " + i + " " + Encoding.ASCII.GetString(pubkey));
                break;
            }
        }
        if (i >= NumberOfTeamPlayers)
        {
            socket.SendTo(Encoding.ASCII.GetBytes("Reject"), sender);
            return;
        }
        Debug.Log("Accept the guys. ");
        candidatePubKey = pubkey;
        candidate_channel = i;
        candidate_ttl = 20.0f;
        candidate_dest = new IPEndPoint(IPAddress.Parse(((IPEndPoint)sender).Address.ToString()), ((IPEndPoint)sender).Port);
        candidate_response = "Accept " + candidate_channel + " " + 
            GetIpAddress() + " " + this.port + " " + Channel + " " + Encoding.ASCII.GetString(PubKey);
        byte[] query = Encoding.ASCII.GetBytes("NewComer " + candidate_channel + " ").Concat(pubkey).ToArray();
        if(destinations.Count == 0)
        {
            socket.SendTo(Encoding.ASCII.GetBytes(candidate_response), sender);
            return;
        }
        foreach (var dest in destinations)
        {
            socket.SendTo(query, dest.Key);
        }
    }

    /// <summary>
    /// In this part, it will process command "Newcomer"
    /// It will send out the command "AcceptRequest" or "RejectRequest"
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="sender"></param>
    private void ProcessNewcomer(string msg, EndPoint sender)
    {
        Debug.Log("ProcessNewcomer now...");
        int channel = int.Parse(msg.Split(' ')[1]);
        byte[] pubkey = Encoding.ASCII.GetBytes(msg.Split(' ')[2]);
        if(!EvaluatePeople(pubkey) || Teammates[channel] != null || candidatePubKey != null)
        {
            socket.SendTo(Encoding.ASCII.GetBytes("RejectRequest ").Concat(pubkey).ToArray(), sender);
            return;
        }
        Debug.Log("Accept the newcomer... ");
        //NewPlayerRequest.Enqueue("AddTeammate " + channel + " " + Encoding.ASCII.GetString(pubkey));
        Teammates[channel] = Instantiate(PlayerPrefab);
        Teammates[channel].GetComponent<P2PNetworkIdentifier>().CreateNonLocalPlayer(channel, pubkey);
        Debug.Log("sent accept to " + ((IPEndPoint)sender).Address.ToString() + " " + ((IPEndPoint)sender).Port);
        socket.SendTo(Encoding.ASCII.GetBytes("AcceptRequest " + msg.Split(' ')[2]), sender);
    }

    /// <summary>
    /// In this part, it will process the command "Greet"
    /// and send out the response command
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="sender"></param>
    private void ProcessGreetingMessage(string msg, EndPoint sender)
    {
        //Debug.Log("ProcessGreetingMessage(string msg, EndPoint sender)");
        if (ProcessGreetingMessage(msg))
        {
            //Debug.Log("finish evaluate...");
            string ipaddr = ((IPEndPoint)sender).Address.ToString();
            int port = ((IPEndPoint)sender).Port;

            destinations.Add(new IPEndPoint(IPAddress.Parse(ipaddr), port), int.Parse(msg.Split(' ')[1]));
            SendGreetingMessage(sender);
        }
    }

    /// <summary>
    /// In this part, it will process the command "Exit"
    /// </summary>
    /// <param name="sender"></param>
    private void ProcessExitMessage(IPEndPoint sender)
    {
        int channel = FindPeople(sender);
        Debug.Log(channel + " exit");
        if(candidatePubKey != null)
        {
            candidate_response.Replace(" " + sender.Address.ToString() + " " + sender.Port + " " + channel + " " +
                Encoding.ASCII.GetString(Teammates[channel].GetComponent<P2PNetworkIdentifier>().PubKey), "");
        }
        if (channel != -1)
        {
            //NewPlayerRequest.Enqueue("Destroy " + channel);
            Destroy(Teammates[channel]);
            foreach (var dest in destinations)
            {
                if(dest.Key.Address.ToString().CompareTo(sender.Address.ToString()) == 0 && dest.Key.Port == sender.Port)
                {
                    destinations.Remove(dest.Key);
                    break;
                }
            }
        }
        FinishTeamUp = false;
    }

    /// <summary>
    /// In this part, it will process the command "AcceptRequest" and "RejectRequest"
    /// If "RejectRequest", send "Reject" back
    /// If "AcceptRequest", wait until all done
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="sender"></param>
    private void ProcessDecisionOnRequest(string msg, IPEndPoint sender)
    {
        int channel = FindPeople(sender);
        if (channel == -1)
        {
            return;
        }
        string decision = msg.Split(' ')[0];
        string pubkey = msg.Split(' ')[1];
        if(!candidatePubKey.SequenceEqual(Encoding.ASCII.GetBytes(pubkey)))
        {
            return;
        }
        if (decision.CompareTo("RejectRequest") == 0)
        {
            socket.SendTo(Encoding.ASCII.GetBytes("Reject"), candidate_dest);
            //NewPlayerRequest.Enqueue("Destroy " + candidate_channel);
            Destroy(Teammates[candidate_channel]);
            candidatePubKey = null;
        }
        else
        {
            if(candidate_response.Contains(sender.Address.ToString() + " " + sender.Port))
            {
                return;
            }
            candidate_response = candidate_response + " " +
                sender.Address.ToString() + " " + sender.Port + " " + channel + " " +
                Encoding.ASCII.GetString(Teammates[channel].GetComponent<P2PNetworkIdentifier>().PubKey);
            if(candidate_response.Split(' ').Length == 4 * NumOfPlayer() + 2)
            {
                socket.SendTo(Encoding.ASCII.GetBytes(candidate_response), candidate_dest);
            }
        }
    }

    public bool IsMatching { get; private set; }
    private IPEndPoint opRep;
    private float BattleTTL;
    private string BattleResponse;
    private int gamingID;
    private int index;
    private bool IsResponseRequired;
    private bool IsAccepted;
    private string broadcast_msg;

    /// <summary>
    /// In this part, it will process the command "BattleRequest" 
    /// It sends out the command "PendingBattle" and "EvaluateOpponents"
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="sender"></param>
    private void ProcessBattleRequest(string msg, IPEndPoint sender)
    {
        if (IsMatching || !EvaluateOpponents(msg.Replace("BattleRequest", "EvaluateOpponents xxxxx")))
        {
            socket.SendTo(Encoding.ASCII.GetBytes("RejectBattle"), sender);
        }
        index = (new System.Random()).Next();
        Debug.Log("Match ID:" + index);
        gamingID = index;
        for (int i = 0; i < NumberOfTeamPlayers; i++)
        {
            Teammates[i].GetComponent<P2PNetworkIdentifier>().TeamNumber = index % 2;
        }
        BattleResponse = "AcceptBattle " + (index+1) + " " + GetIpAddress().ToString() + " " + port;
        BattleTTL = 30.0f;
        IsMatching = true;
        IsResponseRequired = true;
        string response = "PendingBattle " + (index+1) + " " + GetIpAddress().ToString() + " " + Encoding.ASCII.GetString(PubKey);
        opRep = new IPEndPoint(IPAddress.Parse(sender.Address.ToString()), sender.Port);
        foreach(var dest in destinations)
        {
            response = response + " " + dest.Key.Address.ToString() + " " + 
                Encoding.ASCII.GetString(Teammates[dest.Value].GetComponent<P2PNetworkIdentifier>().PubKey);
            socket.SendTo(Encoding.ASCII.GetBytes(msg.Replace("BattleRequest", "EvaluateOpponents " + index)), dest.Key);
        }
        socket.SendTo(Encoding.ASCII.GetBytes(response), opRep);
        if(destinations == null || destinations.Count == 0)
        {
            socket.SendTo(Encoding.ASCII.GetBytes(BattleResponse), opRep);
        }
    }

    public void SendBattleRequest(string ipaddr, int port)
    {
        string response = "BattleRequest " + GetIpAddress().ToString() + " " + Encoding.ASCII.GetString(PubKey);
        foreach (var dest in destinations)
        {
            response = response + " " + dest.Key.Address.ToString() + " " +
                Encoding.ASCII.GetString(Teammates[dest.Value].GetComponent<P2PNetworkIdentifier>().PubKey);
        }
        socket.SendTo(Encoding.ASCII.GetBytes(response), new IPEndPoint(IPAddress.Parse(ipaddr), port));
    }

    private void ProcessEvaluateOpponents(string msg, EndPoint sender)
    {
        if (EvaluateOpponents(msg))
        {
            socket.SendTo(Encoding.ASCII.GetBytes("AcceptOpponents " + msg.Split(' ')[1]), sender);
            IsMatching = true;
            index = int.Parse(msg.Split(' ')[1]);
            //Debug.Log("current Team Number: " + ((index+1) % 2));
            for (int i = 0; i < NumberOfTeamPlayers; i++)
            {
                Teammates[i].GetComponent<P2PNetworkIdentifier>().TeamNumber = index % 2;
            }
            BattleTTL = float.MaxValue;
        }
        else
        {
            socket.SendTo(Encoding.ASCII.GetBytes("RejectOpponents " + msg.Split(' ')[1]), sender);
        }
    }

    /// <summary>
    /// It will process the command "RejectOpponents", "AcceptOpponents"
    /// Send out "RejectBattle", "AcceptBattle"
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="sender"></param>
    private void ProcessDecisionOnOpponents(string msg, IPEndPoint sender)
    {
        if(!IsMatching || int.Parse(msg.Split(' ')[1]) != index)
        {
            return;
        }
        if(msg.Split(' ')[0].CompareTo("RejectOpponents") == 0)
        {
            IsMatching = false;
            if (IsResponseRequired)
            {
                socket.SendTo(Encoding.ASCII.GetBytes("RejectBattle"), opRep);
            }
            foreach(var dest in destinations)
            {
                socket.SendTo(Encoding.ASCII.GetBytes("RejectOpponents " + index), dest.Key);
            }
        }
        else if (msg.Split(' ')[0].CompareTo("AcceptOpponents") == 0)
        {
            if(!BattleResponse.Contains(sender.Address + " " + sender.Port))
            {
                BattleResponse = BattleResponse + " " + sender.Address + " " + sender.Port;
            }
            if(BattleResponse.Split(' ').Length == 2 * NumberOfTeamPlayers + 2)
            {
                if (IsResponseRequired)
                {
                    socket.SendTo(Encoding.ASCII.GetBytes(BattleResponse), opRep);
                }
                else
                {
                    if (IsAccepted)
                    {
                        foreach(var dest in destinations)
                        {
                            //broadcast_msg.Replace("AcceptBattle", "EstablishConnection");
                            socket.SendTo(Encoding.ASCII.GetBytes(broadcast_msg), dest.Key);
                        }
                        ProcessEstablishConnection(broadcast_msg, null);
                    }
                    else
                    {
                        IsAccepted = true;
                    }
                }
                BattleTTL = 30.0f;
            }
        }
    }

    /// <summary>
    /// It will process the command "PengdingBattle", "RejectBattle", "AcceptBattle"
    /// Send out "EvaluateOpponents", "RejectOpponents", "EstablishConnection"
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="sender"></param>
    private void ProcessDecisionOnBattle(string msg, IPEndPoint sender)
    {
        if (msg.Split(' ')[0].CompareTo("PendingBattle") == 0 && !IsMatching)
        {
            if (!EvaluateOpponents(msg))
            {
                return;
            }
            index = int.Parse(msg.Split(' ')[1]);
            gamingID = index - 1;
            for (int i = 0; i < NumberOfTeamPlayers; i++)
            {
                Teammates[i].GetComponent<P2PNetworkIdentifier>().TeamNumber = index % 2;
            }
            BattleResponse = "AcceptBattle " + index + " " + GetIpAddress().ToString() + " " + port;
            BattleTTL = 30.0f;
            IsMatching = true;
            IsResponseRequired = false;
            IsAccepted = false;
            opRep = new IPEndPoint(IPAddress.Parse(sender.Address.ToString()), sender.Port);
            foreach (var dest in destinations)
            {
                socket.SendTo(Encoding.ASCII.GetBytes(msg.Replace("PendingBattle", "EvaluateOpponents")), dest.Key);
            }
            if(destinations == null || destinations.Count == 0)
            {
                IsAccepted = true;
            }
        }
        else if (msg.Split(' ')[0].CompareTo("RejectBattle") == 0)
        {
            IsMatching = false;
            foreach (var dest in destinations)
            {
                socket.SendTo(Encoding.ASCII.GetBytes("RejectOpponents " + index), dest.Key);
            }
        }
        else if (msg.Split(' ')[0].CompareTo("AcceptBattle") == 0)
        {
            broadcast_msg = msg.Replace("AcceptBattle", "EstablishConnection");
            if (IsAccepted)
            {
                foreach (var dest in destinations)
                {
                    //broadcast_msg.Replace("AcceptBattle", "EstablishConnection");
                    socket.SendTo(Encoding.ASCII.GetBytes(broadcast_msg), dest.Key);
                }
                ProcessEstablishConnection(broadcast_msg, null);
            }
            else
            {
                IsAccepted = true;
            }
        }
    }

    /// <summary>
    /// It will process the command "EstablishConnection"
    /// Send out the command "GreetOpponent"
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="sender"></param>
    private void ProcessEstablishConnection(string msg, EndPoint sender)
    {
        //Debug.Log("current Team Number: " + (index % 2));
        //for (int i = 0; i < NumberOfTeamPlayers; i++)
        //{
        //    Teammates[i].GetComponent<P2PNetworkIdentifier>().TeamNumber = index % 2;
        //}
        string[] seg = msg.Split(' ');
        for(int i = 0; i < NumberOfTeamPlayers; i++)
        {
            socket.SendTo(Encoding.ASCII.GetBytes("GreetOpponent " + index + " " + Channel + " " + nickname + " ").Concat(PubKey).ToArray(), 
                new IPEndPoint(IPAddress.Parse(seg[i * 2 + 2]), int.Parse(seg[i * 2 + 3])));
        }
    }

    /// <summary>
    /// It will process the command "GreetOpponent"
    /// Reply with "GreetOpponent"
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="sender"></param>
    private void ProcessGreetOpponent(string msg, EndPoint sender)
    {
        string[] seg = msg.Split(' ');
        int temp = int.Parse(seg[1]);
        if (Mathf.Abs(temp - index) == 1)
        {
            gamingID = Mathf.Min(temp, index);
            int channel = int.Parse(seg[2]);
            string nickname = seg[3];
            byte[] pubkey = Encoding.ASCII.GetBytes(seg[4]);
            int teamnumber = temp % 2;
            string ipaddr = ((IPEndPoint)sender).Address.ToString();
            int port = ((IPEndPoint)sender).Port;
            destinations.Add(new IPEndPoint(IPAddress.Parse(ipaddr), port), channel + NumberOfTeamPlayers);
            //NewPlayerRequest.Enqueue("AddOpponent " + teamnumber + " " + channel + " " + nickname + " " + Encoding.ASCII.GetString(pubkey));
            Opponents[channel] = Instantiate(PlayerPrefab);
            Opponents[channel].GetComponent<P2PNetworkIdentifier>().OpponentSetting(channel, nickname, pubkey);
            Opponents[channel].GetComponent<P2PNetworkIdentifier>().TeamNumber = teamnumber;
            if (index < temp)
            {
                socket.SendTo(Encoding.ASCII.GetBytes("GreetOpponent " + index + " " + 
                    Channel + " " + this.nickname + " ").Concat(PubKey).ToArray(), sender);
            }
        }
    }

    private void ProcessBehaviorAndConfirmed(string msg, IPEndPoint sender)
    {
        if (GetComponent<TurnedBasedNetworkController>() == null)
        {
            return;
        }
        if (msg.Split(' ')[0].CompareTo("Behavior") == 0)
        {
            GetComponent<TurnedBasedNetworkController>().ProcessBehavior(msg, sender);
        }
        else
        {
            GetComponent<TurnedBasedNetworkController>().ProcessConfirmed(msg, sender);
        }
    }

    // ---- End of Processing ----

    private int NumOfPlayer()
    {
        int num = 0;
        foreach(var player in Teammates)
        {
            if (player != null && player.GetComponent<P2PNetworkIdentifier>().SettingDone)
            {
                num += 1;
            }
        }
        return num;
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
        if(channel == -1)
        {
            return null;
        }
        else if(channel / NumberOfTeamPlayers == 0)
        {
            return Teammates[channel];
        }
        else
        {
            return Opponents[channel % NumberOfTeamPlayers];
        }
    }

    /// <summary>
    /// This function is to evaluate whether a player is reliable
    /// </summary>
    /// <param name="pubkey"></param>
    /// <returns>true means the people is reliable</returns>
    private bool EvaluatePeople(byte[] pubkey)
    {
        return true;
    }

    /// <summary>
    /// This function is to evaluate whether two players are friends
    /// </summary>
    /// <returns>true means they are friends</returns>
    private bool EvaluateFriends(string aip, byte[] apubkey, string bip, byte[] bpubkey)
    {
        return false;
        if(aip.CompareTo(bip) == 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Evaluate the opponent
    /// true means the opponents are valid
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    private bool EvaluateOpponents(string msg)
    {
        string[] seg = msg.Split(' ');
        for (int i = 0; i < NumberOfTeamPlayers; i++)
        {
            if (!EvaluatePeople(Encoding.ASCII.GetBytes(seg[i * 2 + 3])))
            {
                return false;
            }
            foreach (var dest in destinations)
            {
                if (EvaluateFriends(seg[i * 2 + 2], Encoding.ASCII.GetBytes(seg[i * 2 + 3]),
                    dest.Key.Address.ToString(), Teammates[dest.Value].GetComponent<P2PNetworkIdentifier>().PubKey))
                {
                    return false;
                }
            }
        }
        return true;
    }

    // ---- End of Evaluation ----

    private bool SceneChanged;
    /// <summary>
    /// This part is to update some necessary part
    /// </summary>
    public void Update()
    {
        ProcessMessage();
        candidate_ttl -= Time.deltaTime;
        if (candidate_ttl < 0)
        {
            candidatePubKey = null;
        }
        for (int i = 0; i < NumberOfTeamPlayers; i++)
        {
            if (Teammates[i] == null || !Teammates[i].GetComponent<P2PNetworkIdentifier>().SettingDone)
            {
                FinishTeamUp = false;
                return;
            }
        }
        FinishTeamUp = true; // If current team is full
        if (IsMatching)
        {
            BattleTTL -= Time.deltaTime;
            if(BattleTTL < 0)
            {
                IsMatching = false;
                foreach (var dest in destinations)
                {
                    socket.SendTo(Encoding.ASCII.GetBytes("RejectOpponents " + index), dest.Key);
                }
                return;
            }
        }
        for (int i = 0; i < NumberOfTeamPlayers; i++)
        {
            if (Opponents[i] == null || !Opponents[i].GetComponent<P2PNetworkIdentifier>().SettingDone)
            {
                return;
            }
        }
        // Everything is done, load new scene is allowed
        if (!SceneChanged)
        {
            Debug.Log("Everything is done. Enter the battle field. ");
            Debug.Log(SceneManager.GetActiveScene().buildIndex);
            SceneChanged = true;
            // IsMatching = false;
            BattleTTL = float.MaxValue;
            // StartCoroutine(LoadScene());
            GameObject.Find("LevelLoader").GetComponent<LevelLoader>().LoadLevel(scene);
            gameObject.AddComponent<TurnedBasedNetworkController>().init(socket, gamingID, Channel, Teammates, Opponents, destinations);
        }
    }

    IEnumerator LoadScene()
    {
        yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        //SceneManager.LoadScene(scene.buildIndex, LoadSceneMode.Single);
        gameObject.AddComponent<TurnedBasedNetworkController>().init(socket, gamingID, Channel, Teammates, Opponents, destinations);
    }
}
