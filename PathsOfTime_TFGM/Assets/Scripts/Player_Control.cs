using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_Control : MonoBehaviour
{// script en el empty padre del PLAYER
 // SINGLETON script
    public static Player_Control instance;
 // SINGLETON script
    public Weapon_Control _WC; //pillo SINGLE del MC
    public Menus_Control _MC; //pillo SINGLE del WC

    #region /// PLAYER MOVEMENT ///
    Rigidbody _rb;
    public float movSpeed;
    public float dashForce;
    public float dashCooldown;
    bool _canDash = true;
    bool _isDashing = false;
    bool _isStunned = false;
    float _movLateral;
    float _movFrontal;
    #endregion

    public float health;


    void Awake()
    {// awake para instanciar singleton sin superponer varios
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _WC = Weapon_Control.instance; //pillo SINGLE del WC
        _MC = Menus_Control.instance; //pillo SINGLE del MC
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // aqui cogemos los controles del movimiento
        _movLateral = Input.GetAxis("Horizontal");
        _movFrontal = Input.GetAxis("Vertical");
        // rotamos el player dependiendo de la direccion
        if (_movLateral != 0 )
        { transform.localScale = new Vector3(_movLateral > 0 ? -1 : 1, 1, 1); }
        // control del DASH
        if (Input.GetKeyDown(KeyCode.LeftShift) && _canDash)
        { DoDASH(); }
    }

    private void FixedUpdate()
    {
        if (_isStunned) return; // si me limpian el movimiento no hago nada
        if (!_isDashing) // cuando dasheo no controlo el movimiento
        {
            // aqui damos los valores del movimiento
            Vector3 playerMovement = (transform.right * _movLateral + transform.forward * _movFrontal);
            Vector3 playerSpeed = new Vector3(playerMovement.x * movSpeed, _rb.linearVelocity.y, playerMovement.z * movSpeed);
            _rb.linearVelocity = playerSpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("portal")) //cargamos escena de dungeon
        {
            SceneManager.LoadScene(2);
        }

        Power_Giver power = other.GetComponent<Power_Giver>();
        if (power != null) //si es un PowUp, lo cojo y quito el que tenía
        {
            _WC.EquipWeapon(power.newWeapon); //equipo en la weapon
            Destroy(other.gameObject);
        }

        if (other.CompareTag("heal") && health != 10) // pillo heal si no estoy a tope
        {
            health += 1;
            _MC.UpdateLives();
            Destroy(other.gameObject);
        }
    }

    private void DoDASH()
    {
        // impulso en la direccion del movimiento
        Vector3 dashDirection = (transform.right * _movLateral + transform.forward * _movFrontal).normalized;
        if (dashDirection == Vector3.zero)
        { // sin direccion, dash hacia adelante
            dashDirection = transform.forward;
        }
        _rb.AddForce(dashDirection * dashForce, ForceMode.VelocityChange);

        // activamos cooldown
        _isDashing = true;
        _canDash = false;
        Invoke("ResetDASH", dashCooldown);
    }
    void ResetDASH()
    {
        _canDash = true;
        _isDashing = false;
    }
    public IEnumerator StunnKnockback(Vector3 direction, float force) //del enemy al hitearme
    {
        _isStunned = true; //bloqueo el fixed movement
        _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0); // limpio fisics del rb
        _rb.AddForce(direction * force, ForceMode.Impulse);
        yield return new WaitForSeconds(0.1f);
        _isStunned = false;
    }
}
