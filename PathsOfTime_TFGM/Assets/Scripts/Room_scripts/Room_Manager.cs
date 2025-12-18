using UnityEngine;
using Unity.AI.Navigation;
using System.Collections.Generic;

public class Room_Manager : MonoBehaviour
{// script en el empty ROM_MAN del inspector
 // SINGLETON script
    public static Room_Manager instance;
    // SINGLETON script

    #region /// ROOM MAN LIST ///
    public GameObject[] room1Z;
    public GameObject[] room1X;
    public GameObject[] room0z;
    public GameObject[] room0x;
    public GameObject closedRoom;
    #endregion

    #region /// MAP CONTROL ///
    public List<GameObject> roomMap;
    public int roomsSpawned;
    public int maxRooms;
    public NavMeshSurface surface;
    #endregion

    #region /// POW SPAWNER ///
    public List<GameObject> powerHands;
    public List<GameObject> powerSpawns;
    #endregion

    #region /// ENEMY SPAWNER ///
    public GameObject bossBall;
    public GameObject minionBall;
    public int minionCount;
    #endregion


    void Awake()
    { // awake para instanciar singleton sin superponer varios
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SpawnPowerUPs(); // spawnea las powerUPs
        Invoke("BakeNavMesh", 2f); //timer bake navmesh
        Invoke("SpawnEnemy", 2.5f); //timer spawn de enemigos
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

    void BakeNavMesh() //bakea NavMeshSurface de la escena cuando esta completa
    { surface.BuildNavMesh(); }

    void SpawnEnemy()
    {
        //el Boss aparece en la ultima sala de la lista en su 00
        Instantiate(bossBall, roomMap[roomMap.Count-1].transform.position + Vector3.up * 5, transform.rotation);

        // en todas menos la ultima, aparecen minions en los spawners
        float radio = 2f;
        for(int i = 0; i < roomMap.Count-1; i++)
        {
           Transform enemySpawn = roomMap[i].transform.Find("EnemySpawn");
           Vector3 center = enemySpawn.position + Vector3.up * 0.5f;
            for (int m = 0; m < minionCount; m++)
            {
               float angle = (360f / minionCount) * m;
                Vector3 offset = new Vector3
                (Mathf.Cos(angle * Mathf.Deg2Rad), 0,
                 Mathf.Sin(angle * Mathf.Deg2Rad)) * radio;
                Instantiate(minionBall, center + offset, transform.rotation);
            }
        }
    }
}
