using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float damage;
    public float bulletSpeed;
    public float maxDistance = 20f;

    private Vector3 startPosition;
    private Transform playerTransform;

    Rigidbody2D rigid;
    CircleCollider2D coll;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<CircleCollider2D>();
    }

    void Start()
    {
        startPosition = transform.position;
        playerTransform = GameManager.instance.player.transform;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, playerTransform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    public void Init(float damage, Vector2 direction)
    {
        this.damage = damage;
        rigid.velocity = direction * bulletSpeed;
        Debug.Log($"EnemyBullet Init - Damage: {this.damage}, Direction: {direction}, Velocity: {rigid.velocity}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Hit Player with damage: {damage}");
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"Applied damage: {damage}");
            }
            Destroy(gameObject);
        }
    }
}