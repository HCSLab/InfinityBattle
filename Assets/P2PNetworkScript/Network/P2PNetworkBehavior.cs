using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2PNetworkBehavior : MonoBehaviour {
    protected bool IsLocalPlayer
    {
        get
        {
            return GetComponent<P2PNetworkIdentifier>().IsLocalPlayer;
        }
    }

    protected string NickName
    {
        get
        {
            return GetComponent<P2PNetworkIdentifier>().NickName;
        }
    }

    protected bool IsOpponent
    {
        get
        {
            return GetComponent<P2PNetworkIdentifier>().IsOpponent;
        }
    }

    protected int TeamNumber
    {
        get
        {
            return GetComponent<P2PNetworkIdentifier>().TeamNumber;
        }
    }

    protected int Channel
    {
        get
        {
            return GetComponent<P2PNetworkIdentifier>().Channel;
        }
    }

    protected string NextMessage
    {
        get
        {
            return GetComponent<P2PNetworkIdentifier>().GetCommand();
        }
    }
}
