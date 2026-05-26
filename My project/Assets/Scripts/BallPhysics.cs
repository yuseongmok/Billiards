using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float extraGravity = 50f;
    [SerializeField] private float tableHeight = 0.5f; // 실제 당구대 바닥의 Y축 높이값

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            if (transform.position.y > tableHeight + 0.01f)
            {
                rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
            }
            else if (transform.position.y < tableHeight - 0.2f)
            {   
                rb.useGravity = true; // 유니티 기본 중력만 사용
            }
        }
    }
}
