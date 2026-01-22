using System.Collections.Generic;
using System.Reflection;
using Unity.AI.Navigation;
using UnityEngine;
using static Mission_Manager;

public class Rooms_Manager : MonoBehaviour
{// script en el empty ROM_MAN
 // SINGLETON script
    public static Rooms_Manager instance;
 // SINGLETON script
    public Mission_Manager _MM; //pillo SINGLE del MM

    #region /// ROOM MAN LIST ///
    public GameObject[] room1Z;
    public GameObject[] room1X;
    public GameObject[] room0z;
    public GameObject[] room0x;
    public GameObject closedPast;
    public GameObject closedFutur;
    #endregion

    #region /// MAP CONTROL ///
    public List<GameObject> roomMap;
    public int roomsSpawned;
    public int maxRooms;
    public NavMeshSurface surface;
    public GameObject lootChest;
    public GameObject exitPortal;
    #endregion

    #region /// ENEMY SPAWNER ///
    public GameObject bossHydra;
    public GameObject bossAngel;
    public GameObject miniGnobot;
    public GameObject miniDronlibri;
    public GameObject miniTurrem;
    public int minionCount;
    #endregion


    void Awake()
    { // awake para instanciar singleton sin superponer varios
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        //pillo SINGLE del MM
        _MM = Mission_Manager.instance;
        Invoke("BakeNavMesh", 2f);
        Invoke("SpawnEnemy", 2.5f);
        Invoke("SpawnChest", 3f);
        Invoke("SpawnExit", 3.5f);
        if (_MM.mission == Mission_Manager.MissionSelect.CompaMis)
        {
            _MM.Invoke("SpawnCompanion", 2.5f);
        }
    }

    void BakeNavMesh() //bakea NavMeshSurface de la escena cuando esta completa
    { surface.BuildNavMesh(); }

    void SpawnEnemy()
    {
        float percentTorrem = 0.25f;
        int dungeon = PlayerPrefs.GetInt("Dungeon"); //compruebo la dungeon escogida
        if (dungeon == 0) //enemigos del pasado
        {
            //Hydra aparece en ultima sala en su 00
            Instantiate(bossHydra, roomMap[roomMap.Count - 1].transform.position + Vector3.up * 5, transform.rotation);
            // spawneo gnobots o torrems en las salas menos la ultima
            for (int i = 0; i < roomMap.Count - 1; i++)
            {
                Transform enemySpawn = roomMap[i].transform.Find("EnemySpawn");
                Vector3 center = enemySpawn.position + Vector3.up * 0.5f;
                //decido si spawneo torrem
                if (Random.value <= percentTorrem)
                { Instantiate(miniTurrem, center, transform.rotation); }
                // sino spawneo grupo de gnobots
                else
                {
                    float radio = 2f;
                    for (int m = 0; m < minionCount; m++)
                    {
                        float angle = (360f / minionCount) * m;
                        Vector3 offset = new Vector3
                        (Mathf.Cos(angle * Mathf.Deg2Rad), 0,
                         Mathf.Sin(angle * Mathf.Deg2Rad)) * radio;
                        Instantiate(miniGnobot, center + offset, transform.rotation);
                    }
                }
            }
        }
        if (dungeon == 1) //enemigos del futuro
        {
            //Angel aparece en ultima sala en su 00
            Instantiate(bossAngel, roomMap[roomMap.Count - 1].transform.position + Vector3.up * 5, transform.rotation);
            // spawneo dronlibris o torrems en las salas menos la ultima
            for (int i = 0; i < roomMap.Count - 1; i++)
            {
                Transform enemySpawn = roomMap[i].transform.Find("EnemySpawn");
                Vector3 center = enemySpawn.position + Vector3.up * 0.5f;
                //decido si spawneo torrem
                if (Random.value <= percentTorrem)
                { Instantiate(miniTurrem, center, transform.rotation); }
                // sino spawneo grupo de dronlibris
                else
                {
                    float radio = 2f;
                    for (int m = 0; m < minionCount; m++)
                    {
                        float angle = (360f / minionCount) * m;
                        Vector3 offset = new Vector3
                        (Mathf.Cos(angle * Mathf.Deg2Rad), 0,
                         Mathf.Sin(angle * Mathf.Deg2Rad)) * radio;
                        Instantiate(miniDronlibri, center + offset, transform.rotation);
                    }
                }
            }
        }
    }

    void SpawnChest()
    {
        float chestProbability = 0.1f;
        for (int i = 0; i < roomMap.Count; i++) // en todas las salas
        {
            Transform chestSpawn = roomMap[i].transform.Find("LootSpawn");
            if (chestSpawn == null) continue;
            if (Random.value <= chestProbability)
            {
                Instantiate(lootChest, chestSpawn.position, transform.rotation);
            }
        }
    }

    void SpawnExit()
    {
        //portal de salida aparece en la ultima sala
        Transform exitSpawn = roomMap[roomMap.Count - 1].transform.Find("ExitSpawner");
        Instantiate(exitPortal, exitSpawn.position + Vector3.up * 5f, exitSpawn.rotation);
    }
}
