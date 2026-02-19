using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.DebugUI.Table;
using static UnityEngine.UI.ScrollRect;
using static Weapon_Control;

public class Enemy_Control : MonoBehaviour
{// script en cada prefab enemigo
 //pillo SINGLEs del PC, MC y MM
   public Player_Control _PC;
   public Companion_Control _CC;
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
    Animator _animator;
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
    public float flyRange;
    #endregion

    #region /// BASE ATTACKS ///
    public GameObject splitPref;
    public GameObject explosivPref;
    Transform _attackOrigin;
    public float attackRange;
    public int attackDamage;
    public float attackForce;
    public Transform companionTarget;
    public float companionAgroChance;
    #endregion

    #region /// GIZDRAW MARKERS ///
    public GameObject bitePref;
    private bool showAttackGizmos = false;
    private List<(Vector3 center, Vector3 halfExtents, Quaternion rotation)> currentHitboxes = new();
    #endregion

    #region /// LASER STATS ///
    public LineRenderer laserBeam;
    public LayerMask laserMask;
    public GameObject laserOrigin;
    float _laserTicks = 0;
    #endregion

    #region /// COOLDOWN CONTROL ///
    public float wanderCooldown; 
    float _wanderTimer = 0; 
    public float attackCooldown;
    float _attackTimer = 0;
    #endregion

    #region /// STATS Y LOOT ///
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
        //pillo SINGLEs
        _PC = Player_Control.instance;
        _CC = Companion_Control.instance;
        _MC = Menus_Control.instance;
        _MM = Mission_Manager.instance;
        // pillo NavMesh si usan Agent Humanoid
        switch (enemyType)
        {
            case EnemyType.Gnobot:
            case EnemyType.Hydra:
            _agent = GetComponent<NavMeshAgent>(); break;
        }
        // pillo rigidbody, origen, objetivo, colores y activo animator
        _rb = GetComponent<Rigidbody>();
        _attackOrigin = this.transform;
        _startPoint = transform.position;
        target = _PC.transform;
        if (_CC != null) companionTarget = _CC.transform;
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
        _animator = GetComponentInChildren<Animator>();
        _animator.SetBool("isMoving", true);
    }
    void Update()
    {
        // si es Torrem, uso su sistema
        if (enemyType == EnemyType.Torrem) { TrackerTurrem(); return; }

        //compruebo si el companion es elegible y está cerca
        if (companionTarget != null)
        {
            float rand = Random.value;
            if (rand < companionAgroChance && Vector3.Distance(transform.position, companionTarget.position) <= agroDistance)
            { target = companionTarget;}
            else target = _PC.transform;
        }
        else target = _PC.transform;

        // cuando pillan agro, van hacia el player
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
        _wanderTimer -= Time.deltaTime; //empiezo timer para cambiar de posicion
        if (_wanderTimer <= 0f) //cuando pasa el tiempo, doy objetivo nuevo y reinicio timer
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            _agent.SetDestination(newPos);
            _wanderTimer = wanderCooldown;
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
    void FlyToTarget() // trackeo voladores a player con distancia de seguridad
    {
        Vector3 playerRange = (transform.position - target.position).normalized;
        Vector3 targetPos = target.position + playerRange * flyRange;
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
    { // genero Vector3 en radio del volador y lo devuelve como punto objetivo
        Vector2 randCircle = Random.insideUnitCircle * radius;
        Vector3 point = new Vector3(origin.x + randCircle.x, flyHeight, origin.z + randCircle.y);
        return point;
    }

    private void FixedUpdate() // ataco en fixed para contar en segundos y no en frames
    {
        // calculo la distancia al target correspondiente
        _targetDistance = Vector3.Distance(transform.position, target.position);
        // si esta a rango de ataque y puedo, ataco
        if (_targetDistance <= attackRange) { AttackFunction(); }
    }
    void AttackFunction() // funcion de ataque común
    {
        // ataco cuando haya pasado el tiempo de cooldown
        if (Time.time < _attackTimer + attackCooldown) return;
        _attackTimer = Time.time;
        // activo el ataque correspondiente al enemigo
        switch (enemyType)
        {
            case EnemyType.Gnobot:
            DoMELE(); break;
            case EnemyType.Hydra:
            DoMultiBites(); break;
            case EnemyType.Dronlibri:
            DoRANGE(); break;
            case EnemyType.Angel:
            DoExplosion(); break;
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
        // copio los datos para darselos al gizdraw durante el ataque
        currentHitboxes.Clear();
        currentHitboxes.Add((center, halfExtents, Quaternion.LookRotation(dir)));
        showAttackGizmos = true;
        StartCoroutine(HideGizmos(0.5f));
        // genero el collider con su animacion y busco golpeados
        Collider[] hits = Physics.OverlapBox(center,halfExtents,Quaternion.LookRotation(dir));
        _animator.SetTrigger("isAttacking");
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                _PC.playerHealth -= attackDamage;
                _MC.UpdateLives(_PC.playerHealth);
                Vector3 hitDir = (_PC.transform.position - transform.position).normalized;
                _PC.StartCoroutine(_PC.StunnKnockback(hitDir, attackForce));
            }
            else if (hit.CompareTag("companion"))
            {
                _CC.companionHealth -= attackDamage;
                _MC.UpdateCompaniers(_CC.companionHealth);
                Vector3 hitDir = (_CC.transform.position - transform.position).normalized;
                _CC.HITcompa(transform.forward * 5f, attackDamage); 
                
            }
        }
    }
    void DoMultiBites()
    {
        // dirección hacia el frente del player
        Vector3 forwardDir = (target.position - _attackOrigin.position).normalized;
        forwardDir.y = 0; forwardDir.Normalize();
        Quaternion rot = Quaternion.LookRotation(forwardDir);
        // parametros del espaciado y el barrido
        float spacing = 10f;
        float range = attackRange;
        Vector3 halfExtents = new Vector3(1f, 1f, range);
        // colocamos cada cabeza de izquierda a derecha
        Vector3 rightDir = rot * Vector3.right;
        Vector3[] offsets = new Vector3[]
        { Vector3.left * spacing, Vector3.zero, Vector3.right * spacing };
        // copio los datos de cada una para el gizdraw durante los ataques
        currentHitboxes.Clear();
        foreach (Vector3 offset in offsets)
        {
            Vector3 center = _attackOrigin.position + forwardDir * range + offset;
            currentHitboxes.Add((center, halfExtents, rot));
        }
        //activo el gizmos y la animacion
        showAttackGizmos = true;
        StartCoroutine(HideGizmos(0.5f));
        _animator.SetTrigger("isAttacking");
        foreach (Vector3 offset in offsets)
        {
            // primero instancio el sprite ajustado a las zonas
            Vector3 center = _attackOrigin.position + forwardDir * range + offset;
            GameObject biteZone = Instantiate(bitePref, center, rot);
            biteZone.transform.Rotate(90f, 0f, 0f);
            Vector3 baseScale = biteZone.transform.localScale;
            biteZone.transform.localScale = Vector3.Scale(baseScale, halfExtents * 2f);
            Destroy(biteZone, 0.5f);
            //ahora genero los impactos
            Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.LookRotation(forwardDir));
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    _PC.playerHealth -= attackDamage;
                    _MC.UpdateLives(_PC.playerHealth);
                    Vector3 hitDir = (_PC.transform.position - transform.position).normalized;
                    _PC.StartCoroutine(_PC.StunnKnockback(hitDir, attackForce));
                }
                else if (hit.CompareTag("companion"))
                {
                    _CC.companionHealth -= attackDamage;
                    _MC.UpdateCompaniers(_CC.companionHealth);
                    Vector3 hitDir = (_CC.transform.position - transform.position).normalized;
                    _CC.HITcompa(transform.forward * 5f, attackDamage);
                }
            }
        }
    }
    void DoRANGE()
    {
        // disparo siempre al centro del objetivo
        Collider playerCollider = target.GetComponent<Collider>();
        Vector3 targetPos = target.position + Vector3.up * (playerCollider.bounds.size.y / 2f);
        Vector3 dir = (targetPos - _attackOrigin.position).normalized;
        // instancio el proyectil con su animacion ( controla su propia collision )
        _animator.SetTrigger("isAttacking");
        GameObject splitShot = Instantiate
        (splitPref, _attackOrigin.position + dir * 1f, Quaternion.LookRotation(dir) * splitPref.transform.rotation);
        // evito que se choque con él mismo
        Collider enemyCollider = GetComponent<Collider>();
        Collider splitCollider = splitShot.GetComponent<Collider>();
        Physics.IgnoreCollision(splitCollider, enemyCollider);
        //doy fuerza al proyectil
        Rigidbody rb = splitShot.GetComponent<Rigidbody>();
        rb.linearVelocity = dir * 50f;
    }
    void DoExplosion()
    {
        // disparo siempre al centro del objetivo
        Collider playerCollider = target.GetComponent<Collider>();
        Vector3 targetPos = target.position + Vector3.up * (playerCollider.bounds.size.y / 2f);
        Vector3 dir = (targetPos - _attackOrigin.position).normalized;
        // instancio el proyectil con su animacion ( controla su propia collision )
        _animator.SetTrigger("isAttacking");
        GameObject explosiveShot = Instantiate
        (explosivPref, _attackOrigin.position + dir * 1f, Quaternion.LookRotation(dir) * explosivPref.transform.rotation);
        // evito que se choque con él mismo
        Collider enemyCollider = GetComponent<Collider>();
        Collider shotCollider = explosiveShot.GetComponent<Collider>();
        Physics.IgnoreCollision(shotCollider, enemyCollider);
        //doy fuerza al proyectil
        Rigidbody rb = explosiveShot.GetComponent<Rigidbody>();
        rb.linearVelocity = dir * 25f;
    }

    void TrackerTurrem()
    {
        // compruebo la distancia con el player
        Vector3 dir = target.position - laserOrigin.transform.position;
        float distance = dir.magnitude;
        // si esta fuera de rango, apago laser
        if (distance > attackRange) 
        { _laserTicks = 0f; DisableLaser(); return; }
        // si esta dentro de rango y me impacta, activo laser y los ticks de daño
        RaycastHit hit;
        if (Physics.Raycast(laserOrigin.transform.position, dir.normalized, out hit, attackRange, laserMask, QueryTriggerInteraction.Ignore))
        {
            EnableLaser(hit.point);
            bool didDamage = false;
            if (hit.collider.CompareTag("Player"))
            {
                _laserTicks += Time.deltaTime;
                if (_laserTicks >= attackCooldown)
                {
                    _PC.playerHealth -= attackDamage;
                    _MC.UpdateLives(_PC.playerHealth);
                    _laserTicks = 0f;
                }
                didDamage = true;
            }
            else if (hit.collider.CompareTag("companion"))
            {
                _laserTicks += Time.deltaTime;
                if (_laserTicks >= attackCooldown)
                {
                    _CC.companionHealth -= attackDamage;
                    _MC.UpdateCompaniers(_CC.companionHealth);
                    Vector3 hitDir = (_CC.transform.position - transform.position).normalized;
                    _CC.HITcompa(transform.forward * 0f, attackDamage);
                    _laserTicks = 0f;
                }
                didDamage = true;
            }
            if(!didDamage) { _laserTicks = 0f; DisableLaser(); } // si golpea a otro objeto, apago laser
        }
        else { _laserTicks = 0f; DisableLaser(); } // si me salgo de rango, apago laser
        // dibujo laser en el visor y leo donde impacta
        Debug.DrawRay(laserOrigin.transform.position, dir.normalized * attackRange, Color.magenta);
        Debug.Log( "Laser hit: " + hit.collider.name +
                   " | Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
    }
    void EnableLaser(Vector3 hitPoint) // activo laser desde su origen al player
    {
        laserBeam.enabled = true;
        laserBeam.SetPosition(0, laserOrigin.transform.position);
        laserBeam.SetPosition(1, hitPoint);
    }
    void DisableLaser() { laserBeam.enabled = false; } // apago laser

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
            Destroy(gameObject); return;//se destruye
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
    private void OnDrawGizmos() // dibujo los hitboxes de los ataques a mele
    {
        if (showAttackGizmos && currentHitboxes.Count > 0)
        {
            Gizmos.color = Color.magenta;
            foreach (var hitbox in currentHitboxes)
            {
                Gizmos.matrix = Matrix4x4.TRS(hitbox.center, hitbox.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, hitbox.halfExtents * 2);
            }
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
    IEnumerator HideGizmos(float seconds) // escondo los ataques tras el cooldown
    {
        yield return new WaitForSeconds(seconds);
        showAttackGizmos = false;
        currentHitboxes.Clear();
    }
}

