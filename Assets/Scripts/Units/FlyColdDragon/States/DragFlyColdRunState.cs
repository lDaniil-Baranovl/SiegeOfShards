using UnityEngine;

public class DragFlyColdRunState : UnitBaseState<StateManagerFlyColdDragon>
{
    private bool hasEnteredFly = false;

    // Параметры полёта
    private float flightHeightOffset = 0.64f; // Высота над полем боя
    private float moveSpeed = 3f;
    private float rotationSpeed = 5f;
    private float heightSmoothSpeed = 5f; // Скорость сглаживания высоты полёта
    private float flightHeightReachedThreshold = 0.05f; // Допустимая погрешность для "набрал высоту"
    private int battleFieldLayer;
    private float lastGroundHeight; // Последняя известная высота поля боя

    public override void EnterState(StateManagerFlyColdDragon manager)
    {
        hasEnteredFly = false;

        // Сохраняем скорость из настроек юнита
        moveSpeed = manager.walkSpeed;

        // Получаем LayerMask для поля боя
        battleFieldLayer = LayerMask.GetMask("BattleField");

        // Инициализируем высоту поля боя по текущей позиции дракона
        float initialGroundHeight;
        if (TryGetGroundHeight(manager.transform.position, out initialGroundHeight))
            lastGroundHeight = initialGroundHeight;
        else
            lastGroundHeight = manager.transform.position.y - flightHeightOffset;

        manager.unitAnimator.SetBool("IsRunningdragFlyCold", false);
    }

    public override void ExitState(StateManagerFlyColdDragon manager)
    {
        manager.unitAnimator.SetBool("IsRunningdragFlyCold", false);
    }

    public override void UpdateState(StateManagerFlyColdDragon manager)
    {
        if (!manager.canMove) return;

        if (!hasEnteredFly)
        {
            // Пока аниматор не вышел из FirstState (дракон сидит на земле),
            // не двигаемся и не набираем высоту - ждём перехода в Fly по Has Exit Time
            if (manager.unitAnimator.GetCurrentAnimatorStateInfo(0).IsName("FirstState"))
                return;

            hasEnteredFly = true;
            manager.unitAnimator.SetBool("IsRunningdragFlyCold", true);
        }

        // Определяем высоту поля боя под драконом с помощью Raycast.
        // Если луч не попал в поле боя (край арены, дыра в коллайдере и т.п.),
        // используем последнюю известную высоту, чтобы не накапливать ошибку
        // и не "улетать" в небо.
        float groundHeight;
        if (TryGetGroundHeight(manager.transform.position, out groundHeight))
            lastGroundHeight = groundHeight;
        else
            groundHeight = lastGroundHeight;

        float targetY = groundHeight + flightHeightOffset;

        // Плавно набираем/теряем высоту полёта. Делается всегда (а не только во время
        // движения к цели), чтобы дракон, заспавнившийся прямо рядом с противником,
        // тоже успел подняться на высоту полёта перед атакой.
        Vector3 position = manager.transform.position;
        position.y = Mathf.Lerp(position.y, targetY, Time.deltaTime * heightSmoothSpeed);
        manager.transform.position = position;

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
                manager.transform.position += direction * moveSpeed * Time.deltaTime;

                // Поворачиваем дракона к цели
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                manager.transform.rotation = Quaternion.Slerp(
                    manager.transform.rotation,
                    lookRotation,
                    Time.deltaTime * rotationSpeed
                );
            }

            // Переходим в атаку только когда долетели до цели и набрали высоту полёта,
            // иначе дракон, заспавнившийся вплотную к противнику, атаковал бы стоя на земле
            bool reachedFlightHeight = Mathf.Abs(manager.transform.position.y - targetY) <= flightHeightReachedThreshold;
            if (manager.HasReachedTarget() && reachedFlightHeight)
            {
                manager.SwitchState(manager.dragFlyColdAttackState);
            }
        }
        else
        {
            manager.unitAnimator.SetBool("IsRunningdragFlyCold", false);
        }
    }

    // Определяет высоту поля боя под заданной позицией.
    // Возвращает false, если луч не попал в коллайдер поля боя.
    private bool TryGetGroundHeight(Vector3 position, out float groundHeight)
    {
        RaycastHit hit;
        // Пускаем луч вниз с большой высоты
        Vector3 rayStart = new Vector3(position.x, position.y + 50f, position.z);

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 100f, battleFieldLayer))
        {
            groundHeight = hit.point.y;
            return true;
        }

        groundHeight = 0f;
        return false;
    }
}
