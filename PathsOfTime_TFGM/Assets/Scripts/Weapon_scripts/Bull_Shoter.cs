using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Bull_Shoter : MonoBehaviour
{//script en cada prefab de Bullet SHOT

    public float damage;
    public float lifeTime;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("enemy") || other.gameObject.CompareTag("boss"))
        {
            print("HITTED!");
            //cojo los componentes del enemigo
            NavMeshAgent agent = other.gameObject.GetComponent<NavMeshAgent>();
            Enemy_Control enemy = other.gameObject.GetComponent<Enemy_Control>();
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();

            // reset fisico
            agent.enabled = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // pegar
            enemy.TakeDamage(damage);
            rb.AddForce(transform.forward * 2.5f, ForceMode.Impulse);

            //reactivar IA
            StartCoroutine(ReactivateAgent(agent, 0.2f));
        }
        // tener referencia visual y tiempo para activar el agent
        StartCoroutine(DestroyAfterDelay(0.3f));
    }
    IEnumerator ReactivateAgent(NavMeshAgent agent, float delay)
    {
        yield return new WaitForSeconds(delay);
        agent.enabled = true;
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        // desactivo el collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        // detengo el rb
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;
        // espero y elimino
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
