using System.Collections;
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.AI;

public class Weapon_Control : MonoBehaviour
{// script en el SPRITE dentro del player
 // SINGLETON script
    public static Weapon_Control instance;
 // SINGLETON script
    public Player1P_Control _PC; //pillo SINGLE del PC
    public Player2D_lobby _2DP; //pillo SINGLE del P2D

    public WeaponType weapon;
    public enum WeaponType
    {
        None,
        Sword,
        Punch,
        Shot,
        Magic
    }

    // las variables estan en los propios codigos
    public Transform attackOrigin;

    #region /// PROYECTIL ZONES ///
    public GameObject swordPref;
    public GameObject punchPref;
    public GameObject shotPref;
    public GameObject magicPref;
    #endregion

    #region /// UI MARKERS ///
    public GameObject uiPower;
    public GameObject swordPow;
    public GameObject punchPow;
    public GameObject shotPow;
    public GameObject magicPow;
    #endregion

    #region /// GIZDRAW MARKERS ///
    WeaponType gizToDraw = WeaponType.None;
    Vector3 gizCenter;
    Quaternion gizRot;
    Vector3 gizExtents;
    #endregion

    #region /// COOLDOWN CONTROL ///
    public float attackCooldown;
    public float lastAttackTimer;
    #endregion

    void Awake()// awake para instanciar singleton sin superponer varios
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    void Start()
    {
        // pillo SINGLE del PC y P2D
        _PC = Player1P_Control.instance;
        _2DP = Player2D_lobby.instance;
        // desde donde se van a generar los ataques
        if (attackOrigin == null)
        { attackOrigin = this.transform;}
        Invoke("EquipWeapon", 0.1f); // equipo arma inicial
    }

    void Update()
    {
        // clic IZD ataca
        if (Input.GetMouseButton(0))
        {
            AttackFunction();
        }
    }

    public void NewWeapon(WeaponType newWeapon) //lo llama el trigger del POW_GIVER
    {
        // igualo mi weapon al del Pow_Giver
        weapon = newWeapon;
        EquipWeapon();
    }
     public void EquipWeapon()
    {
        // elimino el marcador del anterior weapon
        foreach (Transform child in uiPower.transform)
        { Destroy(child.gameObject); }
        // declaro el weapon que voy a instanciar en la UI
        GameObject iconToInstantiate = null;
        switch (weapon)
        {
           case WeaponType.None: return;
           case WeaponType.Sword:iconToInstantiate = swordPow; break;
           case WeaponType.Punch: iconToInstantiate = punchPow; break;
           case WeaponType.Shot: iconToInstantiate = shotPow; break;
           case WeaponType.Magic: iconToInstantiate = magicPow; break;
        }
        Instantiate(iconToInstantiate, uiPower.transform);
    }

    void AttackFunction()
    {
        // compruebo el tiempo del cooldown
        if (Time.time < lastAttackTimer + attackCooldown) return;
        lastAttackTimer = Time.time;
        // activo el ataque correspondiente al weapon equipado
        switch (weapon)
        {
        case WeaponType.None: return;
        case WeaponType.Sword: DoSWOSH(); break;
        case WeaponType.Punch:DoPUNCH();break;
        case WeaponType.Shot: DoSHOT(); break;
        case WeaponType.Magic:DoMAGIC();break;
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
        swoshZone.transform.Rotate(90f, 0f, -45f);
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
                //cojo los componentes del enemigo
                NavMeshAgent agent = hit.GetComponent<NavMeshAgent>();
                Enemy_Control enemy = hit.GetComponent<Enemy_Control>();
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                // reset físico para que les afecte el impulso
                agent.enabled = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                // HITEO
                enemy.TakeDamage(2);
                rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
                rb.AddExplosionForce(2f, attackCenter, 2f, 2f, ForceMode.Impulse);
                //reactivar IA
                StartCoroutine(ReactivateAgent(agent, 0.5f));
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
        Vector3 halfExtents = new Vector3(1f, 1f, 5f);
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
        punchZone.transform.Rotate(90f, 0f, -45f);
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
                //cojo los componentes del enemigo
                NavMeshAgent agent = hit.GetComponent<NavMeshAgent>();
                Enemy_Control enemy = hit.GetComponent<Enemy_Control>();
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                // reset físico para que les afecte el impulso
                agent.enabled = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                // HITEO
                enemy.TakeDamage(2);
                Vector3 kickDir = dir + Vector3.up * 0.25f;
                kickDir.Normalize();
                rb.AddForce(kickDir * 7f, ForceMode.Impulse);
                //reactivar IA
                StartCoroutine(ReactivateAgent(agent, 0.5f));
            }
        }
    }

    void DoSHOT()
    {
        print("SHOTED!");
        // disparo donde miro con la cam
        Transform cam = Camera.main.transform;
        Vector3 dir = cam.forward.normalized;

        // instancio la bull shot ignorando al player
        GameObject bullShot = Instantiate(shotPref, attackOrigin.position + dir * 1f, Quaternion.LookRotation(dir, Vector3.up) * shotPref.transform.rotation);
        Collider playerCollider = _PC !=null ?_PC.GetComponent<Collider>(): _2DP.GetComponent<Collider>();
        Collider bulletCollider = bullShot.GetComponent<Collider>();
        Physics.IgnoreCollision(bulletCollider, playerCollider);
        //configuro el ataque en PREFAB
        Bull_Shoter bullet = bullShot.GetComponent<Bull_Shoter>();
        bullet.damage = 2f;
        bullet.lifeTime = 1.5f;
        // le doy fuerza a la bala pa lanzarla
        Rigidbody rb = bullShot.GetComponent<Rigidbody>();
        rb.linearVelocity = dir * 50f;
    }

    void DoMAGIC()
    {
        print("WIKED!");
        // disparo donde miro con la cam
        Transform cam = Camera.main.transform;
        Vector3 dir = cam.forward.normalized;

        // instancio el spell casted ignorando al player
        GameObject spellCast = Instantiate(magicPref, attackOrigin.position + dir * 2f, Quaternion.LookRotation(dir, Vector3.up) * magicPref.transform.rotation);
        //configuro el ataque en PREFAB
        Spell_Caster spell = spellCast.GetComponent<Spell_Caster>();
        spell.damage = 2f;
        spell.lifeTime = 1f;
        // le doy fuerza a la bala pa lanzarla
        Rigidbody rb = spellCast.GetComponent<Rigidbody>();
        rb.linearVelocity = dir * 15f;
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

    IEnumerator ReactivateAgent(NavMeshAgent agent, float delay)
    { // pa reactivar la IA tras el hit
        yield return new WaitForSeconds(delay);
        agent.enabled = true;
    }
}
