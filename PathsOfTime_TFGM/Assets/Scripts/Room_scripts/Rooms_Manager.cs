using UnityEngine;
using Unity.AI.Navigation;
using System.Collections.Generic;

public class Rooms_Manager : MonoBehaviour
{// script en el empty ROM_MAN
 // SINGLETON script
    public static Rooms_Manager instance;
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

    #region /// ENEMY SPAWNER ///
    public GameObject bossHydra;
    public GameObject bossAngel;
    public GameObject miniGnobot;
    public int minionCount;
    #endregion


    void Awake()
    { // awake para instanciar singleton sin superponer varios
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Invoke("BakeNavMesh", 2f); //timer bake navmesh
        Invoke("SpawnEnemy", 2.5f); //timer spawn de enemigos
    }

    void BakeNavMesh() //bakea NavMeshSurface de la escena cuando esta completa
    { surface.BuildNavMesh(); }

    void SpawnEnemy()
    {
        int dungeon = PlayerPrefs.GetInt("Dungeon"); //compruebo la dungeon escogida
        if (dungeon == 0) //Hydra aparece en ultima sala en su 00
        {
         Instantiate(bossHydra, roomMap[roomMap.Count - 1].transform.position + Vector3.up * 5, transform.rotation);
        }
        if (dungeon == 1)//Angel aparece en ultima sala en su 00
        {
            Instantiate(bossAngel, roomMap[roomMap.Count - 1].transform.position + Vector3.up * 5, transform.rotation);
        }

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
                Instantiate(miniGnobot, center + offset, transform.rotation);
            }
        }
    }
}
