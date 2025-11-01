using UnityEngine;
using UnityEngine.AI;

public class FlyingAgent : MonoBehaviour
{
    [Header("Параметры полёта")]
    public float flightHeight = 10f;     // желаемая высота
    public float heightSmooth = 2f;      // скорость плавного подъёма
    public float rotationSmooth = 5f;    // плавность поворота

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;
        agent.updatePosition = false; // мы сами обновляем позицию
        agent.updateRotation = false; // сами поворачиваем
    }

    void Update()
    {
        if (!agent.hasPath) return;

        // Получаем следующую позицию по XZ
        Vector3 targetPos = agent.nextPosition;

        // Плавно поднимаем/опускаем к нужной высоте
        float newY = Mathf.Lerp(transform.position.y, flightHeight, Time.deltaTime * heightSmooth);
        targetPos.y = newY;

        transform.position = targetPos;

        // Плавно поворачиваем в направлении движения
        Vector3 dir = agent.desiredVelocity.normalized;
        if (dir.sqrMagnitude > 0.1f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSmooth);
        }
    }

    public void SetDestination(Vector3 target)
    {
        agent.SetDestination(target);
    }
}