using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Menus_Control : MonoBehaviour
{ // script en PREF padre CANVAS
  // SINGLETON script
    public static Menus_Control instance;
  // SINGLETON script
    public Player_Control _PC; //pillo SINGLE del PC
    public Companion_Control _CC; //pillo SINGLE del CC
    public Weapon_Control _WC; //pillo SINGLE del WC
    public Mission_Manager _MM; //pillo SINGLE del MM

    #region /// MENUS BASE ///
    public GameObject deadMenu;
    public GameObject pauseMenu;
    public GameObject exitMenu;
    public GameObject loadScreen;
    public GameObject mainMenu;
    public GameObject currentMenu;
    PlayerInput _playerInput;
    #endregion

    #region /// HEARTS UI ///
    public List<Hearts_Eater> actualLives = new List<Hearts_Eater>();
    public List<Hearts_Eater> companionLives = new List<Hearts_Eater>();
    public GameObject heartPrefab;

    public GameObject heartContainer;
    public float heartRadio;

    public GameObject companionContainer;
    public GameObject companionDead;
    public float companionRadio;
    #endregion

    #region /// MISSIONS UI ///
    public GameObject missionContainer;
    public TMP_Text coinText;
    public GameObject tokenSign;
    public GameObject bossSign;
    public GameObject compaSign;
    public GameObject futrPanel;
    public GameObject pastPanel;
    public TextMeshProUGUI exitText;
    #endregion

    #region /// WEAPONS UI ///
    public GameObject weaponContainer;
    public GameObject swordPow;
    public GameObject punchPow;
    public GameObject shotPow;
    public GameObject magicPow;
    #endregion

    void Awake()
    {// awake para instanciar singleton sin superponer varios
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
   // uso sistema de eventos para cambios de escena
    void OnEnable()
    {SceneManager.sceneLoaded += OnSceneLoaded;}
    void OnDisable()
    {SceneManager.sceneLoaded -= OnSceneLoaded;}
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1;
        // pillo los singles
        _PC = Player_Control.instance;
        _WC = Weapon_Control.instance;
        _MM = Mission_Manager.instance;
        // pilla el Input Action
        _playerInput = GetComponent<PlayerInput>(); ;
        // menu principal seleccionable
       if (scene.buildIndex == 0)
        { currentMenu = mainMenu; }
        // menu de carga para la dungeon
        if (scene.buildIndex == 2)
        { StartCoroutine(DungeonLoadScreen()); }
    }
    IEnumerator DungeonLoadScreen()
    {
        _PC.enabled = false;
        loadScreen.SetActive(true);
        yield return new WaitForSecondsRealtime(3f);
        loadScreen.SetActive(false);
        _PC.enabled = true;
    }
    void Start()
    {
        // equipo vidas y monedas
        if (_PC != null)
        {
            LiveContainer(_PC.playerHealth);
            CoinsCounter(_PC.coinsLooted);
        }
        // equipo arma inicial
        if (_WC != null) EquipWeapon(_WC.weapon);
        // equipo la mision
        if (_MM != null) MissionDisplay(_MM.mission);
        if (_MM.mission == Mission_Manager.MissionSelect.CompaMis)
        { Invoke("LiveCompanier", 3f);}
    }
    void Update()
    {
        _CC = Companion_Control.instance; //aseguro pillar al compa cuando aparezca
        if (_PC != null && _PC.playerHealth <= 0) // panel de muerte al morir
        { ShowDead(); }
        // SOLO detectar movimiento de joystick si hay un menú activo
        if (currentMenu != null && currentMenu.activeInHierarchy)
        {
            // al mover RightStik si no hay botón seleccionado 
            InputAction playerLook = _playerInput.actions["Point"];
            Vector2 moveInput = playerLook.ReadValue<Vector2>();
            if ((moveInput.x != 0 || moveInput.y != 0) &&
                EventSystem.current.currentSelectedGameObject == null)
            { SelectFirstActiveButtonInMenu(currentMenu);}
        }
    }
    void SelectFirstActiveButtonInMenu(GameObject menu)
    {
        if (menu == null) return;
        Button[] buttons = menu.GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            if (btn.gameObject.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(btn.gameObject);
                return;
            }
        }
    }
    public void EquipWeapon(Weapon_Control.WeaponType weapon) //llamo desde WC para mostrar arma equipada
    {
        // elimino el marcador del anterior weapon
        if (weaponContainer == null) return;
        foreach (Transform child in weaponContainer.transform)
        { Destroy(child.gameObject); }
        // declaro el weapon que voy a instanciar en la UI
        GameObject iconToInstantiate = null;
        switch (weapon)
        {
            case Weapon_Control.WeaponType.None: return;
            case Weapon_Control.WeaponType.Sword: iconToInstantiate = swordPow; break;
            case Weapon_Control.WeaponType.Punch: iconToInstantiate = punchPow; break;
            case Weapon_Control.WeaponType.Shot: iconToInstantiate = shotPow; break;
            case Weapon_Control.WeaponType.Spell: iconToInstantiate = magicPow; break;
        }
        Instantiate(iconToInstantiate, weaponContainer.transform);
    }
    public void MissionDisplay(Mission_Manager.MissionSelect mission) //llamo desde MM para mostrar mision seleccionada
    {
        // elimino el anterior display
        if (missionContainer == null) return;
        foreach (Transform child in missionContainer.transform)
        { Destroy(child.gameObject); }
        // declaro el weapon que voy a instanciar en la UI
        GameObject displayToInstantiate = null;
        switch (mission)
        {
            case Mission_Manager.MissionSelect.None: return;
            case Mission_Manager.MissionSelect.BossMis: displayToInstantiate = bossSign; break;
            case Mission_Manager.MissionSelect.TokenMis: displayToInstantiate = tokenSign; break;
            case Mission_Manager.MissionSelect.CompaMis: displayToInstantiate = compaSign; break;
        }
        Instantiate(displayToInstantiate, missionContainer.transform);
    }

    void LiveContainer(int playerHealth) //en start instancio total corazones en circulo
    {
        for (int i = 0; i < (_PC.playerHealth*0.5f); i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartContainer.transform);
            RectTransform rt = heart.GetComponent<RectTransform>();
            float angulo = (360f / (_PC.playerHealth * 0.5f)) * i;
            float rad = angulo * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad) * heartRadio;
            float y = Mathf.Sin(rad) * heartRadio;
            rt.anchoredPosition = new Vector2(x, y);
            actualLives.Add(heart.GetComponent<Hearts_Eater>());
            // añado a la lista el script del corazon
        }
        UpdateLives(_PC.playerHealth);
    }
    public void UpdateLives(int playerHealth) // todo lo que varia la vida del player
    {
        foreach (Hearts_Eater cupcake in actualLives) //recorro los estados de los corazones
        {
            if (playerHealth >= 2)
            { cupcake.EatHeart(2); playerHealth -= 2; }
            else if (playerHealth == 1)
            { cupcake.EatHeart(1); playerHealth -= 1;}
            else
            { cupcake.EatHeart(0); }
        }
    }
    void LiveCompanier()
    {
        int compaCakes = _CC.companionHealth;
        for (int i = 0; i < (compaCakes * 0.5f); i++)
        {
            GameObject heart = Instantiate(heartPrefab, companionContainer.transform);
            RectTransform rt = heart.GetComponent<RectTransform>();
            float angulo = (360f / (compaCakes * 0.5f)) * i;
            float rad = angulo * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad) * companionRadio;
            float y = Mathf.Sin(rad) * companionRadio;
            rt.anchoredPosition = new Vector2(x, y);
            rt.localScale = Vector3.one * 0.5f;
            companionLives.Add(heart.GetComponent<Hearts_Eater>());
        }
        UpdateCompaniers(compaCakes);
    }
    public void UpdateCompaniers(int companionHealth) // todo lo que varia la vida del compa
    {
        foreach (Hearts_Eater heart in companionLives)
        {
            if (companionHealth >= 2)
            { heart.EatHeart(2); companionHealth -= 2; }
            else if (companionHealth == 1)
            { heart.EatHeart(1); companionHealth -= 1; }
            else
            { heart.EatHeart(0); }
        }
    }
    public void CoinsCounter(int coinsLooted) //en start y Player collision
    { coinText.text = "x" + coinsLooted.ToString(); }
    public void FutrInteracton() //desde Player collision
    {
        futrPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        currentMenu = futrPanel;
        SelectFirstActiveButtonInMenu(currentMenu);
    }
    public void PastInteracton() //desde Player collision
    {
        pastPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        currentMenu = pastPanel;
        SelectFirstActiveButtonInMenu(currentMenu);
    }
    public void FutrExit() //desde Player collision
    {
        futrPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void PastExit() //desde Player collision
    {
        pastPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void ShowExit() //desde Player collision
    {
        exitMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        // actualizo el texto dependiendo del resultado de la mision
        if (_MM.missionCompleted == false)
            exitText.text = "<color=black>Nice Try!\nBut we failed\nthe mission...";
        if (_MM.missionCompleted == true)
            exitText.text = "<color=white>Just in time!\nYou completed\nthe mission!";

        Time.timeScale = 0;
    }
    public void ShowDead() //en update y Player collision
    {
        deadMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        currentMenu = deadMenu;
        SelectFirstActiveButtonInMenu(currentMenu);
        Time.timeScale = 0;
    }
    void OnPause()
    {
        if (pauseMenu.activeSelf)
            { QuitPause();}
        else
            {
            pauseMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            currentMenu = pauseMenu;
            SelectFirstActiveButtonInMenu(currentMenu);
            Time.timeScale = 0;
            }
    }
    public void QuitPause()
    { 
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseMenu.SetActive(false);
    }
    public void LoadMainScene()
    { SceneManager.LoadScene(1); }
    public void ReturnToMenu()
    { SceneManager.LoadScene(0); }
    public void ExitGameApp()
    { print("Quitting Game..."); Application.Quit(); }
}
