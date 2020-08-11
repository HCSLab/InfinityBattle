using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class huanyueyingDemo : MonoBehaviour {

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

    IEnumerator delayBullet(float amount)
    {
        int count = 20;
        for (int i = 0; i < count; i++)
        {
            AttackedController c = player.GetComponent<AttackedController>();
            GameObject obj = GameObject.Instantiate(ultimateBullet);
            CurvelBullet bullet = obj.GetComponent<CurvelBullet>();
            bullet.player = transform;
            bullet.target = c.transform;
           
            bullet.bulleting(amount);
            yield return null;
            if(i % 9 == 0)
            {
                bullet.effectObj = damageEffect1;
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
                break;
            case AnimationName.Ultimate:
                if (ultimateBullet != null)
                {
                    StartCoroutine(delayBullet(gameObject.GetComponent<HeroAttributes>().getAttackAmount("Ultimate")));
                }
                if (damageEffect3 != null)
                {
                    GameObject obj1 = GameObject.Instantiate(damageEffect3);
                    ParticlesEffect effect = obj1.AddComponent<ParticlesEffect>();
                    Transform target = player.transform;
                    effect.transform.position = MathUtil.findChild(target, "attackedPivot").position;
                    effect.play();
                }
                c.attacked(transform.parent.gameObject, gameObject.GetComponent<HeroAttributes>().getAttackAmount("Ultimate"));
                break;
        }
    }
}
