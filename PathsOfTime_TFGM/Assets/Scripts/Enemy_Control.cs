using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.ScrollRect;

public class Enemy_Control : MonoBehaviour
{// script en cada enemigo
 //pillo SINGLEs del PC y MC
   public Player_Control _PC;
   public Menus_Control _MC;
   
    public EnemyType enemyType;
    public enum EnemyType
    {
        Gnobot,
        Dronlibri,
        Hydra,
        Angel,
    }
    #region /// MOVIMIENTO Y TRACKING ///
    NavMeshAgent _agent;
    Rigidbody _rb;
    public Transform target;
    public float agroDistance;
    public float wanderRadius;
    public float flySpeed;
    public float flyHeight;
    float _targetDistance;
    #endregion

    #region /// ATTACK STATS ///
    public Transform attackOrigin;
    public float attackRange;
    public float attackDamage;
    public float attackForce;
    bool _canAttack = true;
    #endregion

    #region /// COOLDOWN CONTROL ///
    public float wanderCooldown; 
    public float wanderTimer; 
    public float attackCooldown;
    public float attackTimer;
    #endregion

    #region /// HEALTH STATUS ///
    public float enemyHealth;
    SpriteRenderer _spriteRenderer;
    Color _originalColor;
    public GameObject healCherry;
    public float dropChance;
    #endregion


    void Start()
    {
        //pillo SINGLEs del PC y MC
        _PC = Player_Control.instance;
        _MC = Menus_Control.instance;
        // pillo NavMesh si tiene Humanoid agent
        switch (enemyType)
        {
            case EnemyType.Gnobot:
            case EnemyType.Hydra:
            _agent = GetComponent<NavMeshAgent>();
            break;
        }
        // pillo rigidbody, origen, objetivo y colores
        _rb = GetComponent<Rigidbody>();
        attackOrigin = this.transform;
        target = _PC.transform;
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
    }

    void Update()
    {
        //compruebo distancia con player
        _targetDistance = Vector3.Distance(this.transform.position, target.position);
        // cuando pilla agro, va hacia el player
        if (_targetDistance <= agroDistance)
        {
            switch (enemyType)
            {
                case EnemyType.Gnobot:
                case EnemyType.Hydra:
                    _agent.SetDestination(target.position);
                    break;

                case EnemyType.Dronlibri:
                case EnemyType.Angel:
                    FlyToTarget();
                    break;
            }

        }
        // si no, patrulla
        else
        {
            switch (enemyType)
            {
                case EnemyType.Gnobot:
                case EnemyType.Hydra:
                    GroundWander();
                    break;

                case EnemyType.Dronlibri:
                case EnemyType.Angel:
                    FlyWander();
                    break;
            }
        }
    }
    void GroundWander()
    {
        //empiezo el conteo para cambiar de posicion
        wanderTimer -=Time.deltaTime;
        if (wanderTimer <= 0f)
        { //cuando ha pasado el tiempo, le doy una posición nueva y reinicio el contador
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            _agent.SetDestination(newPos);
            wanderTimer = wanderCooldown;
        }
    }
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    { 
        // genero Vector3 en el radio del enemigo y le devuelve un objetivo
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        bool found = NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return !found ? origin : navHit.position;
    }
    void FlyToTarget()
    {
        Vector3 targetPos = target.position;
        targetPos.y += flyHeight;
        transform.position = Vector3.MoveTowards
        (transform.position, targetPos, flySpeed * Time.deltaTime);
        transform.LookAt(targetPos);
    }
    void FlyWander()
    {
        Vector3 floatPos = transform.position;
        floatPos.y = Mathf.Sin(Time.time) * 0.5f + flyHeight;
        transform.position = Vector3.Lerp
        (transform.position, floatPos, Time.deltaTime);
    }

    private void FixedUpdate()
    {
        // si esta a rango de ataque, ataco
        if (_targetDistance <= attackRange && _canAttack)
        { AttackFunction(); }
    }
    void AttackFunction()
    {
        // compruebo el tiempo del cooldown
        if (Time.time < attackTimer + attackCooldown) return;
        attackTimer = Time.time;
        // activo el ataque correspondiente al weapon equipado
        switch (enemyType)
        {
            case EnemyType.Gnobot:
            case EnemyType.Hydra:
                DoMELE();
                break;

            case EnemyType.Dronlibri:
            case EnemyType.Angel:
                DoRANGE();
                break;
        }
    }

    void DoMELE()
    {
        // dirección desde enemigo a player
        Vector3 dir = target.position - attackOrigin.position;
        dir.y = 0; dir.Normalize();
        // limites en anchura, altura, largura y centro
        Vector3 halfExtents = new Vector3(1f, 1f, attackRange);
        Vector3 center = attackOrigin.position + dir * attackRange;
        // genero el collider y HITEO
        Collider[] hits = Physics.OverlapBox(center,halfExtents,Quaternion.LookRotation(dir));
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            { // le hago cosas al PLAYER y al LiveContainer
                _PC.playerHealth -= attackDamage;
                _MC.UpdateLives(_PC.playerHealth);
                Vector3 hitDir = (_PC.transform.position - transform.position).normalized;
                _PC.StartCoroutine(_PC.StunnKnockback(hitDir, attackForce));
            }
        }
    }

    void DoRANGE()
    {
        // dirección desde enemigo a player
        Vector3 dir = target.position - attackOrigin.position;
        dir.y = 0; dir.Normalize();
        // limites en anchura, altura, largura y centro
        Vector3 halfExtents = new Vector3(1f, 1f, attackRange);
        Vector3 center = attackOrigin.position + dir * attackRange;
        // genero el collider y HITEO
        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.LookRotation(dir));
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            { // le hago cosas al PLAYER y al LiveContainer
                _PC.playerHealth -= attackDamage;
                _MC.UpdateLives(_PC.playerHealth);
                Vector3 hitDir = (_PC.transform.position - transform.position).normalized;
                _PC.StartCoroutine(_PC.StunnKnockback(hitDir, attackForce));
            }
        }
    }

    public void HITEDenemy(Vector3 force, float damage)
    {
        StartCoroutine(FlashDamage());
        enemyHealth -= damage;
        if (enemyHealth <= 0)
        {
            if (CompareTag("boss"))
            { _MC.ShowVictory(); }
            if (Random.value <= dropChance)
            {
                Instantiate(healCherry, transform.position + Vector3.up * 1, transform.rotation);
            }
            Destroy(gameObject);
        }
    
         _rb.linearVelocity = Vector3.zero;
         _rb.angularVelocity = Vector3.zero;
         _rb.AddForce(force, ForceMode.Impulse);

        switch (enemyType)
        {
            case EnemyType.Gnobot:
            case EnemyType.Hydra:
                StartCoroutine(DisableAgentTemporarily(0.2f));
                break;
        }
    }
    IEnumerator FlashDamage()
    {
        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(1f);
        _spriteRenderer.color = _originalColor;
    }
    IEnumerator DisableAgentTemporarily(float delay)
    {
        if (_agent != null && _agent.isOnNavMesh)
            _agent.isStopped = true;

        yield return new WaitForSeconds(delay);

        if (_agent != null && _agent.isOnNavMesh)
            _agent.isStopped = false;
    }
}

