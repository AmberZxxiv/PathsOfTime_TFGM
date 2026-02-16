using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Explosiv_Ranged : MonoBehaviour
{//script en cada PREF EXPLOSIVE RANGED del Angel

    public Player_Control _PC; //pillo SINGLE del PC
    public Companion_Control _CC; //pillo SINGLE del CC
    public Menus_Control _MC; //pillo SINGLE del MC
    public int damageExplosiv;
    public float radiusExplosiv;
    public float lifetimeExplosive;
    public float delayExplosive;
    bool _exploded = false;

    void Start()
    {
        _PC = Player_Control.instance;
        _CC = Companion_Control.instance;
        _MC = Menus_Control.instance;
        Destroy(gameObject, lifetimeExplosive);
    }

    private void OnCollisionEnter(Collision other)
    {
        // solo exploto una vez
        if (_exploded) return;
        _exploded = true;
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
        // genero la explosion y luego lo elimino
        Explode();
        yield return new WaitForSeconds(delayExplosive);
        Destroy(gameObject);
    }

    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radiusExplosiv);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                _PC.playerHealth -= damageExplosiv;
                _MC.UpdateLives(_PC.playerHealth);
                Vector3 hitDir = (_PC.transform.position - transform.position).normalized;
                _PC.StartCoroutine(_PC.StunnKnockback(hitDir, 2f));
            }
            else if (hit.CompareTag("companion"))
            {
                _CC.companionHealth -= damageExplosiv;
                _MC.UpdateCompaniers(_CC.companionHealth);
                Vector3 hitDir = (_CC.transform.position - transform.position).normalized;
                _CC.HITcompa(hitDir * 5f, damageExplosiv);
            }
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radiusExplosiv);
    }
}

