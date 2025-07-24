using UnityEngine;
using Unity.AI.Navigation;
public class NavMesh : MonoBehaviour
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
