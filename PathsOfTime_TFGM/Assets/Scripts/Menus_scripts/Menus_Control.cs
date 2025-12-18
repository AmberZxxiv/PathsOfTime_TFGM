using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menus_Control : MonoBehaviour
{ // script en el canvas de cada escena
  // SINGLETON script
    public static Menus_Control instance;
  // SINGLETON script
    public Player_Control _PC; //pillo SINGLE del PC

    public GameObject deadMenu;
    public GameObject pauseMenu;
    public GameObject victoryMenu;
    public List<Hearts_Eater> actualLives = new List<Hearts_Eater>();
    public GameObject heartPrefab;
    public GameObject heartContainer;
    public float heartRadio;

    void Awake()
    {// awake para instanciar singleton sin superponer varios
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // pillo el singleton del Player
        _PC = Player_Control.instance;
        Time.timeScale = 1;
        LiveContainer();
    }

    void Update()
    {
        if (_PC.health <= 0)
        {
            Time.timeScale = 0;
            deadMenu.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.activeSelf)
            {
                QuitPause();
            }
            else
            {
                Time.timeScale = 0;
                pauseMenu.SetActive(true);
            }
        }
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

    public void ShowVictory() //lo llamo desde el TakeDamage del Enemy Boss
    {
        Time.timeScale = 0;
        victoryMenu.SetActive(true);
    }

    public void QuitPause()
    { 
        Time.timeScale = 1;
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
