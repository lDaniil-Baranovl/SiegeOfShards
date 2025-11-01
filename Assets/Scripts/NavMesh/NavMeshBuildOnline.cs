using UnityEngine;
using Unity.AI.Navigation;
public class NavMeshBuildOnline : MonoBehaviour
{
    NavMeshSurface navMeshhSur;
    void Start()
    {
        navMeshhSur = GetComponent<NavMeshSurface>();
    }
    void Update()
    {
        navMeshhSur.BuildNavMesh();
    }
}
