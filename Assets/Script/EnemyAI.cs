using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    Status stat;

    Animator anim;
    Rigidbody2D rb;
    SpriteRenderer sr;
    GameObject AttackTarget = null;
    CapsuleCollider2D cc;


    GameObject HPUI;
    Image HPbar;
    public GameObject DamageText;

    Coroutine AppearHPCoroutine;
    Coroutine ProceedingCoroutine;
    Coroutine HittedCoroutine;

    bool isActing;
    bool isHitStunned;
    bool isMumchit; 

    public string MyEnemyType;

    [SerializeField]
    public List<string> plus_stat;
    [SerializeField]
    public List<float> plus_value;

    enum State
    {
        Idle,
        Move,
        Jump,
        Change
    }

    State state;
    const int StateCount = 4;

    bool isGround;
    int Direction = 1;


    const float MinActionTime = 1f;
    const float MaxActionTime = 3f;

    GameObject EnemyAttackBox;


    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        stat = GetComponent<Status>();
        cc = GetComponent<CapsuleCollider2D>();

        EnemyAttackBox = gameObject.transform.GetChild(1).gameObject;

        HPUI = transform.GetChild(0).gameObject;
        HPbar = HPUI.transform.GetChild(1).gameObject.GetComponent<Image>();

        if (DamageText == null)
            DamageText = Resources.Load("Prefab/DamageUim") as GameObject;

        state = State.Idle;

    }


    void Update()
    {
        AttackBoxDirectionAsync();
        ChangeDirection();
        CheckingTopography();
        MovingPattern();
    }
    void ChangeDirection()
    {
        // flipX에 따른 방향 설정(1/-1)
        Direction = sr.flipX ? -1 : 1;
    }
    void CheckingTopography()
    {
        // 본인기준 주변 지형파악 위한 레이저 체크
        RaycastHit2D rayFrontGroundCheck, rayFrontWallCheck, rayUnderGroundCheck, rayFloorGroundCheck;

        //    //레이저 시각효과 활성화
        //    //앞
        //Debug.DrawRay(transform.position + new Vector3(Direction, 0, 0), new Vector3(0, 1, 0), new Color(0, 1, 0));
        //    //바닥
        //Debug.DrawRay(transform.position + new Vector3(0.35f * -Direction, 0, 0), new Vector3(0, -1, 0) * ((cc.size.y / 2) + 0.15f), new Color(0, 1, 0));

        rayFrontGroundCheck = Physics2D.Raycast(transform.position + new Vector3(Direction, 0, 0), Vector2.up, 1f, 1 << LayerMask.NameToLayer("Ground"));
        rayFrontWallCheck = Physics2D.Raycast(transform.position, new Vector3(Direction, 0, 0), 0.5f, 1 << LayerMask.NameToLayer("UnPassableWall"));
        rayUnderGroundCheck = Physics2D.Raycast(transform.position + new Vector3(0.35f * -Direction, 0, 0), new Vector3(0, -1, 0), (cc.size.y / 2) + 0.15f, 1 << LayerMask.NameToLayer("Ground"));
        rayFloorGroundCheck = Physics2D.Raycast(transform.position, new Vector3(0, -1, 0), (cc.size.y / 2) + 0.15f, 1 << LayerMask.NameToLayer("Ground"));
        // 앞에 땅이 있다면 점프로 도약
        if (isGround && rayFrontGroundCheck.collider != null || (rayFloorGroundCheck.collider == null && isGround))
            rb.velocity = new Vector2(stat.MoveSpeed * Direction, stat.JumpPower);

        // 앞에 벽이 있다면 방향전환
        if (isGround && rayFrontWallCheck.collider != null)
            sr.flipX = (sr.flipX) ? false : true;

        // 바닥 체크후 isGround 전환
        isGround = (rayUnderGroundCheck.collider == null) ? false : true;



    }
    void MovingPattern()
    {
        // 공격 대상이 없다면
        if (AttackTarget == null && !isHitStunned)
        {
            if (!isActing)
                SetAction();
            switch (state)
            {
                // 멈춤
                case State.Idle:
                    rb.velocity = new Vector2(0, rb.velocity.y);

                    anim.SetBool("Idle", true);
                    anim.SetBool("Walk", false);

                    break;

                // 움직임
                case State.Move:

                    rb.velocity = new Vector2(stat.MoveSpeed * Direction, rb.velocity.y);

                    anim.SetBool("Idle", false);
                    anim.SetBool("Walk", true);

                    break;
                // 점프
                case State.Jump:
                    if (isGround)
                        rb.velocity = new Vector2(rb.velocity.x, stat.JumpPower);
                    SetAction();
                    break;
                // 방향 전환
                case State.Change:
                    sr.flipX = (bool)(Random.value > 0.5f); // flipX를 랜덤으로 true false 부여
                    SetAction(1, StateCount - 2);
                    break;
            }
        }
        else  // 공격 대상이 있다면
        {
            if (isMumchit || isGround)
            {
                if (!isHitStunned)
                    rb.velocity = new Vector2(0, rb.velocity.y);
            }
            else
                rb.velocity = new Vector2(stat.MoveSpeed * 1.2f * Direction, rb.velocity.y);

            if (!isHitStunned && isGround && !isMumchit) // 피격시간 끝나면
            {
                sr.flipX = (transform.position.x < AttackTarget.transform.position.x) ? false : true; // 방향 설정

                anim.SetBool("Idle", false);
                anim.SetBool("Walk", true);

                rb.velocity = new Vector2(stat.MoveSpeed * 1.2f * Direction, rb.velocity.y);
            }

            float distanceXGap = Mathf.Abs(transform.position.x - AttackTarget.transform.position.x);
            if (!isHitStunned && distanceXGap < 2f && !isMumchit) // 가까울때 적이 
            {
                float dinstanceYGap = transform.position.y - AttackTarget.transform.position.y;
                if (Mathf.Abs(dinstanceYGap) > 0.5f && dinstanceYGap < 0) // 나보다 높이 위치하면 점프
                {
                    if (isGround)
                        rb.velocity = new Vector2(rb.velocity.x, 7);
                }

                // 공격패턴
                float AttackableDistance;
                switch (MyEnemyType) {
                    case "Warrior":
                        AttackableDistance = 1f;
                        if (distanceXGap < AttackableDistance)
                        {
                            anim.SetTrigger("Attack");
                            StartCoroutine("GetMumchit", 0.5f);
                            Attack();
                        }
                        break;
                    default:
                        break;
                }
                
            }


            if (!isActing)
                AttackTarget = null;
        }
    }

    void Attack()
    {
        StartCoroutine(DamageDelay(GetRandomDamageValue(stat.AttackPower, 0.8f, 1.2f), 0.04f));
    }
    int GetRandomDamageValue(int OriginDamage, float minX, float maxX)
    {
        int Damage;
        Damage = (int)(OriginDamage * UnityEngine.Random.Range(minX, maxX));
        return Damage;
    }

    IEnumerator GetMumchit(float time)
    {
        isMumchit = true;
        yield return new WaitForSeconds(time);
        isMumchit = false;
    }

    // 상태 랜덤 부여  
    void SetAction(int f = 0, int s = StateCount, float FixedTime = 0)
    {
        if (ProceedingCoroutine != null)
            StopCoroutine(ProceedingCoroutine);
        ProceedingCoroutine = (FixedTime == 0) ? StartCoroutine("SetActingTrue", Random.Range(MinActionTime, MaxActionTime)) : StartCoroutine("SetActingTrue", FixedTime);
        if (isGround)
            switch (Random.Range(0, s))
            {
                case 0:
                    state = State.Idle;
                    break;
                case 1:
                    state = State.Move;
                    break;
                case 2:
                    state = State.Jump;
                    break;
                case 3:
                    state = State.Change;
                    break;
            }
        //Debug.Log(state);
        //Debug.Log(isActing);
    }

    IEnumerator SetActingTrue(float time)
    {
        isActing = true;
        yield return new WaitForSeconds(time);
        isActing = false;
    }




    // 체력바 활성화 후 6초뒤 비활성
    IEnumerator AppearHPUI()
    {
        HPUI.SetActive(true);
        yield return new WaitForSeconds(6f);
        HPUI.SetActive(false);
    }

    IEnumerator GetHittedStun(float time)
    {
        isHitStunned = true;
        stat.MoveSpeed = 0.01f;
        yield return new WaitForSeconds(time);
        isHitStunned = false;
        stat.MoveSpeed = stat.BasicSpeed;
    }

    public void GetDamaged(int damage, GameObject Fromwho)
    {
        AttackTarget = Fromwho;
        if (damage == 0)

        damage = AttackTarget.GetComponent<Status>().AttackPower;


        if (AppearHPCoroutine != null)
            StopCoroutine(AppearHPCoroutine);
        AppearHPCoroutine = StartCoroutine("AppearHPUI");
        stat.HP -= damage;
        HPbar.fillAmount = stat.HP / stat.MaxHp;

        // 맞은 방향 쳐다본후 뒤로 밀리기
        sr.flipX = (transform.position.x < AttackTarget.transform.position.x) ? false : true; // 방향 설정
        rb.velocity = new Vector2(1.3f * -Direction, rb.velocity.y);

        // 애니메이션
        anim.SetTrigger("Hitted");

        if (HittedCoroutine != null)
            StopCoroutine(HittedCoroutine);
        HittedCoroutine = StartCoroutine("GetHittedStun", 0.5f);

        if (ProceedingCoroutine != null)
            StopCoroutine(ProceedingCoroutine);
        ProceedingCoroutine = StartCoroutine("SetActingTrue", 10f);

        PopUpDamageText(damage);

        if (stat.HP < 0)
        {
            if (AttackTarget.tag == "Player")
            {
                Dictionary<string, float> statyouearn = new Dictionary<string, float>();
                for (int i = 0; i < plus_stat.Count; i++)
                {
                    statyouearn.Add(plus_stat[i], plus_value[i]);
                    
                }
                AttackTarget.GetComponent<Player>().PlayerStatChange(statyouearn);
            }
            StartCoroutine("Die");
        }
    }

    public void GetAirborne(Vector2 force)
    {
        rb.velocity = new Vector2(0, 0);
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    void PopUpDamageText(int damage)
    {
        GameObject DamageUI = Instantiate(DamageText, transform.localPosition, Quaternion.identity);
        DamageUI.GetComponentInChildren<DamageUI>().Spawn(damage, gameObject);
    }

    void AttackBoxDirectionAsync()
    {
        EnemyAttackBox.GetComponent<Transform>().localScale = sr.flipX ? new Vector2(1, 1) : new Vector2(-1, 1);
    }


    private void DamageAllinHitBox(int damage, GameObject effect = null, string CCtype = null)
    {
        List<GameObject> Enemys = EnemyAttackBox.GetComponent<EnemyAttackBox>().GetAttackableTargets();
        for (int i = 0; i < Enemys.Count; i++)
            if (Enemys[i] != null)
            {
                GameObject Spawnedeffect;
                if (effect != null)
                {
                    Spawnedeffect = Instantiate(effect, Enemys[i].transform.position, Quaternion.identity);
                    Spawnedeffect.transform.SetParent(Enemys[i].transform);
                }


                if (Enemys[i].tag == "Enemy")
                {
                    EnemyAI ai;
                    ai = Enemys[i].GetComponent<EnemyAI>();
                    if (ai != null)
                          ai.GetDamaged(damage, gameObject);
                }
                else if (Enemys[i].tag == "Player") {
                    Player ai;
                    ai = Enemys[i].GetComponent<Player>();
                    if (ai != null)
                          ai.GetDamage(damage);
                }
                
            }

    }


    IEnumerator DamageDelay(int damage, float time, GameObject Effect = null, string CCtype = null)
    {

        yield return new WaitForSeconds(time);
        DamageAllinHitBox(damage, Effect, CCtype);
    }

    // 죽음
    IEnumerator Die()
    {
        anim.SetTrigger("Hitted");
        stat.MoveSpeed = 0f;
        Color color = sr.color;

        color.a = sr.color.a / 2;
        sr.color = color;

        while (color.a >= 0f) 
        {
            color.a -= (Time.deltaTime / 0.5f); // 0.5초에 걸쳐 사라짐
            sr.color = color;
            yield return null;
        }
        Destroy(gameObject);
    }
}
