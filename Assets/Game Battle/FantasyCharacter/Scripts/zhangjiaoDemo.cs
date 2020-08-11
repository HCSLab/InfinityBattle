using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zhangjiaoDemo : MonoBehaviour {

    public GameObject attackBullet;
    public GameObject magicBullet;
    public GameObject magic2Bullet;
    public GameObject ultimateBullet;
    public GameObject rbullet;
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


    IEnumerator delayBullet(float amount)
    {
        int count = 10;
        for (int i = 0; i < count; i++)
        {
            GameObject obj = GameObject.Instantiate(rbullet);
            RotateBullet bullet = obj.GetComponent<RotateBullet>();
            bullet.player = transform;
            bullet.effectObj = damageEffect1;
            bullet.bulleting(amount);
            bullet.y = 0.35f;
            yield return new WaitForSeconds(0.1f);
            if (i % 9 == 0)
            {
                AttackedController c = player.GetComponent<AttackedController>();
                c.attacked(transform.parent.gameObject, amount);
                if (damageEffect2 != null)
                {
                    GameObject obj1 = GameObject.Instantiate(damageEffect2);
                    ParticlesEffect effect = obj1.AddComponent<ParticlesEffect>();
                    Transform target = player.transform;
                    effect.transform.position = MathUtil.findChild(target, "attackedPivot").position;
                    effect.play();
                }
            }
        }
    }

    IEnumerator delayBullet1(float amount)
    {
        int count = 10;
        for (int i = 0; i < count; i++)
        {
            GameObject obj = GameObject.Instantiate(rbullet);
            RotateBullet bullet = obj.GetComponent<RotateBullet>();
            bullet.player = transform;
            bullet.effectObj = damageEffect1;
            bullet.bulleting(amount);
            bullet.y = 0.7f;
            bullet.flag = -1f;
            yield return new WaitForSeconds(0.1f);
            if (i % 9 == 0)
            {
                AttackedController c = player.GetComponent<AttackedController>();
                c.attacked(transform.parent.gameObject, amount);
                if (damageEffect2 != null)
                {
                    GameObject obj1 = GameObject.Instantiate(damageEffect2);
                    ParticlesEffect effect = obj1.AddComponent<ParticlesEffect>();
                    Transform target = player.transform;
                    effect.transform.position = MathUtil.findChild(target, "attackedPivot").position;
                    effect.play();
                }
            }
        }
    }

    void preAction(string actionName)
    {
        player = GetComponent<HeroAttributes>().Target;
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
                StartCoroutine(delayBullet(gameObject.GetComponent<HeroAttributes>().getAttackAmount("Magic")));
                StartCoroutine(delayBullet1(gameObject.GetComponent<HeroAttributes>().getAttackAmount("Magic")));
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
                StartCoroutine(delayBullet(gameObject.GetComponent<HeroAttributes>().getAttackAmount("Magic2")));
                StartCoroutine(delayBullet1(gameObject.GetComponent<HeroAttributes>().getAttackAmount("Magic2")));
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
               
                break;
        }
    }
}
