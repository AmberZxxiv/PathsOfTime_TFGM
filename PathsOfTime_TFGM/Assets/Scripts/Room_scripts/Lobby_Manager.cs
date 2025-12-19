using UnityEngine;
using Unity.AI.Navigation;
using System.Collections.Generic;

public class Lobby_Manager : MonoBehaviour
{// script en el empty ROM_MAN del inspector
 // SINGLETON script
    public static Lobby_Manager instance;
 // SINGLETON script
  
    public List<GameObject> powerHands;
    public List<GameObject> powerSpawns;


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

            int randPow = Random.Range(0, availablePower.Count);
            GameObject prefab = availablePower[randPow];
            Instantiate(prefab, spawn.transform.position, prefab.transform.rotation);
            availablePower.RemoveAt(randPow);
        }
    }
}
