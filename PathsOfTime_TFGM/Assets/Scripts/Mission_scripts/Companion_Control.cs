using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Companion_Control : MonoBehaviour
{// script en el COMPANION pref
 //pillo SINGLEs del PC y MM
    public Player_Control _PC;
    public Mission_Manager _MM;


    NavMeshAgent _agent;
    Transform _target;
    Rigidbody _rb;
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
    }

    void Update()
    {
        _agent.SetDestination(_target.position);

        if (companionHealth <= 0)
        {
            _MM.CompanionLose();
            Destroy(this.gameObject);
        }
    }
}
