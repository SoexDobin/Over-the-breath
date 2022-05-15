using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreeOfDesire : MonoBehaviour
{
    Animator anim;

    Status stat;
    enum Faze
    {
        Faze1,
        Faze2
    }

    Faze MyFaze = Faze.Faze1;



    GameObject HPUI;
    Image HPbar;
    public GameObject DamageText;



    Coroutine AppearHPUICoroutine;


    public GameObject Seed;

    Coroutine ActCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();


        // UI 관련
        HPUI = transform.GetChild(0).gameObject;
        HPbar = HPUI.transform.GetChild(1).gameObject.GetComponent<Image>();

        if (DamageText == null)
            DamageText = Resources.Load("Prefab/DamageUim") as GameObject;

        stat = GetComponent<Status>();







        ActCoroutine = StartCoroutine("Act");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Act()
    {
        yield return new WaitForSeconds(10f);

        int Actnum = 0;//Random.Range(0, 3);
        switch (Actnum)
        {
            case 0:
                SummonSeeds(Random.Range(8,20));
                break;
        }
        //yield return new WaitForSeconds(10f);
        ActCoroutine = StartCoroutine("Act");
    }
    void SummonSeeds(float count)
    {
        Debug.Log("발사!");
        for (int i = 0; i < count; i++)
        {
            StartCoroutine(SeedManage());
        }
    }
    IEnumerator SeedManage()
    {
        GameObject Spawned = Instantiate(Seed, transform.position, Quaternion.identity);
        Spawned.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-0.02f, 0.02f), Random.Range(0.02f, 0.04f)));
        yield return new WaitForSeconds(3f);
        Spawned.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        Spawned.GetComponent<CircleCollider2D>().isTrigger = true;
        yield return new WaitForSeconds(3f);
        Destroy(Spawned);
    }



    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "PlayerAttack")
        {
            GetDamaged(GetRandomDamageValue(col.GetComponent<Fire>().Damage, 0.8f, 1.2f), GameObject.FindGameObjectWithTag("Player"));
            Destroy(col.gameObject);
        }
    }
    // 랜덤 숫자 발급, Ontrigger의 GetDamage에 쓰임
    int GetRandomDamageValue(float OriginDamage, float minX, float maxX)
    {
        int Damage;
        Damage = (int)(OriginDamage * UnityEngine.Random.Range(minX, maxX));
        return Damage;
    }

    // 데미지 입음
    public void GetDamaged(float damage, GameObject Fromwho)
    {
        stat.HP -= damage;                          // 체력감소
        HPbar.fillAmount = stat.HP / stat.MaxHp;    // UI 업데이트

        if (AppearHPUICoroutine != null)
            StopCoroutine(AppearHPUICoroutine);
        AppearHPUICoroutine = StartCoroutine("AppearHPUI");     // HP UI 보이기

        PopUpDamageText(damage);

        FazeCheck();

        if (stat.HP <= 0)                           // 체력 다달면
        {
            Die(Fromwho);
        }
    }
    void Die(GameObject Fromwho)
    {
        //if(Fromwho != null){
        //      Fromwho.GetComponent<Status>().
        //}
        Destroy(gameObject);
    }

    // 체력바 UI 6초간 보이기
    IEnumerator AppearHPUI()
    {
        HPUI.SetActive(true);
        yield return new WaitForSeconds(6f);
        HPUI.SetActive(false);
    }

    void PopUpDamageText(float damage)
    {
        GameObject DamageUI = Instantiate(DamageText, transform.localPosition, Quaternion.identity);
        DamageUI.GetComponentInChildren<DamageUI>().Spawn((int)damage, gameObject);
    }

    void FazeCheck()
    {
        if(MyFaze == Faze.Faze1 && stat.HP < stat.MaxHp * 0.5f)
        {
            anim.SetTrigger("Faze2");
            MyFaze = Faze.Faze2;
            transform.position = new Vector2(transform.position.x, -1.24f);
        }
    }


}
