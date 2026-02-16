using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using static UnityEngine.GraphicsBuffer;

public class Companion_Control : MonoBehaviour
{// script en el COMPANION pref
    // SINGLETON script
    public static Companion_Control instance;
    //pillo SINGLEs del PC y MM
    public Player_Control _PC;
    public Mission_Manager _MM;

    #region /// MOVEMENT ///
    NavMeshAgent _agent;
    Transform _target;
    Rigidbody _rb;
    public float minDistance;
    public float maxDistance;
    public float orbitRadius;
    public float orbitSpeed;
    float orbitAngle;
    #endregion

    public int companionHealth;
    SpriteRenderer _spriteRenderer;
    Color _originalColor;
    void Awake()// singleton sin superponer y no destruir al cambiar escena
    {
        if (instance == null) { instance = this; }
        else Destroy(gameObject);
    }
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
        orbitAngle = Random.Range(0f, 360f);
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, _target.position);
        if (distanceToPlayer > maxDistance)
        {
            // demasiado lejos sigue al player
            _agent.SetDestination(_target.position);
        }
        else if (distanceToPlayer < minDistance)
        {
            // demasiado cerca retrocede un poco para mantener espacio
            Vector3 dir = (transform.position - _target.position).normalized;
            Vector3 targetPos = _target.position + dir * orbitRadius;
            _agent.SetDestination(targetPos);
        }
        else
        {
            // dentro del rango de orbita
            OrbitAroundPlayer();
        }
    }
    void OrbitAroundPlayer()
    {
        // varia el ángulo y la velocidad en función del tiempo
        orbitAngle += orbitSpeed * Time.deltaTime;
        if (orbitAngle >= 360f) orbitAngle -= 360f;
        float rad = orbitAngle * Mathf.Deg2Rad;
        Vector3 orbitPos = _target.position + new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * orbitRadius;
        _agent.SetDestination(orbitPos);
    }

    public void HITcompa(Vector3 force, int damage) //desde ENEMYS al golpear
    {
        // le impacto visualmente y le bajo la vida
        StartCoroutine(FlashDamage());
        companionHealth -= damage;
        // si muere
        if (companionHealth <= 0)
        {
            _MM.CompanionLose();
            Destroy(gameObject);
        }
        // desactivo agente para aplicar fisicas
        StartCoroutine(DisableAgentTemporarily(0.2f));
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.AddForce(force, ForceMode.Impulse);
        
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
