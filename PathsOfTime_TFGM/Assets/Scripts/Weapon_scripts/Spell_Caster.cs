using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Spell_Caster : MonoBehaviour
{//script en cada PREF Spell Caster

    public float damage;
    public float lifeTime;
    public float delay;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("enemy") || other.gameObject.CompareTag("boss"))
        {
            print("HITTED!");
            //cojo el script del enemigo
            Enemy_Control enemy = other.gameObject.GetComponent<Enemy_Control>();
            enemy.HITEDenemy(transform.forward * 2f, damage);
            StartCoroutine(ImpactDestroy());
        }
       
    }
    IEnumerator ImpactDestroy()
    {
        // espero y elimino
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
