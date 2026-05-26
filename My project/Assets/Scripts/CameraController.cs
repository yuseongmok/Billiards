using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // 흰 공
    public float rotateSpeed = 150f;

    private float mouseX = 0f;

    void Start()
    {
        if (target == null) return;
        mouseX = transform.localEulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        //공이 움직이든 말든 카메라는 항상 흰 공 위치를 쫓아갑니다.
        transform.position = target.position;


        if (Input.GetKey(KeyCode.Mouse1)) // 마우스 우클릭
        {
            mouseX += Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
        }

        // 키보드 A, D나 좌우 방향키로도 언제나 회전 가능
        mouseX += Input.GetAxis("Horizontal") * rotateSpeed * 0.5f * Time.deltaTime;

        // 회전 적용
        transform.rotation = Quaternion.Euler(0f, mouseX, 0f);
    }
}