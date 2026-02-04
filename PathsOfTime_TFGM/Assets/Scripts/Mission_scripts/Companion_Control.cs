using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Companion_Control : MonoBehaviour
{// script en el COMPANION pref
 //pillo SINGLEs del PC y MM
    public Player_Control _PC;
    public Mission_Manager _MM;

    #region /// PLAYER MOVEMENT ///
    NavMeshAgent _agent;
    Transform _target;
    Rigidbody _rb;
    public float minDistance;
    public float maxDistance;
    public float patrolRadius;
    public float patrolCooldown;
    float patrolTimer;
    #endregion

    public float companionHealth;
    SpriteRenderer _spriteRenderer;
    Color _originalColor;

    void Start()
    {
        //pillo SINGLEs del PC y MM
        _PC = Player_Control.instance;
        _MM = Mission_Manager.instance;
        // pillo rigidbody, objetivo y colores
        _rb = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
        _target = _PC.transform;
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
        patrolTimer = patrolCooldown;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, _target.position);

        patrolTimer += Time.deltaTime;

        if (distanceToPlayer > maxDistance)
        {
            MoveToPatrolPoint();
        }
        else if (distanceToPlayer > minDistance && patrolTimer >= patrolCooldown)
        {
            MoveToPatrolPoint();
        }
        if (companionHealth <= 0)
        {
            _MM.CompanionLose();
            Destroy(gameObject);
        }
    }
    void MoveToPatrolPoint()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        Vector3 patrolPoint = _target.position + new Vector3(randomOffset.x, 0, randomOffset.y);

        _agent.SetDestination(patrolPoint);
        patrolTimer = 0f;
    }
}
