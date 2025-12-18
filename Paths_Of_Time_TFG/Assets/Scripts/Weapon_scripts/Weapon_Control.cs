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
    public Player_Control _PC; //pillo SINGLE del PC

    public WeaponType weapon;
    public enum WeaponType
    {
        None,
        Kick,
        Punch,
        Shot,
        Magic
    }

    // las variables estan en los propios codigos
    public Animator playAnimator;
    public Transform attackOrigin;

    public AnimationCurve test;

    #region /// PROYECTIL ZONES ///
    public GameObject kickPref;
    public GameObject punchPref;
    public GameObject shotPref;
    public GameObject magicPref;
    #endregion

    #region /// UI MARKERS ///
    public GameObject uiPower;
    public GameObject kickPow;
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
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // pillo el singleton del Player
        _PC = Player_Control.instance;
        // pillo el animator controler propio
        playAnimator = GetComponent<Animator>();
        // desde donde se van a generar los ataques
        if (attackOrigin == null)
        { attackOrigin = this.transform;}
    }

    void Update()
    {
        // clic IZD ataca
        if (Input.GetMouseButton(0))
        {
            AttackFunction();
        }
    }

     public void EquipWeapon(WeaponType newWeapon) //lo llama el trigger del PLAYER
    { 
        // igualo mi weapon al del Pow_Giver
        weapon = newWeapon;
        // elimino el marcador del anterior weapon
        foreach (Transform child in uiPower.transform)
        { Destroy(child.gameObject); }
        // declaro el weapon que voy a instanciar en la UI
        GameObject iconToInstantiate = null;
        switch (newWeapon)
        {
           case WeaponType.None: return;
           case WeaponType.Kick:iconToInstantiate = kickPow; break;
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
        case WeaponType.Kick: DoKICK(); break;
        case WeaponType.Punch:DoPUNCH();break;
        case WeaponType.Shot: DoSHOT(); break;
        case WeaponType.Magic:DoMAGIC();break;
        }
    }

    void DoKICK()
    {
        print("KICKED!");
        // ray desde cam al plano del origen del ataque
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, attackOrigin.position.y, 0));
        if (!plane.Raycast(ray, out float enter)) return;
        // dirección desde origen hacia posición del ratón
        Vector3 mouseWorld = ray.GetPoint(enter);
        Vector3 dir = mouseWorld - attackOrigin.position;
        dir.y = 0; dir.Normalize();

        // limites del rectangulo en anchura, altura y largura
        Vector3 halfExtents = new Vector3(1f, 1f, 5f);
        // centro a mitad de la longitud hacia delante
        Vector3 attackCenter = attackOrigin.position + dir * 5f;
        attackCenter.y = 0;
        // rotacion del box que apunte a dir
        Quaternion attackRot = Quaternion.LookRotation(dir, Vector3.up);
        // guardo los datos para darselos al gizdraw
        gizToDraw = WeaponType.Kick;
        gizCenter = attackCenter;
        gizRot = attackRot;
        gizExtents = halfExtents;

        // triggereo la animacion
        playAnimator.SetTrigger("isKicking");
        // instancio prefab para visualizar la zona
        GameObject kickZone = Instantiate(kickPref, attackCenter, attackRot);
        kickZone.transform.localScale = halfExtents * 2;
        Destroy(kickZone, 0.5f);
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

    void DoPUNCH()
    {
        print("PUNCHED!");
        // solo necesito el centro debajo del player
        Vector3 attackCenter = attackOrigin.position;
        attackCenter.y = 0;
        // copio los datos para darselos al gizdraw
        gizToDraw = WeaponType.Punch;
        gizCenter = attackCenter;

        // triggereo la animacion
        playAnimator.SetTrigger("isPunching");
        // instancio prefab para visualizar la zona
        GameObject punchZone = Instantiate(punchPref, attackCenter, Quaternion.identity);
        Destroy(punchZone, 0.5f);
        // genero el collider con todos los datos e impacto
        Collider[] hits = Physics.OverlapSphere(attackCenter, 7f);
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

    void DoSHOT()
    {
        print("SHOTED!");
        // ray desde cam al plano del origen del ataque
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, attackOrigin.position.y, 0));
        if (!plane.Raycast(ray, out float enter)) return;
        // dirección desde origen hacia posición del ratón
        Vector3 mouseWorld = ray.GetPoint(enter);
        Vector3 dir = mouseWorld - attackOrigin.position;
        dir.y = 0; dir.Normalize();

        // triggereo la animacion
        playAnimator.SetTrigger("isShoting");
        // instancio la bull shot ignorando al player
        GameObject bullShot = Instantiate(shotPref, attackOrigin.position + dir * 1f, Quaternion.LookRotation(dir, Vector3.up) * shotPref.transform.rotation);
        Collider playerCollider = _PC.GetComponent<Collider>();
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
        // ray desde cam al plano del origen del ataque
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, attackOrigin.position.y, 0));
        if (!plane.Raycast(ray, out float enter)) return;
        // dirección desde origen hacia posición del ratón
        Vector3 mouseWorld = ray.GetPoint(enter);
        Vector3 dir = mouseWorld - attackOrigin.position;
        dir.y = 0; dir.Normalize();

        // triggereo la animacion
        playAnimator.SetTrigger("isCasting");
        // instancio el spell del caster
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

            case WeaponType.Kick:
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(gizCenter, gizRot, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, gizExtents * 2f);
            break;

            case WeaponType.Punch:
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireSphere(gizCenter, 7f);
            break;
        }
    }

    IEnumerator ReactivateAgent(NavMeshAgent agent, float delay)
    { // pa reactivar la IA tras el hit
        yield return new WaitForSeconds(delay);
        agent.enabled = true;
    }
}
