using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Spell_Caster : MonoBehaviour
{//script en cada PREF Spell Caster

    float _lifeTime = 1f;
    float _delay = 1f;

    void Start()
    {
        Destroy(gameObject, _lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("enemy") || other.gameObject.CompareTag("boss"))
        {
            print("HITTED!");
            //cojo el script del enemigo
            Enemy_Control enemy = other.gameObject.GetComponent<Enemy_Control>();
            enemy.HITEDenemy(transform.forward * 5f, 2f); // DAÑO
            StartCoroutine(ImpactDestroy());
        }
       
    }
    IEnumerator ImpactDestroy()
    {
        // espero y elimino
        yield return new WaitForSeconds(_delay);
        Destroy(gameObject);
    }
}
