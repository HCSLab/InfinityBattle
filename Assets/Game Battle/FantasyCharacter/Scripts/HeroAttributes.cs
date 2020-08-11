using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroAttributes : MonoBehaviour {
    public float FilledBlood;

    public Button AttackButton;
    public bool AttackIsFar;
    public float AttackHarmToOpponent;
    public bool AttackHarmAll;
    public float AttackRecoverToSelf;
    public float AttackRecoverToTeammate;
    public int AttackConsumed = 0;
    public int AttackExpectedFrame = 40;

    public Button MagicButton;
    public bool MagicIsFar;
    public float MagicHarmToOpponent;
    public bool MagicHarmAll;
    public float MagicRecoverToSelf;
    public float MagicRecoverToTeammate;
    public int MagicConsumed = 1;
    public int MagicExpectedFrame = 50;

    public Button Magic2Button;
    public bool Magic2IsFar;
    public float Magic2HarmToOpponent;
    public bool Magic2HarmAll;
    public float Magic2RecoverToSelf;
    public float Magic2RecoverToTeammate;
    public int Magic2Consumed = 2;
    public int Magic2ExpectedFrame = 80;

    public Button UltimateButton;
    public bool UltimateIsFar;
    public float UltimateHarmToOpponent;
    public bool UltimateHarmAll;
    public float UltimateRecoverToSelf;
    public float UltimateRecoverToTeammate;
    public int UltimateConsumed = 4;
    public int UltimateExpectedFrame = 100;

    public GameObject Target;

    public float getAttackAmount(string name)
    {
        switch (name)
        {
            case "Attack": return AttackHarmToOpponent;
            case "Magic": return MagicHarmToOpponent;
            case "Magic2": return Magic2HarmToOpponent;
            case "Ultimate": return UltimateHarmToOpponent;
            default: return 0;
        }
    }

    public bool getAttackHarmToAll(string name)
    {
        switch (name)
        {
            case "Attack": return AttackHarmAll;
            case "Magic": return MagicHarmAll;
            case "Magic2": return Magic2HarmAll;
            case "Ultimate": return UltimateHarmAll;
            default: return false;
        }
    }

    public float getRecoverToSelf(string name)
    {
        switch (name)
        {
            case "Attack": return AttackRecoverToSelf;
            case "Magic": return MagicRecoverToSelf;
            case "Magic2": return Magic2RecoverToSelf;
            case "Ultimate": return UltimateRecoverToSelf;
            default: return 0;
        }
    }

    public float getRecoverToAll(string name)
    {
        switch (name)
        {
            case "Attack": return AttackRecoverToTeammate;
            case "Magic": return MagicRecoverToTeammate;
            case "Magic2": return Magic2RecoverToTeammate;
            case "Ultimate": return UltimateRecoverToTeammate;
            default: return 0;
        }
    }

    public int getConsumed(string name)
    {
        switch (name)
        {
            case "Attack": return AttackConsumed;
            case "Magic": return MagicConsumed;
            case "Magic2": return Magic2Consumed;
            case "Ultimate": return UltimateConsumed;
            default: return 0;
        }
    }

    public bool isFar(string name)
    {
        switch (name)
        {
            case "Attack": return AttackIsFar;
            case "Magic": return MagicIsFar;
            case "Magic2": return Magic2IsFar;
            case "Ultimate": return UltimateIsFar;
            default: return false;
        }
    }

    public int getExpectedFrame(string name)
    {
        switch (name)
        {
            case "Attack": return AttackExpectedFrame;
            case "Magic": return MagicExpectedFrame;
            case "Magic2": return Magic2ExpectedFrame;
            case "Ultimate": return UltimateExpectedFrame;
            default: return 0;
        }
    }

    private void Start()
    {
        if(AttackButton != null)
        {
            AttackButton.onClick.AddListener(() => GetComponentInParent<PlayerController>().SetPendingAction("Attack"));
        }
        if (MagicButton != null)
        {
            MagicButton.onClick.AddListener(() => GetComponentInParent<PlayerController>().SetPendingAction("Magic"));
        }
        if (Magic2Button != null)
        {
            Magic2Button.onClick.AddListener(() => GetComponentInParent<PlayerController>().SetPendingAction("Magic2"));
        }
        if (UltimateButton != null)
        {
            UltimateButton.onClick.AddListener(() => GetComponentInParent<PlayerController>().SetPendingAction("Ultimate"));
        }
    }
}
