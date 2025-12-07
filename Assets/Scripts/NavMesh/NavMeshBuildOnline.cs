using UnityEngine;
using Unity.AI.Navigation;
public class NavMeshBuildOnline : MonoBehaviour
{
    NavMeshSurface navMeshhSur;
    void Start()
    {
        navMeshhSur = GetComponent<NavMeshSurface>();
    }
    public void BuildNavMeshOnce()
    {
        navMeshhSur.BuildNavMesh();
    }
}
