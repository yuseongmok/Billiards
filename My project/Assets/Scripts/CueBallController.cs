using UnityEngine;

public class CueBallController : MonoBehaviour
{
    public Rigidbody whiteBallRb;
    public LineRenderer redCord;   // 빨간 끈 Line Renderer

    public float maxForce = 25f;
    public float dragSensitivity = 0.02f;
    public float maxDragDistance = 3f;

    private Vector3 dragStartPos;
    private bool isDragging = false;
    private Vector3 shootDirection;
    private float currentForce = 0f;

    void Start()
    {
        if (redCord != null) redCord.enabled = false;
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.CanPlayerShoot())
        {
            if (isDragging) CancelDrag();
            return;
        }

        // 1. 드래그 시작
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            isDragging = true;
            dragStartPos = Input.mousePosition;
            if (redCord != null) redCord.enabled = true;

            if (GameManager.Instance != null) GameManager.Instance.ResetTurnState();
        }

        // 2. 드래그 중
        if (isDragging)
        {
            Vector3 currentMousePos = Input.mousePosition;
            Vector3 mouseDelta = currentMousePos - dragStartPos;

            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 dragDir = (camRight * mouseDelta.x + camForward * mouseDelta.y) * dragSensitivity;
            float dragLength = Mathf.Clamp(dragDir.magnitude, 0f, maxDragDistance);

            if (dragLength > 0.05f)
            {
                shootDirection = -dragDir.normalized;
                currentForce = (dragLength / maxDragDistance) * maxForce;

                if (redCord != null)
                {
                    redCord.SetPosition(0, transform.position);
                    redCord.SetPosition(1, transform.position + (dragDir.normalized * dragLength));
                }
            }
        }

        // 3. 마우스 놓기
        if (Input.GetKeyUp(KeyCode.Mouse0) && isDragging)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        isDragging = false;
        if (redCord != null) redCord.enabled = false;

        if (currentForce > 0.5f)
        {
            whiteBallRb.AddForce(shootDirection * currentForce, ForceMode.Impulse);
        }

        currentForce = 0f;
        StartCoroutine(WaitAndSwitchTurn());
    }

    void CancelDrag()
    {
        isDragging = false;
        if (redCord != null) redCord.enabled = false;
    }

    System.Collections.IEnumerator WaitAndSwitchTurn()
    {
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => GameManager.Instance.CanPlayerShoot());
        if (GameManager.Instance != null) GameManager.Instance.CheckAndSwitchTurn();
    }
}