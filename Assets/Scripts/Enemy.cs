using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public RuntimeAnimatorController[] animCon;
    public Rigidbody2D target;

    [Header("Coin Drop")]
    public GameObject coinPrefab;

    AttackType attackType;
    float meleeDamage;
    float rangedDamage;
    float meleeCooldown;
    float rangedCooldown;
    GameObject rangedBulletPrefab;

    bool isLive;
    bool canMeleeAttack = true;
    bool canRangedAttack = true;

    Rigidbody2D rigid;
    Collider2D coll;
    Animator anim;
    SpriteRenderer spriter;
    WaitForFixedUpdate wait;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();
    }
    void FixedUpdate()
    {
        if (!GameManager.instance.isLive) return;

        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit")) return;

        if (attackType == AttackType.Melee)
        {
            Vector2 dirVec = target.position - rigid.position;
            Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);
        }

        rigid.velocity = Vector2.zero;
    }
    void LateUpdate()
    {
        if (!GameManager.instance.isLive) return;

        if (!isLive) return;

        spriter.flipX = target.position.x < rigid.position.x;
    }
    void Update()
    {
        if (!GameManager.instance.isLive || !isLive) return;
        HandleAttackLogic();
    }
    void HandleAttackLogic()
    {
        float dist = Vector2.Distance(transform.position, target.position);

        if (attackType == AttackType.Melee)
        {
            if (dist < 1.5f && canMeleeAttack)
            {
                StartCoroutine(MeleeAttack());
            }
        }
        else if (attackType == AttackType.Ranged)
        {
            if (dist < 5f)
            {
                Vector2 directionAwayFromPlayer = ((Vector2)transform.position - target.position).normalized;
                Vector2 retreatPosition = (Vector2)transform.position + directionAwayFromPlayer * speed * Time.deltaTime;
                rigid.MovePosition(retreatPosition);
            }
            else if (dist >= 5f && dist <= 10f && canRangedAttack)
            {
                StartCoroutine(RangedAttack());
            }
            else if (dist > 10f)
            {
                Vector2 dirVec = target.position - rigid.position;
                Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
                rigid.MovePosition(rigid.position + nextVec);
            }
        }
    }
    IEnumerator MeleeAttack()
    {
        canMeleeAttack = false;
        // Directly damage the player
        GameManager.instance.player.TakeDamage(meleeDamage);

        yield return new WaitForSeconds(meleeCooldown);
        canMeleeAttack = true;
    }

    IEnumerator RangedAttack()
    {
        canRangedAttack = false;

        if (rangedBulletPrefab != null)
        {
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;

            GameObject bulletObj = Instantiate(rangedBulletPrefab, transform.position, Quaternion.identity);

            Debug.Log($"Enemy rangedDamage: {rangedDamage}");

            EnemyBullet bullet = bulletObj.GetComponent<EnemyBullet>();
            if (bullet != null)
            {
                bullet.damage = rangedDamage;
                bullet.Init(rangedDamage, direction);

                Debug.Log($"Bullet damage after Init: {bullet.damage}");
            }
            else
            {
                Debug.LogError("EnemyBullet component not found on prefab!");
            }

            Physics2D.IgnoreCollision(bulletObj.GetComponent<Collider2D>(), coll);
        }
        else
        {
            Debug.LogError("RangedBulletPrefab is not assigned!");
        }

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);

        yield return new WaitForSeconds(rangedCooldown);
        canRangedAttack = true;
    }

    void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        anim.SetBool("Dead", false);
        health = maxHealth;
    }
    public void Init(SpawnData data)
    {
        anim.runtimeAnimatorController = animCon[data.spriteType];
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;

        attackType = data.attackType;
        meleeDamage = data.meleeDamage;
        rangedDamage = data.rangedDamage;
        meleeCooldown = data.meleeCooldown;
        rangedCooldown = data.rangedCooldown;
        rangedBulletPrefab = data.rangedBulletPrefab;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet") || !isLive || collision.CompareTag("EnemyBullet"))
            return;

        health -= collision.GetComponent<Bullet>().damage;
        StartCoroutine(KnockBack());

        if (health > 0)
        {
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            isLive = false;
            coll.enabled = false;
            rigid.simulated = false;
            spriter.sortingOrder = 1;
            anim.SetBool("Dead", true);
            GameManager.instance.kill++;
            GameManager.instance.GetExp();

            DropCoin();

            if (GameManager.instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
        }
    }

    IEnumerator KnockBack()
    {
        yield return wait;
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }
    void DropCoin()
    {
        if (coinPrefab != null)
        {
            GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
        }
    }

}
