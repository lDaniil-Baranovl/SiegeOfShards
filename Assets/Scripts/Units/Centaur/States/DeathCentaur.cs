using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DeathCentaur : CentaurBaseState
{
    private Vector3 deathPosition;
    private Quaternion deathRotation;
    private Transform rootBone;
    private const string deathStateName = "DeathCentaur";
    private bool isAnimationComplete = false;

    public override void EnterState(CentaurStateManager manager)
    {
        Debug.Log("Enter Death State");

        // Сохраняем позицию ДО любых изменений
        deathPosition = manager.transform.position;
        deathRotation = manager.transform.rotation;

        Debug.Log($"Death position: {deathPosition}");

        // Находим root bone
        rootBone = manager.transform;
        if (rootBone != null)
        {
            Debug.Log($"Root bone found: {rootBone.name}");
        }

        // Останавливаем все движения
        StopAllMovement(manager);

        // Отключаем ВСЕ коллайдеры
        DisableAllColliders(manager);

        // Жестко фиксируем позицию
        manager.transform.position = deathPosition;
        manager.transform.rotation = deathRotation;

        // Запускаем анимацию смерти
        if (manager.centaur_animator != null)
        {
            manager.centaur_animator.applyRootMotion = false;
            manager.centaur_animator.SetTrigger("DeathCentaur");
            Debug.Log("Death animation triggered");
        }

        manager.StartCoroutine(WaitAndDestroy(manager));
    }

    private void StopAllMovement(CentaurStateManager manager)
    {
        // NavMeshAgent
        var agent = manager.centaur_navMeshAgent;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
            Debug.Log("NavMeshAgent disabled");
        }

        // Rigidbody
        var rbs = manager.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rbs)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Debug.Log($"Rigidbody {rb.name} kinematic: true");
        }
    }

    private void DisableAllColliders(CentaurStateManager manager)
    {
        var colliders = manager.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
            Debug.Log($"Collider {collider.name} disabled");
        }
    }

    private IEnumerator WaitAndDestroy(CentaurStateManager manager)
    {
        Debug.Log("WaitAndDestroy started");

        // Ждем пока анимация начнется
        yield return new WaitForSeconds(0.1f);

        Animator anim = manager.centaur_animator;
        float timeout = 10f;
        float timer = 0f;

        if (anim != null)
        {
            // Ждем начала анимации смерти
            while (timer < timeout && !anim.GetCurrentAnimatorStateInfo(0).IsName(deathStateName))
            {
                FixPosition(manager);
                timer += Time.deltaTime;
                yield return null;
            }

            if (timer >= timeout)
            {
                Debug.LogWarning("Timeout waiting for death animation to start");
            }

            // Ждем завершения анимации
            timer = 0f;
            while (timer < timeout && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f)
            {
                FixPosition(manager);
                timer += Time.deltaTime;
                yield return null;
            }
        }

        isAnimationComplete = true;
        Debug.Log("Animation complete, destroying object");

        GameObject.Destroy(manager.gameObject);
    }

    private void FixPosition(CentaurStateManager manager)
    {
        // Жесткая фиксация позиции
        manager.transform.position = deathPosition;
        manager.transform.rotation = deathRotation;

        // Дополнительная фиксация через root bone если нужно
        if (rootBone != null && rootBone != manager.transform)
        {
            rootBone.position = deathPosition;
            rootBone.rotation = deathRotation;
        }
    }

    public override void UpdateState(CentaurStateManager manager)
    {
        if (!isAnimationComplete)
        {
            FixPosition(manager);

            // Дебаг-визуализация
            Debug.DrawLine(deathPosition, deathPosition + Vector3.up * 2f, Color.red);
            Debug.DrawLine(manager.transform.position, manager.transform.position + Vector3.up * 2f, Color.green);
        }
    }
    public override void ExitState(CentaurStateManager manager) { }
}