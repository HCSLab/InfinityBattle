using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ActionEffectManager:MonoBehaviour
{
    public bool isFar = true;
    public Vector3 pos;
	public List<ActionEffect> UltimateEffect = new List<ActionEffect>();
	public List<ActionEffect> MagicEffect = new List<ActionEffect>();
	public List<ActionEffect> AttackEffect = new List<ActionEffect>();
	public List<ActionEffect> Magic2Effect = new List<ActionEffect>();
	public List<ActionEffect> getEffectByName(string str)
	{
		switch(str)
		{
		case AnimationName.Ultimate:
			return UltimateEffect;
		case AnimationName.Magic:
			return MagicEffect;
		case AnimationName.Attack:
			return AttackEffect;
		case AnimationName.Magic2:
			return Magic2Effect;
		}
		return null;
	}

	public void stopUltimateEffect()
	{
		for(int i = 0; i < UltimateEffect.Count; i++)
		{
			UltimateEffect[i].stop();
		}
	}

    void ActionDone(string actionName)
    {

    }

    void ActionStart(string actionName)
    {
        Debug.Log("hello1 " + actionName);
        List<ActionEffect> list = getEffectByName(actionName);
        if(list == null)
        {
            return;
        }

        for(int i = 0; i < list.Count; i++)
        {
            list[i].play();
        }
    }

   
}
