using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class caohongDemo : MonoBehaviour {

    public GameObject attackBullet;
    public GameObject magicBullet;
    public GameObject magic2Bullet;
    public GameObject ultimateBullet;
    public GameObject damageEffect1;
    public GameObject damageEffect2;
    public GameObject damageEffect3;

    private GameObject player;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void preAction(string actionName)
    {
        player = GetComponent<HeroAttributes>().Target;
        AttackedController c = player.GetComponent<AttackedController>();
        string[] arr = actionName.Split('|');
        string name = arr[0];
        switch(name)
        {
            case AnimationName.Attack:
                if (damageEffect1 != null)
                {
                    GameObject obj = GameObject.Instantiate(damageEffect1);
                    ParticlesEffect effect = obj.AddComponent<ParticlesEffect>();
                    Transform target = player.transform;
                    effect.transform.position = MathUtil.findChild(target, "attackedPivot").position;
                    effect.play();
                }
                c.attacked(transform.parent.gameObject, gameObject.GetComponent<HeroAttributes>().getAttackAmount("Attack"));
                break;
            case AnimationName.Magic:
                if (damageEffect2 != null)
                {
                    GameObject obj = GameObject.Instantiate(damageEffect2);
                    ParticlesEffect effect = obj.AddComponent<ParticlesEffect>();
                    Transform target = player.transform;
                    effect.transform.position = MathUtil.findChild(target, "attackedPivot").position;
                    effect.play();
                }
                c.attacked(transform.parent.gameObject, gameObject.GetComponent<HeroAttributes>().getAttackAmount("Magic"));
                break;
            case AnimationName.Magic2:
                if (damageEffect2 != null)
                {
                    GameObject obj = GameObject.Instantiate(damageEffect2);
                    ParticlesEffect effect = obj.AddComponent<ParticlesEffect>();
                    Transform target = player.transform;
                    effect.transform.position = MathUtil.findChild(target, "attackedPivot").position;
                    effect.play();
                }
                c.attacked(transform.parent.gameObject, gameObject.GetComponent<HeroAttributes>().getAttackAmount("Magic2"));
                break;
            case AnimationName.Ultimate:

                if(damageEffect3 != null)
                {
                    GameObject obj = GameObject.Instantiate(damageEffect3);
                    ParticlesEffect effect = obj.AddComponent<ParticlesEffect>();

                    effect.transform.position = player.transform.position;
                    effect.transform.rotation = Quaternion.Euler(0f, -111f, 0f);
                    effect.play();
                }
                c.attacked(transform.parent.gameObject, gameObject.GetComponent<HeroAttributes>().getAttackAmount("Ultimate"));
                break;
        }
    }

    IEnumerator delayAttacked(float amount)
    {
        yield return new WaitForSeconds(1.5f);
        AttackedController c = player.GetComponent<AttackedController>();
        c.attacked(transform.parent.gameObject, amount);
        //yield return new WaitForSeconds(2.5f);
        //c.attacked();
    }
}
