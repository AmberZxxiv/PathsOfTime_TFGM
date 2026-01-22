using System.Collections;
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.AI;

public class Mission_Manager : MonoBehaviour
{// script en NON DESTROY EMPTY
 // SINGLETON script
    public static Mission_Manager instance;
 // SINGLETON script
    public Menus_Control _MC; //pillo SINGLE del MC
    public Player_Control _PC; //pillo SINGLE del PC

    public MissionSelect mission;
    public enum MissionSelect
    {
        None,
        BossMis,
        TokenMis,
        CompaMis
    }

    public GameObject companionPrefab;
    bool _missionCompleted = false;

    void Awake()// singleton sin superponer y no destruir al cambiar escena
    {
        if (instance == null)
        { instance = this; DontDestroyOnLoad(this.gameObject); }
        else Destroy(gameObject);
    }
    void Start()
    { 
        // Pillo Singles
        if (_MC == null) { _MC = Menus_Control.instance; } 
        if (_PC == null) { _PC = Player_Control.instance; }
        if (mission == MissionSelect.CompaMis)
        {
            Invoke("SpawnCompanion", 2.5f);
        }
    }
    void Update()
    {
        // Comprobar TOKENS completada
        if (_PC.coinsLooted >= 5 && mission == MissionSelect.TokenMis && !_missionCompleted)
        {
            _missionCompleted = true;
            print("MISSION COMPLETE!");
            //Instanciar mision complete o algo asi
        }
    }

    void SpawnCompanion()
    {
        Instantiate(companionPrefab, new Vector3(0, 3, 0), transform.rotation);
    }
    public void CompanionLose() // lo llamo desde Companion al morirse
    {
        //Instanciar mision lose o algo asi
        _missionCompleted = true;
        print("MISSION LOSED!");
    }

    public void BossComplete() // lo llamo desde el Boss al morir
    {
        //Instanciar mision complete o algo asi
        _missionCompleted = true;
        print("MISSION COMPLETE!");
    }

    public void BossMission()
    {
        mission = MissionSelect.BossMis;
        _MC.MissionDisplay(mission);
    }
    public void TokenMission()
    {
        mission = MissionSelect.TokenMis;
        _MC.MissionDisplay(mission);
    }
    public void CompanionMission()
    {
        mission = MissionSelect.CompaMis;
        _MC.MissionDisplay(mission);
    }
}
