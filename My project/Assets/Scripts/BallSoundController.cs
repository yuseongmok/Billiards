using UnityEngine;

public class BallSoundController : MonoBehaviour
{
    private AudioSource audioSource;
    
    [Header("사운드 설정")]
    public AudioClip hitClip;           // 재생할 충돌 효과음 파일
    public float minCollisionForce = 1f; // 소리가 나기 위한 최소 충돌 세기

    void Start()
    {
        // 이 공에 붙어있는 AudioSource를 가져옵니다.
        audioSource = GetComponent<AudioSource>();
        
        // 만약 오브젝트에 AudioSource가 없다면 자동으로 추가해 줍니다.
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 시작할 때 자동으로 소리가 나는 현상 방지
        audioSource.playOnAwake = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //내(일반 공)가 '다른 당구공들'이나 '당구대 벽면(쿠션)'에 부딪혔을 때 소리를 냅니다.
        // (흰 공과의 충돌은 흰 공 스크립트가 처리하므로, 여기서는 다른 오브젝트들과의 충돌을 처리합니다.)
        if (collision.gameObject.CompareTag("SolidBall") || 
            collision.gameObject.CompareTag("StripeBall") || 
            collision.gameObject.CompareTag("BlackBall") ||
            collision.gameObject.name.Contains("Wall") || // 오브젝트 이름에 Wall이 들어간 경우 (당구대 벽)
            collision.gameObject.name.Contains("Cushion")) 
        {
            float collisionForce = collision.relativeVelocity.magnitude;

            if (collisionForce > minCollisionForce && audioSource != null && hitClip != null)
            {
                // 속도에 비례한 볼륨 조절
                float volume = Mathf.Clamp01(collisionForce / 15f); 
                audioSource.PlayOneShot(hitClip, volume);
            }
        }
    }
}