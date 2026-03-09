using UnityEngine;

// ScriptableObject crear diferentes modelos dungeon
[CreateAssetMenu(menuName = "Dungeon/Models")]
public class Dungeon_Models : ScriptableObject
{//rellenar en cada uno de los modelos y declarar en Spawner
    public GameObject floorPref;
    public Material floorMat;
    public Mesh floorMesh;
    public GameObject wallPref;
    public Material wallMat;
    public Mesh wallMesh;
    public GameObject interPref;
    public Material interMat;
    public Mesh interMesh;
    public GameObject cellPref;
    public Material cellMat;
    public Mesh cellMesh;
    public GameObject platPref;
    public Material platMat;
    public Mesh platMesh;
    public GameObject rumbPref;
    public Material rumbMat;
    public Mesh rumbMesh;
    public GameObject liquidPref;
    public Material liquidMat;
    public Mesh liquidMesh;
}
