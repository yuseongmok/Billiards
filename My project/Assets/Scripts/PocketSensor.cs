using UnityEngine;

public class PocketSensor : MonoBehaviour
{
    [Header("사운드 설정")]
    public AudioClip goalClip; //포켓 골인 효과음 파일 고정

    private void OnTriggerEnter(Collider other)
    {
        string ballTag = other.tag;

        // 1. 일반 목적구가 구멍에 들어간 경우 (득점)
        if (ballTag == "SolidBall" || ballTag == "StripeBall" || ballTag == "BlackBall")
        {
            //골인 효과음을 구멍 위치(transform.position)에서 재생합니다.
            if (goalClip != null)
            {
                // PlayClipAtPoint는 스피커 컴포넌트 없이도 지정한 위치에서 소리를 한 번 나고 알아서 사라지게 해주는 유용한 함수입니다.
                AudioSource.PlayClipAtPoint(goalClip, transform.position, 1.0f);
            }

            // GameManager에 구멍 위치와 함께 골인 처리 전달
            GameManager.Instance.BallPocketed(ballTag, transform.position);
            Destroy(other.gameObject);
        }
        // 2. 흰 공이 구멍에 들어간 경우 (파울)
        else if (ballTag == "WhiteBall")
        {
            // 🌟 파울일 때도 똑같은 소리를 내거나, 나중에 원하시면 파울용 소리를 따로 연결할 수도 있습니다.
            if (goalClip != null)
            {
                AudioSource.PlayClipAtPoint(goalClip, transform.position, 0.8f);
            }

            Rigidbody whiteRb = other.GetComponent<Rigidbody>();
            if (whiteRb != null)
            {
                whiteRb.linearVelocity = Vector3.zero;
                whiteRb.angularVelocity = Vector3.zero;
            }
            other.transform.position = new Vector3(0f, 1.0f, 0f); 

            GameManager.Instance.OnWhiteBallFoul();
        }
    }
}