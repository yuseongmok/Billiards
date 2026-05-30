using UnityEngine;

public class CueBallController : MonoBehaviour
{
    private Rigidbody rb;
    private LineRenderer lineRenderer;
    private Camera mainCamera;
    private Collider ballCollider; //흰 공의 콜라이더를 제어하기 위한 변수 추가

    [Header("새총 설정")]
    public float maxForce = 40f;        
    public float maxDragDistance = 3f;  
    
    private bool isDragging = false;
    private Vector3 dragStartMousePos; 

    private AudioSource audioSource;
    [Header("사운드 설정")]
    public AudioClip hitClip;           // 재생할 충돌 효과음 파일
    public float minCollisionForce = 1f; // 소리가 나기 위한 최소 충돌 세기

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        ballCollider = GetComponent<Collider>(); //콜라이더 컴포넌트 가져오기
        mainCamera = Camera.main;

        audioSource = GetComponent<AudioSource>();

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        //프리볼 배치 모드일 때는 흰 공의 콜라이더를 잠시 꺼서 레이저를 통과시킵니다.
        if (GameManager.Instance.isPlacingMode)
        {
            if (ballCollider != null) ballCollider.enabled = false;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("TableFloor");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Vector3 mouseWorldPosition = hit.point;
            mouseWorldPosition.y = transform.position.y; 

            // 1. 프리볼 배치 모드일 때
            if (GameManager.Instance.isPlacingMode)
            {
                if (lineRenderer != null) lineRenderer.enabled = false;
                rb.isKinematic = true; 
                transform.position = mouseWorldPosition;

                if (Input.GetMouseButtonDown(0))
                {
                    rb.isKinematic = false; 
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;

                    //배치가 끝났으니 흰 공의 콜라이더를 다시 켭니다.
                    if (ballCollider != null) ballCollider.enabled = true;

                    GameManager.Instance.EndPlacingMode();
                }
                return; 
            }

            // 2. 공이 구르는 중일 때 조작 차단
            if (GameManager.Instance.isBallMoving)
            {
                if (lineRenderer != null) lineRenderer.enabled = false;
                isDragging = false;
                return; 
            }

            // 3. 새총 발사 로직
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                dragStartMousePos = Input.mousePosition; 
                if (lineRenderer != null) lineRenderer.enabled = true;
            }

            if (isDragging)
            {
                Vector3 currentMousePos = Input.mousePosition;
                Vector3 mouseDelta = currentMousePos - dragStartMousePos;

                float dragDistance = mouseDelta.magnitude * 0.01f; 
                if (dragDistance > maxDragDistance) dragDistance = maxDragDistance;

                Vector3 camForward = CameraController.Instance.GetForwardDirection();
                Vector3 camRight = Quaternion.Euler(0, 90, 0) * camForward;

                Vector3 dragDirection = (camForward * mouseDelta.y + camRight * mouseDelta.x).normalized;

                if (lineRenderer != null)
                {
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, transform.position + dragDirection * dragDistance);
                }

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
        else
        {
            //만약 마우스가 당구대 바닥을 벗어났더라도 배치 모드 중이라면 콜라이더를 계속 꺼둡니다.
            if (GameManager.Instance.isPlacingMode && ballCollider != null)
            {
                ballCollider.enabled = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 부딪힌 대상의 태그가 일반 당구공(SolidBall, StripeBall 등)일 때만 소리를 냅니다.
        if (collision.gameObject.CompareTag("SolidBall") || 
            collision.gameObject.CompareTag("StripeBall") || 
            collision.gameObject.CompareTag("BlackBall"))
        {
            // 얼마나 세게 부딪혔는지 '충돌 강도'를 계산합니다.
            float collisionForce = collision.relativeVelocity.magnitude;

            // 너무 미세하게 스친 게 아니라면 소리를 재생합니다.
            if (collisionForce > minCollisionForce && audioSource != null && hitClip != null)
            {
                // 살짝 부딪히면 작게, 세게 부딪히면 크게 들리도록 볼륨을 조절해 주는 디테일입니다.
                float volume = Mathf.Clamp01(collisionForce / 15f); 
                
                // 소리가 겹치더라도 끊기지 않고 팅! 팅! 다 나도록 PlayOneShot으로 재생합니다.
                audioSource.PlayOneShot(hitClip, volume);
            }
        }
    }
}