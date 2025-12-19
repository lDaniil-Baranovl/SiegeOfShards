using UnityEngine;

public class DragFlyColdRunState : UnitBaseState<StateManagerFlyColdDragon>
{
    private float delayTimer = 0f;
    private float delayBeforeRun = 0.5f;
    private bool hasStartedRunning = false;

    // Параметры полёта
    private float flightHeightOffset = 0.64f; // Высота над полем боя
    private float moveSpeed = 3f;
    private float rotationSpeed = 5f;
    private int battleFieldLayer;

    public override void EnterState(StateManagerFlyColdDragon manager)
    {
        delayTimer = 0f;
        hasStartedRunning = false;

        // Сохраняем скорость из настроек юнита
        moveSpeed = manager.walkSpeed;

        // Получаем LayerMask для поля боя
        battleFieldLayer = LayerMask.GetMask("BattleField");

        manager.unitAnimator.SetBool("IsRunningdragFlyCold", false);
        manager.unitAnimator.SetBool("IsAttackingdragFlyCold", false);
    }

    public override void ExitState(StateManagerFlyColdDragon manager)
    {
        manager.unitAnimator.SetBool("IsRunningdragFlyCold", false);
    }

    public override void UpdateState(StateManagerFlyColdDragon manager)
    {
        if (!manager.canMove) return;

        if (!hasStartedRunning)
        {
            delayTimer += Time.deltaTime;

            if (delayTimer >= delayBeforeRun)
            {
                hasStartedRunning = true;

                Transform target = manager.GetTarget();
                if (target != null)
                {
                    manager.target = target;
                    manager.unitAnimator.SetBool("IsRunningdragFlyCold", true);
                }
            }
            return;
        }

        // Постоянно обновляем цель
        Transform newTarget = manager.GetTarget();

        if (newTarget != null)
        {
            manager.target = newTarget;

            // Получаем позицию цели
            Vector3 targetPos = manager.GetAttackPosition(newTarget);

            // Движение к цели на фиксированной высоте
            Vector3 direction = (targetPos - manager.transform.position).normalized;
            direction.y = 0; // Игнорируем вертикальное направление

            if (direction.sqrMagnitude > 0.01f)
            {
                // Перемещаем дракона
                Vector3 newPosition = manager.transform.position + direction * moveSpeed * Time.deltaTime;

                // Определяем высоту поля боя под драконом с помощью Raycast
                float groundHeight = GetGroundHeight(newPosition);
                newPosition.y = groundHeight + flightHeightOffset;

                manager.transform.position = newPosition;

                // Поворачиваем дракона к цели
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                manager.transform.rotation = Quaternion.Slerp(
                    manager.transform.rotation,
                    lookRotation,
                    Time.deltaTime * rotationSpeed
                );
            }

            // Проверяем достижение цели
            if (manager.HasReachedTarget())
            {
                manager.SwitchState(manager.dragFlyColdAttackState);
            }
        }
        else
        {
            manager.unitAnimator.SetBool("IsRunningdragFlyCold", false);
        }
    }

    // Определяет высоту поля боя под заданной позицией
    private float GetGroundHeight(Vector3 position)
    {
        RaycastHit hit;
        // Пускаем луч вниз с большой высоты
        Vector3 rayStart = new Vector3(position.x, position.y + 50f, position.z);

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 100f, battleFieldLayer))
        {
            return hit.point.y;
        }

        // Если не нашли поле боя, возвращаем текущую высоту
        return position.y;
    }
}
