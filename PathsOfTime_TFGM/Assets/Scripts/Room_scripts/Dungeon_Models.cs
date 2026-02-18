using UnityEngine;

// ScriptableObject crear diferentes modelos dungeon
[CreateAssetMenu(menuName = "Dungeon/Models")]
public class Dungeon_Models : ScriptableObject
{//rellenar en cada uno de los modelos y declarar en Spawner
    public Material floorMat;
    public Mesh floorMesh;
    public Material wallMat;
    public Mesh wallMesh;
    public Material cellMat;
    public Mesh cellMesh;
    public Material platMat;
    public Mesh platMesh;
    public Material rumbMat;
    public Mesh rumbMesh;
    public Material liquidMat;
    public Mesh liquidMesh;
}
