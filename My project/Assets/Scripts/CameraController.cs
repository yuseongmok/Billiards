using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [Header("대상 설정")]
    public Transform target;            // 추적할 대상 (흰 공)
    public Transform tableCenter;       // 당구대 중심 (프리 배치 탑뷰용)

    [Header("시점 회전 설정")]
    public float xSpeed = 200.0f;       // 좌우 회전 속도
    public float ySpeed = 120.0f;       // 상하 회전 속도
    public float distance = 2.5f;       // 흰 공과 카메라 사이의 거리
    public float yMinLimit = 10f;       // 상하 회전 최소 각도
    public float yMaxLimit = 80f;       // 상하 회전 최대 각도

    [Header("탑뷰 설정")]
    public float topViewHeight = 7.0f;  // 탑뷰일 때 카메라 높이 (당구대가 다 보이도록 조금 높였습니다)

    private float x = 0.0f;
    private float y = 0.0f;
    private bool isTopView = false;     // 현재 탑뷰 상태인가?

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        //1. 프리 배치 상태여서 탑뷰가 켜졌을 때
        if (isTopView)
        {
            if (tableCenter == null) return;

            // 당구대 중심 위 하늘에서 수직으로 내려다봅니다.
            Vector3 targetTopPos = tableCenter.position + Vector3.up * topViewHeight;
            
            // 부드럽게 탑뷰 위치와 회전으로 이동시킵니다.
            transform.position = Vector3.Lerp(transform.position, targetTopPos, Time.deltaTime * 7f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Vector3.down, Vector3.forward), Time.deltaTime * 7f);
        }
        //2. 일반 평소 상태 (조준 중이거나 공이 구르는 것을 지켜볼 때)
        else
        {
            // 마우스 우클릭을 누르고 있을 때만 시점 회전 계산
            if (Input.GetMouseButton(1))
            {
                x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
            }

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            // 흰 공 뒤에 카메라를 부드럽게 고정시킵니다.
            transform.rotation = rotation;
            transform.position = position;
        }
    }

    //탑뷰 상태를 외부에 켜고 끄는 함수
    public void SetTopView(bool enable)
    {
        isTopView = enable;
    }

    public Vector3 GetForwardDirection()
    {
        Vector3 forward = transform.forward;
        forward.y = 0; 
        return forward.normalized;
    }
}
