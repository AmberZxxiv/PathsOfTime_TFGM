using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Companion_Control : MonoBehaviour
{// script en el COMPANION pref
 //pillo SINGLE del PC
    public Player_Control _PC;

    NavMeshAgent _agent;
    Transform _target;
    Rigidbody _rb;
    public float companionHealth;
    SpriteRenderer _spriteRenderer;
    Color _originalColor;

    void Start()
    {
        //pillo SINGLE del PC
        _PC = Player_Control.instance;
        // pillo rigidbody, objetivo y colores
        _rb = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
        _target = _PC.transform;
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
    }

    void Update()
    {
        _agent.SetDestination(_target.position);
    }
}
