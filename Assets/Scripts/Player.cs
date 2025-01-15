using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);
    }
    void OnEnable()
    {
        speed = 3f * Character.Speed;
        Debug.Log($"Player OnEnable - Initial speed: {speed}");
        anim.runtimeAnimatorController = animCon[GameManager.instance.playerId];    
    }
    void Update()
    {
        if (!GameManager.instance.isLive) return;

        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");
    }
    void FixedUpdate()
    {
        if (!GameManager.instance.isLive) return;

        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }
/*    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }*/

    void LateUpdate()
    {
        if (!GameManager.instance.isLive) return;

        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }    
    }
    /*    void OnCollisionStay2D(Collision2D collision)
        {
            if (!GameManager.instance.isLive) return;

            GameManager.instance.health -= Time.deltaTime * 10;

            if (GameManager.instance.health < 0)
            {
                for (int index = 0; index < transform.childCount; index++)
                {
                    transform.GetChild(index).gameObject.SetActive(false);
                }

                anim.SetTrigger("Dead");
                GameManager.instance.GameOver();
            }
        }*/

    public void TakeDamage(float damage)
    {
        if (!GameManager.instance.isLive)
            return;

        Debug.Log($"Player TakeDamage called with damage: {damage}");
        Debug.Log($"Before damage - Health: {GameManager.instance.health}");

        if (damage > 0)
        {
            GameManager.instance.health -= damage;
            Debug.Log($"After damage - Health: {GameManager.instance.health}");

            if (GameManager.instance.health <= 0)
            {
                Debug.Log("Player died!");
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }

                anim.SetTrigger("Dead");
                GameManager.instance.GameOver();
            }
        }
        else
        {
            Debug.LogWarning("Damage value is 0 or negative!");
        }
    }

}
