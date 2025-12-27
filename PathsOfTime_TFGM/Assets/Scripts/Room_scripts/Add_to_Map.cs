using UnityEngine;

public class Add_to_Map : MonoBehaviour
{ // este script está en el empty padre de cada sala

    public PAS_rooman _PRM; //singleton PAS_rooman
    public FUT_rooman _FRM; //singleton FUT_rooman

    void Start()
    {
        // pillo el singleton del ROOM_MAN
        _PRM = PAS_rooman.instance;
        _FRM = FUT_rooman.instance;

        if (_PRM != null)
        {
            // añado la sala spawneada al mapa del pasado

            _PRM.roomMap.Add(this.gameObject);
        }
        if (_FRM != null)
        {
            // añado la sala spawneada al mapa del futuro
            _FRM.roomMap.Add(this.gameObject);

        }
    }
}
