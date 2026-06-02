using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    // 시작 버튼을 누르면 실행될 함수
    public void ClickStartButton()
    {
        // 빌드 세팅에 등록한 GameScene을 불러옵니다.
        SceneManager.LoadScene("GameScenes");
    }
}