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

    public MissionSelect mission;
    public enum MissionSelect
    {
        None,
        BossMis,
        TokenMis,
        CompaMis
    }

    void Awake()// singleton sin superponer y no destruir al cambiar escena
    {
        if (instance == null)
        { instance = this; DontDestroyOnLoad(this.gameObject); }
        else Destroy(gameObject);
    }
    void Start()
    { 
        // Pillo Single Menu
        if (_MC == null) { _MC = Menus_Control.instance; } 
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
