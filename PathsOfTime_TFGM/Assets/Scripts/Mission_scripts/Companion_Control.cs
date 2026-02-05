using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Companion_Control : MonoBehaviour
{// script en el COMPANION pref
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
        orbitAngle = Random.Range(0f, 360f);
    }

    void Update()
    {
        if (companionHealth <= 0)
        {
            _MM.CompanionLose();
            Destroy(gameObject);
        }
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
        // Incrementa el ángulo en función del tiempo y la velocidad
        orbitAngle += orbitSpeed * Time.deltaTime;
        if (orbitAngle >= 360f) orbitAngle -= 360f;

        // Calcula la posición en la circunferencia
        float rad = orbitAngle * Mathf.Deg2Rad;
        Vector3 orbitPos = _target.position + new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * orbitRadius;

        // Mueve al companion hacia esa posición
        _agent.SetDestination(orbitPos);
    }
}
