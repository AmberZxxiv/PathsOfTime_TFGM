using UnityEngine;
using Unity.AI.Navigation;
using System.Collections.Generic;
using System;

public class Lobby_Manager : MonoBehaviour
{// script en empty padre del LOBBY
 // SINGLETON script
    public static Lobby_Manager instance;
    // SINGLETON script

    public List<GameObject> powerHands;
    public List<GameObject> powerSpawns;
    
    public GameObject futrPanel;
    public GameObject pastPanel;

    void Awake()
    { // awake para instanciar singleton sin superponer varios
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    
    private void Start()
    {
        SpawnPowerUPs(); // spawnea las powerUPs
    }

    void SpawnPowerUPs()// busco un pow random no repetido de la lista y se lo doy a cada spawner
    {
        List<GameObject> availablePower = new List<GameObject>(powerHands);
        foreach (GameObject spawn in powerSpawns)
        {
            if (availablePower.Count == 0) break;

            int randPow = UnityEngine.Random.Range(0, availablePower.Count);
            GameObject prefab = availablePower[randPow];
            Instantiate(prefab, spawn.transform.position, prefab.transform.rotation);
            availablePower.RemoveAt(randPow);
        }
    }

    public void FutrInteracton()
    { 
        futrPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    public void PastInteracton()
    { 
        pastPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    public void FutrExit()
    { 
        futrPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void PastExit()
    {
        pastPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
