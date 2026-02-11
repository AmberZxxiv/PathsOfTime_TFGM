using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Split_Ranged : MonoBehaviour
{//script en cada PREF SPLIT RANGED de enemys

    public Player_Control _PC; //pillo SINGLE del PC
    public Companion_Control _CC; //pillo SINGLE del CC
    public Menus_Control _MC; //pillo SINGLE del MC
    public float damage;
    public float lifeTime;
    public float delay;

    void Start()
    {
        _PC = Player_Control.instance;
        _CC = Companion_Control.instance;
        _MC = Menus_Control.instance;
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision other)
    {
       if (other.collider.CompareTag("Player"))
            {
                _PC.playerHealth -= damage;
                _MC.UpdateLives(_PC.playerHealth);
                Vector3 hitDir = (_PC.transform.position - transform.position).normalized;
                _PC.StartCoroutine(_PC.StunnKnockback(hitDir, 2f));
            }
            else if (other.collider.CompareTag("companion"))
            {
              _CC.companionHealth -= damage;
              _MC.UpdateCompaniers(_CC.companionHealth);
              Vector3 hitDir = (_CC.transform.position - transform.position).normalized;
              _CC.HITcompa(transform.forward * 5f, damage);
            }
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
