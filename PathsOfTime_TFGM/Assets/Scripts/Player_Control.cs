using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player_Control : MonoBehaviour
{// script en PREF padre PLAYER
 // SINGLETON script
    public static Player_Control instance;
 // SINGLETON script
    public Weapon_Control _WC; //pillo SINGLE del WC
    public Menus_Control _MC; //pillo SINGLE del MC
    public Mission_Manager _MM; //pillo SINGLE del MM

    #region /// PLAYER MOVEMENT ///
    Rigidbody _rb;
    Animator _animator;
    public float movSpeed;
    public float dashForce;
    public float dashCooldown;
    bool _canDash = true;
    bool _isDashing = false;
    bool _isStunned = false;
    float _movLateral;
    float _movFrontal;
    public float jumpForce;
    public float jumpSpeed;
    public float fallSpeed;
    public float driftControl;
    #endregion

    #region /// JUMP CONTROL ///
    public Transform playerFeet;
    public float floorDistance;
    public LayerMask floors;
    bool _isGrounded;
    public float coyoteCooldown;
    float _coyoteTimer;
    #endregion

    #region /// CAMERA LOCATION ///
    public float mouseSensitivity;
    public float joystickSensitivity;
    private float mouseRotation = 0f;
    public Transform cameraTransform;
    public bool _isAiming;
    #endregion

    #region /// STATS NUMBERS ///
    public int playerHealth;
    public int coinsLooted;
    #endregion

    void Awake()// singleton sin superponer y no destruir al cambiar escena
    {
        if (instance == null) { instance = this; }
        else Destroy(gameObject);
    }
    void Start()
    {
        // pillo SINGLES y componentes
        _WC = Weapon_Control.instance;
        _MC = Menus_Control.instance;
        _MM = Mission_Manager.instance;
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        // centramos el cursos en pantalla y lo ocultamos
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        if (_isAiming) // en primera persona cogemos MouseDelta y RightStick
        {
            PlayerInput playerInput = GetComponent<PlayerInput>();
            InputAction playerLook = GetComponent<PlayerInput>().actions["Look"];
            Vector2 lookInput = playerLook.ReadValue<Vector2>();
            float horizontalRotation = 0f;
            float verticalRotation = 0f;
            // deadzone para joystick
            if (playerInput.currentControlScheme != "Keyboard&Mouse" && lookInput.magnitude < driftControl)
            { lookInput = Vector2.zero; }
            
            if (playerInput.currentControlScheme == "Keyboard&Mouse")
            { // MouseDelta pequeño (en ActionMap)
                horizontalRotation = lookInput.x * mouseSensitivity;
                verticalRotation = lookInput.y * mouseSensitivity;
            }
            else
            { // RightStick grande (este realmente esta bien)
                horizontalRotation = lookInput.x * joystickSensitivity;
                verticalRotation = lookInput.y * joystickSensitivity;
            }
            // rotación del jugador
            transform.Rotate(0, horizontalRotation, 0);
            // rotación vertical de la cámara
            mouseRotation -= verticalRotation;
            mouseRotation = Mathf.Clamp(mouseRotation, -90f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(mouseRotation, 0, 0);
        }

        // aqui cogemos los controles del movimiento
        _movLateral = Input.GetAxis("Horizontal");
        _movFrontal = Input.GetAxis("Vertical");
        // rotamos el sprite y activamos la animacion
        if (_movLateral != 0)
        { transform.localScale = new Vector3(_movLateral > 0 ? -1 : 1, 1, 1); }
        bool isMoving = Mathf.Abs(_movLateral) > 0.1f || Mathf.Abs(_movFrontal) > 0.1f;
        _animator.SetBool("isMoving", isMoving);
        // chequeamos el raycast del salto
        GroundCheck();
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
        { _MC.PastInteracton(); }
        if (other.CompareTag("NPCfut")) //conversar con NPC futuro
        { _MC.FutrInteracton(); }
       
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
        { _MC.PastExit(); }
        if (other.CompareTag("NPCfut")) //cerrar NPC futuro
        { _MC.FutrExit();}
    }
    private void OnCollisionEnter(Collision collision)
    {
        // si choco con algo mortal, me quito las vidas
        if (collision.gameObject.CompareTag("deadly"))
        {
            playerHealth -= 10;
            _MC.UpdateLives(playerHealth);
            _MC.ShowDead();
        }
        if (collision.gameObject.CompareTag("heal") && playerHealth <= 10) // pillo heal si no estoy a tope
        {
            playerHealth += 2;
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
    
    void OnJump() // llamamos Jump ActionMap en Space y LeftTrigger
    {
        if (_coyoteTimer > 0f)
        {
            //actualizamos timer del salto, la altura y damos la fuerza
            _coyoteTimer = 0f;
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        
    }
    void GroundCheck() // raycast para controlar el salto 
    {
        _isGrounded = Physics.Raycast(
        playerFeet.position, Vector3.down, floorDistance, floors );
        if (_isGrounded) _coyoteTimer = coyoteCooldown;
        else _coyoteTimer -= Time.deltaTime;
    }
    void OnDash() // llamamos Dash ActionMap en Shift y LeftShoulder
    {
        if (_canDash) // si puedo, hago el dash
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
    }
    void ResetDASH()
    { _canDash = true; _isDashing = false; }
    public IEnumerator StunnKnockback(Vector3 direction, float force) //del enemy al hitearme
    {
        _isStunned = true; //bloqueo el fixed movement
        _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0); // limpio fisics del rb
        _rb.AddForce(direction * force, ForceMode.Impulse);
        yield return new WaitForSeconds(0.1f);
        _isStunned = false;
    }
    void OnDrawGizmosSelected() // muestro raycast de los pies del player
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(playerFeet.position, playerFeet.position + Vector3.down * floorDistance);
    }
}
