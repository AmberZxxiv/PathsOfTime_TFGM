using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Shoter_Bullet : MonoBehaviour
{//script en cada PREF Shot Bull

    public float damage;
    public float lifeTime;
    public float delay;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("enemy") || other.gameObject.CompareTag("boss"))
        {
            print("HITTED!");
            //cojo el script del enemigo
            Enemy_Control enemy = other.gameObject.GetComponent<Enemy_Control>();
            enemy.HITEDenemy(transform.forward * 2f, damage);
        }
        StartCoroutine(ImpactDestroy());
    }
    IEnumerator ImpactDestroy()
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
