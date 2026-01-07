using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class Room_Spawner : MonoBehaviour
{ // script en trigger RoomSpawner de cada puerta abierta
    public PAS_rooman _PRM; //singleton PAS_rooman
    public FUT_rooman _FRM; //singleton FUT_rooman


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
    public bool spawned = false;

    void Start()
    {
        //pillo SINGLE del RM
        _PRM = PAS_rooman.instance;
        _FRM = FUT_rooman.instance;
        // si pillo pasado activo pasado
        if (_PRM != null)
        { Invoke("SpawnPastRoom", spawnTime);}
        // si pillo futuro activo futuro
        if (_FRM != null)
        { Invoke("SpawnFutureRoom", spawnTime);}

    }

    void SpawnPastRoom()
    {
        GameObject roomToSpawn = null; //declaro la sala variable
        Vector3 roomPos = Vector3.zero; //declaro la posición de referencia
        switch (direction) // segun el estado del enum RoomDirection
        {
           case RoomDirection.Zplus:// para 1Z need 0z
                roomToSpawn = _PRM.room0z[Random.Range(0, _PRM.room0z.Length)];
           break;

           case RoomDirection.Xplus:// para 1X need 0x
                roomToSpawn = _PRM.room0x[Random.Range(0, _PRM.room0x.Length)];
           break;
            
           case RoomDirection.Zminus:// para 0z need 1Z
                roomToSpawn = _PRM.room1Z[Random.Range(0, _PRM.room1Z.Length)];
           break;
        
           case RoomDirection.Xminus:// para 0x need 1X
                roomToSpawn = _PRM.room1X[Random.Range(0, _PRM.room1X.Length)];
           break;

           default: // si no encuentra estado asignado, detiene el SCRIPT
           Debug.LogWarning("Room Direction no asignada en Room_Spawner");
           return;
        }
        if (_PRM.roomsSpawned >= _PRM.maxRooms && spawned==false)
        { // al max de salas, sala cerrada en altura
            roomToSpawn = _PRM.closedRoom;
            roomPos = Vector3.up *5;
        }
        else { _PRM.roomsSpawned++; } // de base, sumo las salas spawneadas
        // instancio la selección y apago el spawn
        Instantiate(roomToSpawn, transform.position + roomPos, transform.rotation);
        spawned = true;
    }

    void SpawnFutureRoom()
    {
        GameObject roomToSpawn = null; //declaro la sala variable
        Vector3 roomPos = Vector3.zero; //declaro la posición de referencia
        switch (direction) // segun el estado del enum RoomDirection
        {
            case RoomDirection.Zplus:// para 1Z need 0z
                roomToSpawn = _FRM.room0z[Random.Range(0, _FRM.room0z.Length)];
                break;

            case RoomDirection.Xplus:// para 1X need 0x
                roomToSpawn = _FRM.room0x[Random.Range(0, _FRM.room0x.Length)];
                break;

            case RoomDirection.Zminus:// para 0z need 1Z
                roomToSpawn = _FRM.room1Z[Random.Range(0, _FRM.room1Z.Length)];
                break;

            case RoomDirection.Xminus:// para 0x need 1X
                roomToSpawn = _FRM.room1X[Random.Range(0, _FRM.room1X.Length)];
                break;

            default: // si no encuentra estado asignado, detiene el SCRIPT
                Debug.LogWarning("Room Direction no asignada en Room_Spawner");
                return;
        }
        if (_FRM.roomsSpawned >= _FRM.maxRooms && spawned == false)
        { // al max de salas, sala cerrada en altura
            roomToSpawn = _FRM.closedRoom;
            roomPos = Vector3.up * 10;
        }
        else { _FRM.roomsSpawned++; } // de base, sumo las salas spawneadas
        // instancio la selección y apago el spawn
        Instantiate(roomToSpawn, transform.position + roomPos, transform.rotation);
        spawned = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // si estoy en el pasado
        if (other.CompareTag("spawnroom") && _PRM != null)
        { // pillo el script del trigger
            Room_Spawner otherSpawner = other.GetComponent<Room_Spawner>();
            if (otherSpawner == null)
            { return; } // si no encuentra el componente, se sale
            if (spawned==false && otherSpawner.spawned==false)
            {// si dos spawnrooms chocan, se cierra el pasillo y se elimina el spawn
                Instantiate(_PRM.closedRoom, transform.position + Vector3.up * 5, transform.rotation);
                Destroy(gameObject);
            }
        }

        // si estoy en el futuro
        if (other.CompareTag("spawnroom") && _FRM != null)
        { // pillo el script del trigger
            Room_Spawner otherSpawner = other.GetComponent<Room_Spawner>();
            if (otherSpawner == null)
            { return; } // si no encuentra el componente, se sale
            if (spawned == false && otherSpawner.spawned == false)
            {// si dos spawnrooms chocan, se cierra el pasillo y se elimina el spawn
                Instantiate(_FRM.closedRoom, transform.position + Vector3.up * 10, transform.rotation);
                Destroy(gameObject);
            }
        }
    }
}
