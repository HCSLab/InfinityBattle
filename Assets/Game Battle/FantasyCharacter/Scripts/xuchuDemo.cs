using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class xuchuDemo : MonoBehaviour {

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
                if(attackBullet != null)
                {
                    GameObject obj = GameObject.Instantiate(attackBullet);
                    NormalBullet bullet = obj.GetComponent<NormalBullet>();
                    bullet.player = transform;
                    bullet.target = player.transform;
                    bullet.effectObj = damageEffect1;
                    bullet.bulleting(gameObject.GetComponent<HeroAttributes>().getAttackAmount("Attack"));
                }
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
                if (magicBullet != null)
                {
                    GameObject obj = GameObject.Instantiate(magicBullet);
                    NormalBullet bullet = obj.GetComponent<NormalBullet>();
                    bullet.player = transform;
                    bullet.target = player.transform;
                    bullet.effectObj = damageEffect1;
                    bullet.bulleting(gameObject.GetComponent<HeroAttributes>().getAttackAmount("Magic"));
                }
                if (damageEffect3 != null)
                {
                    GameObject obj = GameObject.Instantiate(damageEffect3);
                    ParticlesEffect effect = obj.AddComponent<ParticlesEffect>();
                    Transform target = player.transform;
                    effect.transform.position = MathUtil.findChild(target, "attackedPivot").position;
                    effect.play();
                }
                c.attacked(transform.parent.gameObject, gameObject.GetComponent<HeroAttributes>().getAttackAmount("Magic"));
                break;
            case AnimationName.Magic2:
                if (magic2Bullet != null)
                {
                    GameObject obj = GameObject.Instantiate(magic2Bullet);
                    NormalBullet bullet = obj.GetComponent<NormalBullet>();
                    bullet.player = transform;
                    bullet.target = player.transform;
                    bullet.effectObj = damageEffect2;
                    bullet.bulleting(gameObject.GetComponent<HeroAttributes>().getAttackAmount("Magic2"));
                }
                if (damageEffect2 != null)
                {
                    GameObject obj = GameObject.Instantiate(damageEffect2);
                    ParticlesEffect effect = obj.AddComponent<ParticlesEffect>();

                    effect.transform.position = player.transform.position;
                    effect.play();
                }
                c.attacked(transform.parent.gameObject, gameObject.GetComponent<HeroAttributes>().getAttackAmount("Magic2"));
                break;
            case AnimationName.Ultimate:
                if (ultimateBullet != null)
                {
                    GameObject obj = GameObject.Instantiate(ultimateBullet);
                    LightBullet bullet = obj.GetComponent<LightBullet>();
                    bullet.player = transform;
                    bullet.target = player.transform;
                    bullet.effectObj = damageEffect3;
                    bullet.bulleting(gameObject.GetComponent<HeroAttributes>().getAttackAmount("Ultimate"));
                }
                if(damageEffect2 != null)
                {
                    GameObject obj = GameObject.Instantiate(damageEffect2);
                    ParticlesEffect effect = obj.AddComponent<ParticlesEffect>();

                    effect.transform.position = player.transform.position;
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
