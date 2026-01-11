using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menus_Control : MonoBehaviour
{ // script en el canvas de cada escena
  // SINGLETON script
    public static Menus_Control instance;
  // SINGLETON script
    public Player_Control _PC; //pillo SINGLE del PC
    public Weapon_Control _WC; //pillo SINGLE del WC

    public GameObject deadMenu;
    public GameObject pauseMenu;
    public GameObject victoryMenu;

    #region /// HEARTS UI ///
    public List<Hearts_Eater> actualLives = new List<Hearts_Eater>();
    public GameObject heartPrefab;
    public GameObject heartContainer;
    public float heartRadio;
    #endregion

    #region /// WEAPONS UI ///
    public GameObject weaponContainer;
    public GameObject swordPow;
    public GameObject punchPow;
    public GameObject shotPow;
    public GameObject magicPow;
    public Weapon_Control.WeaponType weaponType;
    #endregion

    void Awake()
    {// awake para instanciar singleton sin superponer varios
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // pillo los singles
        _PC = Player_Control.instance;
        _WC = Weapon_Control.instance;
        Time.timeScale = 1;
        LiveContainer();
        // equipo arma inicial
        EquipWeapon(weaponType); 
    }

    void Update()
    {
        if (_PC != null && _PC.health <= 0)
        { ShowDead(); }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.activeSelf)
            {
                QuitPause();
            }
            else
            {
                pauseMenu.SetActive(true);
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                Time.timeScale = 0;
            }
        }
    }

    public void EquipWeapon(Weapon_Control.WeaponType weapon) //llamo Weapon_Control para mostrar arma equipada
    {
        // elimino el marcador del anterior weapon
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
            case Weapon_Control.WeaponType.Magic: iconToInstantiate = magicPow; break;
        }
        Instantiate(iconToInstantiate, weaponContainer.transform);
    }

    void LiveContainer()
    {
        for (int i = 0; i < (_PC.health*0.5f); i++)
        { // instancio corazones en circulo
            GameObject heart = Instantiate(heartPrefab, heartContainer.transform);
            RectTransform rt = heart.GetComponent<RectTransform>();
            float angulo = (360f / (_PC.health * 0.5f)) * i;
            float rad = angulo * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad) * heartRadio;
            float y = Mathf.Sin(rad) * heartRadio;
            rt.anchoredPosition = new Vector2(x, y);
            actualLives.Add(heart.GetComponent<Hearts_Eater>());
        }
        UpdateLives();
    }
    public void UpdateLives()
    {
        float remaining = _PC.health;

        foreach (Hearts_Eater cupcake in actualLives)
        {
            if (remaining >= 2)
            {
                cupcake.EatHeart(2);
                remaining -= 2;
            }
            else if (remaining == 1)
            {
                cupcake.EatHeart(1);
                remaining -= 1;
            }
            else
            {
                cupcake.EatHeart(0);
            }
        }
    }

    public void ShowVictory() //llamo desde TakeDamage de Enemy Boss
    {
        victoryMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Time.timeScale = 0;

    }

    public void ShowDead()
    {
        deadMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Time.timeScale = 0;
    }

    public void QuitPause()
    { 
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseMenu.SetActive(false);
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene(1);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ExitGameApp()
    {
        print("Quitting Game...");
        Application.Quit();
    }
}
