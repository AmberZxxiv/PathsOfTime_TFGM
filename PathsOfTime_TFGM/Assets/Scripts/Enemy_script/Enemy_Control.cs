using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.ScrollRect;

public class Enemy_Control : MonoBehaviour
{// script en cada enemigo
 //pillo SINGLEs del PC, MC y MM
   public Player_Control _PC;
   public Menus_Control _MC;
   public Mission_Manager _MM;

    public EnemyType enemyType;
    public enum EnemyType
    {
        Gnobot,
        Dronlibri,
        Torrem,
        Hydra,
        Angel,
    }
    #region /// MOVIMIENTO ///
    NavMeshAgent _agent;
    public Transform target;
    public float agroDistance;
    public float wanderRadius;
    #endregion

    #region /// CONTROL DE VUELO ///
    Rigidbody _rb;
    Vector3 _startPoint;
    Vector3 _patrolPoint;
    bool _hasPoint = false;
    public float flySpeed;
    public float flyHeight;
    public float maxRange;
    float _targetDistance;
    #endregion

    #region /// ATTACK STATS ///
    public GameObject splitPref;
    Transform _attackOrigin;
    public float attackRange;
    public float attackDamage;
    public float attackForce;
    bool _canAttack = true;
    #endregion

    #region /// LASER STATS ///
    public LineRenderer laserBeam;
    public LayerMask laserMask;
    public GameObject laserOrigin;
    public float laserTicks;
    bool _laserActive = false;
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
    public float healChance;
    public GameObject lootCoin;
    public float coinChance;
    #endregion


    void Start()
    {
        //pillo SINGLEs del PC, MC y MM
        _PC = Player_Control.instance;
        _MC = Menus_Control.instance;
        _MM = Mission_Manager.instance;
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
        _attackOrigin = this.transform;
        _startPoint = transform.position;
        target = _PC.transform;
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
    }
    void Update()
    {
        // si es Torrem, uso su sistema
        if (enemyType == EnemyType.Torrem) { TrackerTurrem(); return; }
        
        //compruebo distancia con player
        _targetDistance = Vector3.Distance(transform.position, target.position);
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
        Vector3 playerRange = (transform.position - target.position).normalized;
        Vector3 targetPos = target.position + playerRange * maxRange;
        targetPos.y = flyHeight;
        Vector3 finalPos = Vector3.MoveTowards
        (_rb.position, targetPos, flySpeed * Time.deltaTime);
        _rb.MovePosition(finalPos);
    }
    void FlyWander()
    {
        //compruebo si ha llegado al punto o no tiene
        if (!_hasPoint || Vector3.Distance
            (new Vector3(transform.position.x, 0, transform.position.z),
             new Vector3(_patrolPoint.x, 0, _patrolPoint.z)) < 1f)
        {
            _patrolPoint = RandomFlyPoint(transform.position, wanderRadius);
            _hasPoint = true;
        }

        Vector3 newPos = _patrolPoint;
        newPos.y += Mathf.Sin(Time.time) * 0.5f;
       Vector3 finalPos = Vector3.MoveTowards
        (_rb.position, newPos, flySpeed * Time.deltaTime);
        _rb.MovePosition(finalPos);

    }
    Vector3 RandomFlyPoint(Vector3 origin, float radius)
    {
        Vector2 randCircle = Random.insideUnitCircle * radius;
        Vector3 point = new Vector3(origin.x + randCircle.x, flyHeight, origin.z + randCircle.y);
        return point;
    }
    private void FixedUpdate()
    {
        // si esta a rango de ataque, ataco
        if (_targetDistance <= attackRange && _canAttack) { AttackFunction(); }
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
             DoMELE(); break;

            case EnemyType.Dronlibri:
            case EnemyType.Angel:
            DoRANGE(); break;
        }
    }
    void DoMELE()
    {
        // dirección desde enemigo a player
        Vector3 dir = target.position - _attackOrigin.position;
        dir.y = 0; dir.Normalize();
        // limites en anchura, altura, largura y centro
        Vector3 halfExtents = new Vector3(1f, 1f, attackRange);
        Vector3 center = _attackOrigin.position + dir * attackRange;
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
        print("SPLITED!");
        // disparo hacia el pecho del player
        Vector3 targetPos = target.position + Vector3.up * 1.5f;
        Vector3 dir = (targetPos - _attackOrigin.position).normalized;
        // instancio el proyectil
        GameObject splitShot = Instantiate(splitPref, _attackOrigin.position + dir * 1f, Quaternion.LookRotation(dir) * splitPref.transform.rotation);
        // evito que se choque con el mismo
        Collider enemyCollider = GetComponent<Collider>();
        Collider splitCollider = splitShot.GetComponent<Collider>();
        Physics.IgnoreCollision(splitCollider, enemyCollider);
        //doy fuerza al proyectil
        Rigidbody rb = splitShot.GetComponent<Rigidbody>();
        rb.linearVelocity = dir * 50f;
    }

    void TrackerTurrem()
    {
        Vector3 dir = target.position - laserOrigin.transform.position;
        float distance = dir.magnitude;
        
        if (distance > attackRange) 
        { laserTicks = 0f; DisableLaser(); return; }

        RaycastHit hit;
        if (Physics.Raycast(laserOrigin.transform.position, dir.normalized, out hit, attackRange, laserMask, QueryTriggerInteraction.Ignore))
        {
            EnableLaser(hit.point);
            if (hit.collider.CompareTag("Player"))
            {
                laserTicks += Time.deltaTime;
                if (laserTicks >= attackCooldown)
                {
                    _PC.playerHealth -= attackDamage;
                    _MC.UpdateLives(_PC.playerHealth);
                    laserTicks = 0f;
                }
            }
            else { laserTicks = 0f; DisableLaser(); }
        }
        else { laserTicks = 0f; DisableLaser(); }

        Debug.DrawRay(laserOrigin.transform.position, dir.normalized * attackRange, Color.magenta);
        Debug.Log( "Laser hit: " + hit.collider.name +
                   " | Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
    }

    void EnableLaser(Vector3 hitPoint)
    {
        laserBeam.enabled = true; _laserActive = true;
        laserBeam.SetPosition(0, laserOrigin.transform.position);
        laserBeam.SetPosition(1, hitPoint);
    }

    void DisableLaser()
    { laserBeam.enabled = false; _laserActive = false; } 

    public void HITEDenemy(Vector3 force, float damage) //desde WEAPON
    {
        // le bajo la vida
        StartCoroutine(FlashDamage());
        enemyHealth -= damage;
        // si muere
        if (enemyHealth <= 0)
        {
            if (CompareTag("boss")) //es boss, fin de dungeon
            { 
                if (_MM.mission == Mission_Manager.MissionSelect.CompaMis)
                { _MM.BossComplete(); }
                _MC.ShowVictory(); 
            }
            // compruebo el 25% del heal drop
            if (Random.value <= healChance)
            { Instantiate(healCherry, transform.position + Vector3.up * 1, transform.rotation);}
            // compruebo el 10% del coin drop
            if (Random.value <= coinChance)
            { Instantiate(lootCoin, transform.position + Vector3.up * 1, transform.rotation);}
            Destroy(gameObject); //se destruye
        }
        switch (enemyType) // si tiene NavMesh lo pauso
        {
            case EnemyType.Gnobot:
            case EnemyType.Hydra:
            StartCoroutine(DisableAgentTemporarily(0.2f));
            break;
        }
        if (_rb != null) // si tiene RB le empujo
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.AddForce(force, ForceMode.Impulse);
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

