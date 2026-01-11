using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Spell_Caster : MonoBehaviour
{//script en cada prefab de Bullet MAGIC

    public float damage;
    public float lifeTime;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("enemy") || other.gameObject.CompareTag("boss"))
        {
            print("HITTED!");
            //cojo los componentes del enemigo
            NavMeshAgent agent = other.gameObject.GetComponent<NavMeshAgent>();
            Enemy_Control enemy = other.gameObject.GetComponent<Enemy_Control>();
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();

            // reset fisico
            agent.isStopped = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // pegar
            enemy.TakeDamage(damage);
            rb.AddForce(transform.forward * 2.5f, ForceMode.Impulse);

            //reactivar IA
            StartCoroutine(ResumeAgent(agent, 0.2f));
            // tener referencia visual y tiempo para activar el _agent
            StartCoroutine(DestroyAfterDelay(0.5f));
        }
       
    }
    IEnumerator ResumeAgent(NavMeshAgent agent, float delay)
    {
        yield return new WaitForSeconds(delay);
        agent.enabled = true;
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        // espero y elimino
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
