using UnityEngine;

public class CueBallController : MonoBehaviour
{
    private Rigidbody rb;
    private LineRenderer lineRenderer;
    private Camera mainCamera;
    private Collider ballCollider; 
    
    private AudioSource audioSource;
    [Header("사운드 설정")]
    public AudioClip hitClip;           
    public float minCollisionForce = 1f; 

    [Header("새총 설정")]
    public float maxForce = 40f;        
    public float maxDragDistance = 3f;  
    
    private bool isDragging = false;
    private Vector3 dragStartMousePos; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        ballCollider = GetComponent<Collider>(); 
        audioSource = GetComponent<AudioSource>();
        mainCamera = Camera.main;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // 1. 프리볼 배치 모드일 때 (콜라이더 끄고 바닥 레이캐스트 필수)
        if (GameManager.Instance.isPlacingMode)
        {
            if (ballCollider != null) ballCollider.enabled = false;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = LayerMask.GetMask("TableFloor");

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                Vector3 mouseWorldPosition = hit.point;
                mouseWorldPosition.y = transform.position.y; 
                rb.isKinematic = true; 
                transform.position = mouseWorldPosition;

                if (Input.GetMouseButtonDown(0))
                {
                    rb.isKinematic = false; 
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    if (ballCollider != null) ballCollider.enabled = true;
                    GameManager.Instance.EndPlacingMode();
                }
            }
            return; 
        }

        //공이 구르는 중일 때 조작 차단
        if (GameManager.Instance.isBallMoving)
        {
            if (lineRenderer != null) lineRenderer.enabled = false;
            isDragging = false;
            return; 
        }

        // 마우스 왼쪽 버튼을 처음 누르는 순간 (이때는 흰 공 근처나 바닥을 누르므로 체크)
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragStartMousePos = Input.mousePosition; 
            if (lineRenderer != null) lineRenderer.enabled = true;
        }

        //마우스를 당기는 중일 때: 이제 마우스 커서가 벽 밖으로 넘어가든 하늘로 가든 상관없이 '무조건' 작동합니다!
        if (isDragging)
        {
            Vector3 currentMousePos = Input.mousePosition;
            Vector3 mouseDelta = currentMousePos - dragStartMousePos;

            float dragDistance = mouseDelta.magnitude * 0.01f; 
            if (dragDistance > maxDragDistance) dragDistance = maxDragDistance;

            //카메라 시점 기준 방향 계산
            Vector3 camForward = CameraController.Instance.GetForwardDirection();
            Vector3 camRight = Quaternion.Euler(0, 90, 0) * camForward;

            Vector3 dragDirection = (camForward * mouseDelta.y + camRight * mouseDelta.x).normalized;

            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, transform.position + dragDirection * dragDistance);
            }

            //마우스를 놓았을 때 발사!
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                if (lineRenderer != null) lineRenderer.enabled = false;

                Vector3 shootDirection = -dragDirection;
                float powerRatio = dragDistance / maxDragDistance;
                Vector3 finalForce = shootDirection * (powerRatio * maxForce);

                rb.AddForce(finalForce, ForceMode.Impulse);
                GameManager.Instance.OnBallShot();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("SolidBall") || 
            collision.gameObject.CompareTag("StripeBall") || 
            collision.gameObject.CompareTag("BlackBall"))
        {
            float collisionForce = collision.relativeVelocity.magnitude;

            if (collisionForce > minCollisionForce && audioSource != null && hitClip != null)
            {
                float volume = Mathf.Clamp01(collisionForce / 15f); 
                audioSource.PlayOneShot(hitClip, volume);
            }
        }
    }
}