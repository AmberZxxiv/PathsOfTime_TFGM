using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player2D_lobby : MonoBehaviour
{// script en empty padre del LOBBY PLAYER
 // SINGLETON script
    public static Player2D_lobby instance;
 // SINGLETON script
    public Weapon_Control _WC; //pillo SINGLE del MC
    public Menus_Control _MC; //pillo SINGLE del WC

    #region /// PLAYER MOVEMENT ///
    Rigidbody _rb;
    public float movSpeed;
    float _movLateral;
    float _movFrontal;
    #endregion

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
    }

    private void FixedUpdate()
    {
            // aqui damos los valores del movimiento
            Vector3 playerMovement = (transform.right * _movLateral + transform.forward * _movFrontal);
            Vector3 playerSpeed = new Vector3(playerMovement.x * movSpeed, _rb.linearVelocity.y, playerMovement.z * movSpeed);
            _rb.linearVelocity = playerSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("portal")) //cargamos escena de dungeon
        {
            SceneManager.LoadScene(2);
        }

        Power_Giver power = other.GetComponent<Power_Giver>();
        if (power != null) //si es un PowUp, lo equipo en WEAPON
        {
            _WC.EquipWeapon(power.newWeapon);
        }
    }
}
