using System.Collections;
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Mission_Manager : MonoBehaviour
{// script en DONT DESTROY EMPTY
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
    public bool missionCompleted = false;

    void Awake()
    {
        if (instance == null) // si no hay singleton, esta instancia persiste
        { instance = this; DontDestroyOnLoad(this.gameObject); }
        else if (instance != this) // si la instancia no es esta
        {
            // y estamos en lobby, destruimos la persistente y priorizamos nuevo inicio
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                Destroy(instance.gameObject); // destruye instancia antigua
                instance = this; // asigna nueva instancia persistente
                DontDestroyOnLoad(this.gameObject);
            }
            else // y no estamos en el lobby, persistimos?
            { Destroy(gameObject); }
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) //reseteo mision al cambiar a escena lobby
    {
        //ASEGURO SINGLES AL CAMBIAR ESCENA
        if (_PC == null) { _PC = Player_Control.instance; }
        if (_MC == null) { _MC = Menus_Control.instance; }
        if (scene.buildIndex == 1)
        {
            mission = MissionSelect.None;
            missionCompleted = false;
            _MC.MissionDisplay(mission);
        }
    }

    void Update()
    {
        // Comprobar TOKENS completada
        if (_PC.coinsLooted >= 5 && mission == MissionSelect.TokenMis && !missionCompleted)
        { missionCompleted = true; }
    }

    public void SpawnCompanion() //desde Room_Manager al iniciar sala
    {
        Instantiate(companionPrefab, new Vector3(0, 2, 0), transform.rotation);
        missionCompleted = true;
    }
    public void CompanionLose() //desde Companion al morirse
    {
        // limpio los restos de los cupcakes
        Transform container = _MC.companionContainer.transform;
        for (int i = container.childCount - 1; i >= 0; i--)
        { Destroy(container.GetChild(i).gameObject); }
        // instancio mision lose
        missionCompleted = false;
        Instantiate(_MC.companionDead, container);
    }
    public void BossComplete() // actualizo mision desde Boss al morir
    { missionCompleted = true; }

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
