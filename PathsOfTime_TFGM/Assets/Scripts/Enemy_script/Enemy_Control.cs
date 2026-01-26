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
    #region /// MOVIMIENTO BASE ///
    NavMeshAgent _agent;
    public Transform target;
    public float agroDistance;
    float _targetDistance;
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
    #endregion

    #region /// COOLDOWN CONTROL ///
    public float wanderCooldown; 
    public float wanderTimer; 
    public float attackCooldown;
    public float attackTimer;
    #endregion

    #region /// HEALTH STATS ///
    public float enemyHealth;
    SpriteRenderer _spriteRenderer;
    Color _originalColor;
    public GameObject healCherry;
    public float healChance;
    public GameObject coinLoot;
    public float coinChance;
    #endregion


    void Start()
    {
        //pillo SINGLEs del PC, MC y MM
        _PC = Player_Control.instance;
        _MC = Menus_Control.instance;
        _MM = Mission_Manager.instance;
        // pillo NavMesh si usan Agent Humanoid
        switch (enemyType)
        {
            case EnemyType.Gnobot:
            case EnemyType.Hydra:
            _agent = GetComponent<NavMeshAgent>(); break;
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
        
        //compruebo distancia de los móviles con el player
        _targetDistance = Vector3.Distance(transform.position, target.position);
        // cuando pillan agro, van hacia el player
        if (_targetDistance <= agroDistance)
        {
            switch (enemyType)
            {
                case EnemyType.Gnobot:
                case EnemyType.Hydra:
                _agent.SetDestination(target.position); break;

                case EnemyType.Dronlibri:
                case EnemyType.Angel:
                FlyToTarget(); break;
            }

        }
        // si no, de base, patrullan
        else switch (enemyType)
            {
                case EnemyType.Gnobot:
                case EnemyType.Hydra:
                GroundWander(); break;

                case EnemyType.Dronlibri:
                case EnemyType.Angel:
                FlyWander(); break;
            }
    }
    void GroundWander() // patrulla de enemigos terrestres
    {
        wanderTimer -= Time.deltaTime; //empiezo timer para cambiar de posicion
        if (wanderTimer <= 0f) //cuando ha pasado el tiempo, le doy objetivo nuevo y reinicio timer
        { 
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            _agent.SetDestination(newPos);
            wanderTimer = wanderCooldown;
        }
    }
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    { // genero Vector3 en radio del enemigo y lo devuelve como objetivo
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        bool found = NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return !found ? origin : navHit.position;
    }
    void FlyToTarget() // trackeo de voladores al player
    {
        Vector3 playerRange = (transform.position - target.position).normalized;
        Vector3 targetPos = target.position + playerRange * maxRange;
        targetPos.y = flyHeight;
        Vector3 finalPos = Vector3.MoveTowards
        (_rb.position, targetPos, flySpeed * Time.deltaTime);
        _rb.MovePosition(finalPos);
    }
    void FlyWander() // patrulla de voladores
    {
        //compruebo si ha llegado al punto o no tiene
        if (!_hasPoint || Vector3.Distance
            (new Vector3(transform.position.x, 0, transform.position.z),
             new Vector3(_patrolPoint.x, 0, _patrolPoint.z)) < 1f)
        {
            _patrolPoint = RandomFlyPoint(transform.position, wanderRadius);
            _hasPoint = true;
        }
        // movimiento hacia el punto objetivo
        Vector3 newPos = _patrolPoint;
        newPos.y += Mathf.Sin(Time.time) * 0.5f;
       Vector3 finalPos = Vector3.MoveTowards
        (_rb.position, newPos, flySpeed * Time.deltaTime);
        _rb.MovePosition(finalPos);
    }
    Vector3 RandomFlyPoint(Vector3 origin, float radius)
    { // genero Vector3 en radio del enemigo y lo devuelve como objetivo
        Vector2 randCircle = Random.insideUnitCircle * radius;
        Vector3 point = new Vector3(origin.x + randCircle.x, flyHeight, origin.z + randCircle.y);
        return point;
    }

    private void FixedUpdate() // ataco en fixed para contar en segundos y no en frames
    {
        // si esta a rango de ataque y puedo, ataco
        if (_targetDistance <= attackRange && _canAttack) { AttackFunction(); }
    }
    void AttackFunction() // funcion de ataque común
    {
        // compruebo el tiempo del cooldown
        if (Time.time < attackTimer + attackCooldown) return;
        attackTimer = Time.time;
        // activo el ataque correspondiente al enemigo
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
        // genero el collider y busco golpeados
        Collider[] hits = Physics.OverlapBox(center,halfExtents,Quaternion.LookRotation(dir));
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            { // aplico el daño al PC y al MC
                _PC.playerHealth -= attackDamage;
                _MC.UpdateLives(_PC.playerHealth);
                Vector3 hitDir = (_PC.transform.position - transform.position).normalized;
                _PC.StartCoroutine(_PC.StunnKnockback(hitDir, attackForce));
            }
        }
    }
    void DoRANGE()
    {
        // disparo hacia el pecho del player
        Vector3 targetPos = target.position + Vector3.up * 1.5f;
        Vector3 dir = (targetPos - _attackOrigin.position).normalized;
        // instancio el proyectil ( controla su propia colision )
        GameObject splitShot = Instantiate(splitPref, _attackOrigin.position + dir * 1f, Quaternion.LookRotation(dir) * splitPref.transform.rotation);
        // evito que se choque con él mismo
        Collider enemyCollider = GetComponent<Collider>();
        Collider splitCollider = splitShot.GetComponent<Collider>();
        Physics.IgnoreCollision(splitCollider, enemyCollider);
        //doy fuerza al proyectil
        Rigidbody rb = splitShot.GetComponent<Rigidbody>();
        rb.linearVelocity = dir * 50f;
    }

    void TrackerTurrem()
    {
        // compruebo la distancia con el player
        Vector3 dir = target.position - laserOrigin.transform.position;
        float distance = dir.magnitude;
        // si esta fuera de rango, apago el laser
        if (distance > attackRange) 
        { laserTicks = 0f; DisableLaser(); return; }
        // si esta dentro de rango y me impacta, activo el laser
        RaycastHit hit;
        if (Physics.Raycast(laserOrigin.transform.position, dir.normalized, out hit, attackRange, laserMask, QueryTriggerInteraction.Ignore))
        {
            EnableLaser(hit.point);
            if (hit.collider.CompareTag("Player"))
            { // al golpear al player, activo el cooldown y aplico el daño al PC y al MC
                laserTicks += Time.deltaTime;
                if (laserTicks >= attackCooldown)
                {
                    _PC.playerHealth -= attackDamage;
                    _MC.UpdateLives(_PC.playerHealth);
                    laserTicks = 0f;
                }
            }
            else { laserTicks = 0f; DisableLaser(); } // si golpea a otro objeto, lo apago
        }
        else { laserTicks = 0f; DisableLaser(); } // si me salgo de rango, lo apago
        // dibujo el laser en el visor y leo donde impacta
        Debug.DrawRay(laserOrigin.transform.position, dir.normalized * attackRange, Color.magenta);
        Debug.Log( "Laser hit: " + hit.collider.name +
                   " | Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
    }
    void EnableLaser(Vector3 hitPoint) // activo el laser desde su origen al player
    {
        laserBeam.enabled = true;
        laserBeam.SetPosition(0, laserOrigin.transform.position);
        laserBeam.SetPosition(1, hitPoint);
    }
    void DisableLaser() { laserBeam.enabled = false; } // apago el laser

    public void HITEDenemy(Vector3 force, float damage) //desde WEAPON cuando golpeo a enemigos
    {
        // le impacto visualmente y le bajo la vida
        StartCoroutine(FlashDamage());
        enemyHealth -= damage;
        // si muere
        if (enemyHealth <= 0)
        {
            // en mision de matar a boss 
            if (CompareTag("boss") && _MM.mission == Mission_Manager.MissionSelect.BossMis)
            { _MM.BossComplete(); }
            // compruebo el 25% del heal drop
            if (Random.value <= healChance)
            { Instantiate(healCherry, transform.position + Vector3.up * 1, transform.rotation);}
            // compruebo el 10% del coin drop
            if (Random.value <= coinChance)
            { Instantiate(coinLoot, transform.position + Vector3.up * 1, transform.rotation);}
            Destroy(gameObject); //se destruye
        }
        if (_agent != null) // si tiene NavMesh lo pauso
        { StartCoroutine(DisableAgentTemporarily(0.2f)); }
        if (_rb != null) // si tiene RB le empujo
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.AddForce(force, ForceMode.Impulse);
        }
    }
    IEnumerator FlashDamage() // cambio su color a rojo al recibir daño
    {
        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(1f);
        _spriteRenderer.color = _originalColor;
    }
    IEnumerator DisableAgentTemporarily(float delay) // pauso los NavMesh para afectar a _rb
    {
        if (_agent != null && _agent.isOnNavMesh)
            _agent.isStopped = true;
        yield return new WaitForSeconds(delay);
        if (_agent != null && _agent.isOnNavMesh)
            _agent.isStopped = false;
    }
}

