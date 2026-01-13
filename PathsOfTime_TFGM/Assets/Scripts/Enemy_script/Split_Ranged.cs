using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Split_Ranged : MonoBehaviour
{//script en cada prefab SPLIT RANGED de enemys

    public Player_Control _PC; //pillo SINGLE del PC
    public Menus_Control _MC; //pillo SINGLE del MC
    public float damage;
    public float lifeTime;
    public float delay;

    void Start()
    {
        _PC = Player_Control.instance;
        _MC = Menus_Control.instance;
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // le hago cosas al PLAYER y al LiveContainer
            _PC.playerHealth -= damage;
            _MC.UpdateLives(_PC.playerHealth);
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
