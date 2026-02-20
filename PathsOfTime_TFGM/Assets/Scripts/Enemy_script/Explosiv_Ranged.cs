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
    public ParticleSystem particles;

    void Start()
    {
        _PC = Player_Control.instance;
        _CC = Companion_Control.instance;
        _MC = Menus_Control.instance;
        ShowExplosionArea();
        Destroy(gameObject, lifetimeExplosive);
    }

    void ShowExplosionArea()
    { // le doy la configuracion del emisor
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radiusExplosiv;
        var main = particles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startSpeed = 0f;
        particles.Clear();
        particles.Play();
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
        // desactivo el collider y el RB
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;
        // aplico el daño y lanzo las particulas desde el impacto
        yield return new WaitForSeconds(delayExplosive);
        var main = particles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        Explode();
        LaunchParticles();
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
    void LaunchParticles()
    { // cojo las particulas y las lanzo
        ParticleSystem.Particle[] particleArray =
        new ParticleSystem.Particle[particles.particleCount];
        int count = particles.GetParticles(particleArray);
        for (int i = 0; i < count; i++)
        {
            Vector3 dir = (particleArray[i].position - transform.position).normalized;
            particleArray[i].velocity = dir * 10f; // fuerza de expansión
        }
        particles.SetParticles(particleArray, count);
    }
    void OnDrawGizmos()
    { // gizmos pa ver en le editor
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radiusExplosiv);
    }
}

