using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_Control : MonoBehaviour
{// script en PREF padre PLAYER
 // SINGLETON script
    public static Player_Control instance;
 // SINGLETON script
    public Weapon_Control _WC; //pillo SINGLE del WC
    public Menus_Control _MC; //pillo SINGLE del MC
    public Lobby_Manager _LB; //pillo SINGLE del LB
    public Mission_Manager _MM; //pillo SINGLE del MM

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
    public float jumpForce;
    bool _isGrounded = true;
    public float jumpSpeed;
    public float fallSpeed;
    #endregion

    #region /// CAMERA LOCATION ///
    public float mouseSensitivity;
    private float mouseRotation = 0f;
    public Transform cameraTransform;
    #endregion

    #region /// STATS NUMBERS ///
    public float playerHealth;
    public int coinsLooted;
    #endregion

    void Awake()// singleton sin superponer y no destruir al cambiar escena
    {
        if (instance == null) { instance = this; }
        else Destroy(gameObject);
    }
    void Start()
    {
        // pillo SINGLES de WC, MC, LM, MM
        _WC = Weapon_Control.instance;
        _MC = Menus_Control.instance; 
        _LB = Lobby_Manager.instance;
        _MM = Mission_Manager.instance;
        _rb = GetComponent<Rigidbody>();
        // centramos el cursos en pantalla y lo ocultamos
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        // cogemos el valor del cursor para poder darlo de vuelta
        float horizontalRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, horizontalRotation, 0);
        mouseRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        mouseRotation = Mathf.Clamp(mouseRotation, -90f, 90f);
        // lo copiamos en la camara y lo bloqueamos en los polos
        cameraTransform.localRotation = Quaternion.Euler(mouseRotation, 0, 0);

        // aqui cogemos los controles del movimiento
        _movLateral = Input.GetAxis("Horizontal");
        _movFrontal = Input.GetAxis("Vertical");
        // hacemos que el personaje se mueva a donde mira
        if (_movLateral != 0)
        { transform.localScale = new Vector3(_movLateral > 0 ? -1 : 1, 1, 1); }
        // control del DASH
        if (Input.GetKeyDown(KeyCode.LeftShift) && _canDash)
        { DoDASH(); }
        // si pulsamos espacio y estamos tocando suelo, aplicamos el salto
        if (Input.GetButtonDown("Jump") && _isGrounded)
        { DoJUMP(); }
    }
    private void FixedUpdate()
    {
        if (_isStunned) return; // si me limpian el movimiento no hago nada
        if (!_isDashing) // cuando dasheo y salto NO controlo el movimiento
        {
            // aqui damos los valores del movimiento
            Vector3 playerMovement = (transform.right * _movLateral + transform.forward * _movFrontal);
            Vector3 playerSpeed = new Vector3(playerMovement.x * movSpeed, _rb.linearVelocity.y, playerMovement.z * movSpeed);
            _rb.linearVelocity = playerSpeed;
        }
        // aumentamos la velocidad del salto al subir
        if (_rb.linearVelocity.y > 0f)
        {
            _rb.linearVelocity += Vector3.up * Physics.gravity.y * jumpSpeed * Time.fixedDeltaTime;
        }
        // aumentamos la gravedad al caer del salto
        if (_rb.linearVelocity.y < 0f)
        {
            _rb.linearVelocity += Vector3.up * Physics.gravity.y * fallSpeed * Time.fixedDeltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPCpas")) //conversar con NPC pasado
        { _LB.PastInteracton(); }
        if (other.CompareTag("NPCfut")) //conversar con NPC futuro
        { _LB.FutrInteracton(); }
       
        if (other.CompareTag("PORpas") //si hemos escogido mision y arma, guardamos pasado y cargamos dungeon
            && _MM.mission != Mission_Manager.MissionSelect.None
            && _WC.weapon != Weapon_Control.WeaponType.None) 
        {
            PlayerPrefs.SetInt("Dungeon", 0);
            PlayerPrefs.Save();
            SceneManager.LoadScene(2);
        }
        if (other.CompareTag("PORfut") //si hemos escogido mision y arma, guardamos futuro y cargamos dungeon
            && _MM.mission != Mission_Manager.MissionSelect.None
            && _WC.weapon != Weapon_Control.WeaponType.None)
        {
            PlayerPrefs.SetInt("Dungeon", 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene(2);
        }

        if (other.CompareTag("PORexit")) //salimos al menu de completao
        { _MC.ShowExit(); }

        Power_Giver power = other.GetComponent<Power_Giver>();
        if (power != null) //si es PowUp, equipo en WEAPON
        { _WC.NewWeapon(power.newWeapon); }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPCpas")) //cerrar NPC pasado
        { _LB.PastExit(); }
        if (other.CompareTag("NPCfut")) //cerrar NPC futuro
        { _LB.FutrExit();}
    }
    private void OnCollisionEnter(Collision collision)
    {
        // compruebo haber colisionado con el suelo
        if (collision.gameObject.CompareTag("ground") || (collision.gameObject.CompareTag("plataform")))
        {
            _isGrounded = true;
        }
        // si choco con algo mortal, me quito las vidas
        if (collision.gameObject.CompareTag("deadly"))
        {
            playerHealth -= 10;
            _MC.UpdateLives(playerHealth);
            _MC.ShowDead();
        }
        if (collision.gameObject.CompareTag("heal") && playerHealth != 10) // pillo heal si no estoy a tope
        {
            playerHealth += 1;
            _MC.UpdateLives(playerHealth);
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("looteable"))
        {
            coinsLooted += 1;
            _MC.CoinsCounter(coinsLooted);
            Destroy(collision.gameObject);
        }
    }
    
    void DoJUMP()
    {
        //actualizamos el estado del salto, la altura y damos la fuerza
        _isGrounded = false;
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
