using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using static Mission_Manager;

public class Weapon_Control : MonoBehaviour
{// script en DONT DESTROY EMPTY
 // SINGLETON script
    public static Weapon_Control instance;
 // SINGLETON script
    public Player_Control _PC; //pillo SINGLE del PC
    public Menus_Control _MC; //pillo SINGLE del MC

    public WeaponType weapon;
    public enum WeaponType
    {
        None,
        Sword,
        Punch,
        Shot,
        Spell
    }

    #region /// ATTACK ZONES ///
    public Transform attackOrigin;
    public GameObject swordPref;
    public GameObject punchPref;
    public GameObject shotPref;
    public GameObject magicPref;
    #endregion

    #region /// GIZDRAW MARKERS ///
    WeaponType gizToDraw = WeaponType.None;
    Vector3 gizCenter;
    Quaternion gizRot;
    Vector3 gizExtents;
    #endregion

    #region /// COOLDOWN CONTROL ///
    Dictionary<WeaponType, float> cooldowns = new Dictionary<WeaponType, float>();
    public float lastAttackTimer;
    #endregion

    void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        { Destroy(gameObject); return; }
        if (instance == null) // si no hay singleton, esta instancia persiste
        { instance = this; DontDestroyOnLoad(this.gameObject); }
        else if (instance != this) // si la instancia no es esta
        {
            // y estamos en lobby, destruimos la persistente y priorizamos nuevo inicio
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                Destroy(instance.gameObject); // destruye instancia antigua
                instance = this; // asigna nueva instancia persistente
                DontDestroyOnLoad(this.gameObject);
            }
            else // y no estamos en el lobby, persistimos?
            { Destroy(gameObject); }
        }
        // me subscribo al evento de cambio de escena para resetear el arma al volver al lobby
        SceneManager.sceneLoaded += OnSceneLoaded;
        // declaro los valores de los cooldowns en el diccionario
        cooldowns.Add(WeaponType.Sword, 0.75f);
        cooldowns.Add(WeaponType.Punch, 0.5f);
        cooldowns.Add(WeaponType.Shot, 0.5f);
        cooldowns.Add(WeaponType.Spell, 0.75f);
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) //reseteo arma al cambiar a escena lobby
    {
        // ASEGURO SINGLES AL CAMBIAR ESCENA
        if (_PC == null) { _PC = Player_Control.instance; }
        if (_MC == null) { _MC = Menus_Control.instance; }
        if (attackOrigin == null && _PC != null) { attackOrigin = _PC.transform; }
        if (scene.buildIndex == 1)
        {
            weapon = WeaponType.None;
            _MC.EquipWeapon(WeaponType.None);
        }
        if (scene.buildIndex == 0)
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    { SceneManager.sceneLoaded -= OnSceneLoaded; } // desubscribimos de evento

    public void NewWeapon(WeaponType newWeapon) //llamo en el POW_GIVER
    {
        weapon = newWeapon;
        _MC.EquipWeapon(newWeapon); //cambio el UI del Menu_Control
    }

    void Update()
    {
        // clic IZD ataca
        if (Input.GetMouseButton(0))
        { AttackFunction(); }
    }
    void AttackFunction()
    {
        if (weapon == WeaponType.None) return;
        // ataco cuando haya pasado el cooldown correspondiente
        float actualCooldown = cooldowns[weapon];
        if (Time.time < lastAttackTimer + actualCooldown) return;
        lastAttackTimer = Time.time;
        // activo el ataque correspondiente al weapon equipado
        switch (weapon)
        {
        case WeaponType.None: return;
        case WeaponType.Sword: DoSWOSH(); break;
        case WeaponType.Punch:DoPUNCH();break;
        case WeaponType.Shot: DoSHOT(); break;
        case WeaponType.Spell:DoSPELL();break;
        }
    }

    void DoSWOSH()
    {
        print("TOUCHE!");
        float radius = 5f;
        // disparo donde miro con la cam
        Transform cam = Camera.main.transform;
        Vector3 dir = cam.forward.normalized;
        Vector3 attackCenter = attackOrigin.position + dir * radius;
        // copio los datos para darselos al gizdraw
        gizToDraw = WeaponType.Sword;
        gizCenter = attackCenter;

        // instancio prefab para visualizar la zona
        GameObject swoshZone = Instantiate(swordPref, attackCenter, Quaternion.LookRotation(dir));
        swoshZone.transform.Rotate(90f, 0f, 0f);
        Vector3 baseScale = swoshZone.transform.localScale;
        swoshZone.transform.localScale = baseScale * (radius-2f);
        Destroy(swoshZone, 0.5f);
        // genero el collider con todos los datos e impacto
        Collider[] hits = Physics.OverlapSphere(attackCenter, radius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("enemy") || hit.CompareTag("boss"))
            {
                print("HITTED!");
                //cojo el script del enemigo
                Enemy_Control enemy = hit.gameObject.GetComponent<Enemy_Control>();
                enemy.HITEDenemy(transform.forward * 2.5f, 2f); // DAÑO
            }
        }
    }
    void DoPUNCH()
    {
        print("PUNCHED!");
        // disparo donde miro con la cam
        Transform cam = Camera.main.transform;
        Vector3 dir = cam.forward.normalized;

        // limites del rectangulo en anchura, altura y largura
        Vector3 halfExtents = new Vector3(0.75f, 0.75f, 5f);
        // centro el box del ataque a mitad distancia
        float attackLength = halfExtents.z;
        Vector3 attackCenter = attackOrigin.position + dir * attackLength;
        // rotacion del box que apunte a dir
        Quaternion attackRot = Quaternion.LookRotation(dir, Vector3.up);
        // guardo los datos para darselos al gizdraw
        gizToDraw = WeaponType.Punch;
        gizCenter = attackCenter;
        gizRot = attackRot;
        gizExtents = halfExtents;

        // instancio prefab para visualizar la zona
        GameObject punchZone = Instantiate(punchPref, attackCenter, attackRot);
        punchZone.transform.Rotate(90f, 0f, 0f);
        Vector3 baseScale = punchZone.transform.localScale;
        punchZone.transform.localScale = Vector3.Scale(baseScale, halfExtents * 2f);
        Destroy(punchZone, 0.5f);
        // genero el collider con todos los datos e impacto
        Collider[] hits = Physics.OverlapBox(attackCenter, halfExtents, attackRot);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("enemy") || hit.CompareTag("boss"))
            {
                print("HITTED!");
                //cojo el script del enemigo
                Enemy_Control enemy = hit.gameObject.GetComponent<Enemy_Control>();
                enemy.HITEDenemy(transform.forward * 7.5f, 2f); // DAÑO
            }
        }
    }

    void DoSHOT()
    {
        print("SHOTED!");
        // disparo donde miro con la cam
        Transform cam = Camera.main.transform;
        Vector3 dir = cam.forward.normalized;

        // instancio la bala ignorando al player
        GameObject bullShot = Instantiate(shotPref, attackOrigin.position + dir * 1f, Quaternion.LookRotation(dir, Vector3.up) * shotPref.transform.rotation);
        Collider playerCollider = _PC.GetComponent<Collider>();
        Collider bulletCollider = bullShot.GetComponent<Collider>();
        Physics.IgnoreCollision(bulletCollider, playerCollider);
        // le doy fuerza a la bala pa lanzarla
        Rigidbody rb = bullShot.GetComponent<Rigidbody>();
        rb.linearVelocity = dir * 50f;
    }
    void DoSPELL()
    {
        print("WIKED!");
        // disparo donde miro con la cam
        Transform cam = Camera.main.transform;
        Vector3 dir = cam.forward.normalized;

        // instancio el spell ignorando al player
        GameObject spellCast = Instantiate(magicPref, attackOrigin.position + dir * 2f, Quaternion.LookRotation(dir, Vector3.up) * magicPref.transform.rotation);
        // le doy fuerza a la bala pa lanzarla
        Rigidbody rb = spellCast.GetComponent<Rigidbody>();
        rb.linearVelocity = dir * 25f;
    }

    IEnumerator ResumeAgent(NavMeshAgent agent, float delay)
    { // pa reactivar la IA tras el hit
        yield return new WaitForSeconds(delay);
        if (agent != null) agent.isStopped = false;
    }
    private void OnDrawGizmos() // pa ver referencia mele en visor
    {
        if (attackOrigin == null) return;
        switch (gizToDraw)
        {
            case WeaponType.None: return;

            case WeaponType.Sword:
                Gizmos.color = Color.green;
                Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                Gizmos.DrawWireSphere(gizCenter, 5f);
                break;

            case WeaponType.Punch:
                Gizmos.color = Color.red;
                Gizmos.matrix = Matrix4x4.TRS(gizCenter, gizRot, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, gizExtents * 2f);
                break;
        }
    }
}
