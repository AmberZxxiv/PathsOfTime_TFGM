using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class Room_Spawner : MonoBehaviour
{ // script en trigger RoomSpawner de cada puerta abierta
    public Rooms_Manager _RM; //singleton Rooms_Manager

    // enum para definir la direccion del spawn
    public RoomDirection direction;
    public enum RoomDirection
    { 
        Zplus,
        Xplus,
        Zminus,
        Xminus
    }

    // SPAWNEO CADA X SEGUNDOS PORQUE PETA
    [Range(0.1f, 0.9f)]
    public float spawnTime = 0.2f;
    // SPAWNEO CADA X SEGUNDOS PORQUE PETA
    int _dungeon;
    public bool spawned = false;

    // declaro todas las variables de materiales
    public Material futurFloor;
    public Material pastFloor;
    public Material futurWall;
    public Material pastWall;
    public Material futurCeiling;
    public Material pastCeiling;
    public Material futurPlataform;
    public Material pastPlataform;
    public Material futurRumble;
    public Material pastRumble;
    public Material futurLiquid;
    public Material pastLiquid;

    void Start()
    {
        //pillo SINGLE del RM
        _RM = Rooms_Manager.instance;
        //compruebo la dungeon escogida
        _dungeon = PlayerPrefs.GetInt("Dungeon");
        { Invoke("SpawnRoom", spawnTime); }

    }

    void SpawnRoom()
    {
        GameObject roomToSpawn = null; //declaro la sala variable
        Vector3 roomPos = Vector3.zero; //declaro la posición de referencia
        switch (direction) // segun el estado del enum RoomDirection
        {
           case RoomDirection.Zplus:// para 1Z need 0z
                roomToSpawn = _RM.room0z[Random.Range(0, _RM.room0z.Length)];
           break;

           case RoomDirection.Xplus:// para 1X need 0x
                roomToSpawn = _RM.room0x[Random.Range(0, _RM.room0x.Length)];
           break;
            
           case RoomDirection.Zminus:// para 0z need 1Z
                roomToSpawn = _RM.room1Z[Random.Range(0, _RM.room1Z.Length)];
           break;
        
           case RoomDirection.Xminus:// para 0x need 1X
                roomToSpawn = _RM.room1X[Random.Range(0, _RM.room1X.Length)];
           break;

           default: // si no encuentra estado asignado, detiene el SCRIPT
           Debug.LogWarning("Room Direction no asignada en Room_Spawner");
           return;
        }
        // si he llegado al max de salas, pongo salas cerradas
        if (_RM.roomsSpawned >= _RM.maxRooms && spawned==false && _dungeon == 0)
        { roomToSpawn = _RM.closedPast; roomPos = Vector3.up *5;}
        if (_RM.roomsSpawned >= _RM.maxRooms && spawned == false && _dungeon == 1)
        { roomToSpawn = _RM.closedFutur; roomPos = Vector3.up * 5; }
        // de base, sumo las salas spawneadas
        else { _RM.roomsSpawned++; }
        // instancio la selección y apago el spawn
        GameObject roomInstantiated = Instantiate(roomToSpawn, transform.position + roomPos, transform.rotation);
        FindFloors(roomInstantiated);
        spawned = true;
    }
    void FindFloors(GameObject roomToSearch)
    {
        foreach (Transform child in roomToSearch.GetComponentInChildren<Transform>())
        {
            if (child.CompareTag("ground"))
            { 
              var floor = child.GetComponent<MeshRenderer>();
              floor.material = _dungeon == 0? pastFloor : futurFloor;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // si estoy en el pasado
        if (other.CompareTag("spawnroom") && _RM != null)
        { // pillo el script del trigger
            Room_Spawner otherSpawner = other.GetComponent<Room_Spawner>();
            if (otherSpawner == null)
            { return; } // si no encuentra el componente, se sale
            // si dos spawnrooms chocan, se cierra el pasillo y se elimina el spawn
            if (spawned==false && otherSpawner.spawned==false && _dungeon == 0)
            {
                Instantiate(_RM.closedPast, transform.position + Vector3.up * 5, transform.rotation);
                Destroy(gameObject);
            }
            if (spawned == false && otherSpawner.spawned == false && _dungeon == 1)
            {
                Instantiate(_RM.closedFutur, transform.position + Vector3.up * 5, transform.rotation);
                Destroy(gameObject);
            }
        }
    }
}
