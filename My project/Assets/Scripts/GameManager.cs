using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("플레이어 상태")]
    public int currentPlayer = 1;
    public int player1Score = 0;
    public int player2Score = 0;

    [Header("화면 UI 연결")]
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p2ScoreText;

    [Header("이펙트 설정")]
    public GameObject pocketEffectPrefab;

    [HideInInspector] public bool isBallMoving = false; // 현재 공들이 움직이는 중인가?
    [HideInInspector] public bool isPlacingMode = false; //현재 흰 공을 자유롭게 배치하는 중인가?

    private bool hasShotThisTurn = false;               // 이번 턴에 이미 공을 쳤는가?
    private bool isGameOver = false;
    private Rigidbody[] allBallRigidbodies;             // 씬에 있는 모든 공들의 리지드바디

    private bool scoredThisTurn = false; //이번 턴에 자신의 공을 넣었는지 체크

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
        // 게임 시작 시 당구대 위의 모든 공들의 Rigidbody를 미리 찾아둡니다.
        UpdateAllBallReferences();
    }

    void Update()
    {
        if (isGameOver || isPlacingMode) return; //배치 모드일 때는 정지 감시를 잠시 멈춥니다.

        if (hasShotThisTurn)
        {
            if (AreAllBallsStopped())
            {
                hasShotThisTurn = false;
                isBallMoving = false;
                
                //모든 공이 멈춘 시점에 턴을 유지할지 바꿀지 결정합니다.
                HandleTurnEnd();
            }
        }
    }

    // 플레이어가 흰 공을 치는 순간 호출될 함수 (CueBallController에서 부를 예정)
    public void OnBallShot()
    {
        hasShotThisTurn = true;
        isBallMoving = true;

        //새로운 턴이 시작되었으므로 보너스 플래그를 초기화합니다.
        scoredThisTurn = false;

        turnText.text = "공이 멈출 때까지 대기 중...";
    }

    // 당구대 위의 모든 공이 멈췄는지 체크하는 함수
    bool AreAllBallsStopped()
    {
        // 맵에 있는 공들이 도중에 삭제되었을 수 있으므로 주기적으로 갱신되거나 null 체크를 합니다.
        foreach (Rigidbody rb in allBallRigidbodies)
        {
            if (rb != null)
            {
                if (rb.linearVelocity.magnitude > 0.01f || rb.angularVelocity.magnitude > 0.01f)
                {
                    return false; // 하나라도 움직이고 있으면 false 반환
                }
            }
        }
        return true; // 모두 멈췄으면 true 반환
    }

    public void BallPocketed(string ballTag, Vector3 spawnPosition)
    {
        if (isGameOver) return;

        if (pocketEffectPrefab != null)
        {
            // 구멍 위치에 이펙트를 복제 생성합니다.
            GameObject effect = Instantiate(pocketEffectPrefab, spawnPosition, Quaternion.identity);
            // 이펙트가 다 터지면 메모리 관리를 위해 2초 뒤 자동으로 파괴되도록 예약합니다.
            Destroy(effect, 2.0f); 
        }

        if (ballTag == "SolidBall")
        {
            player1Score++;
            //플레이어 1의 턴인데 민무늬 공이 들어왔다면 보너스 획득!
            if (currentPlayer == 1) scoredThisTurn = true;
        }
        else if (ballTag == "StripeBall")
        {
            player2Score++;
            //플레이어 2의 턴인데 줄무늬 공이 들어왔다면 보너스 획득!
            if (currentPlayer == 2) scoredThisTurn = true;
        }
        else if (ballTag == "BlackBall")
        {
            isGameOver = true;
            int winner = (currentPlayer == 1) ? 2 : 1;
            turnText.text = $" 승자는 플레이어 {winner}! \n(8번 공 파울)";
            return;
        }

        UpdateUI();
    }

    public void OnWhiteBallFoul()
    {
        if (isGameOver) return;

        Debug.Log("파울! 흰 공이 들어갔습니다. 다음 플레이어 프리볼!");
        
        // 1. 즉시 다음 사람으로 턴을 넘깁니다.
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        
        // 2. 배치 모드를 켜고 조작을 잠급니다.
        isPlacingMode = true;
        isBallMoving = true; 

        turnText.text = $"Player {currentPlayer} - 흰 공을 원하는 위치에 클릭하여 배치하세요!";

        CameraController.Instance.SetTopView(true); //흰 공이 빠지면 프리 배치를 위해 탑뷰(관전 모드)
    }

    public void EndPlacingMode()
    {
        isPlacingMode = false;
        isBallMoving = false; // 이제 조작 잠금을 해제하여 공을 칠 수 있게 만듭니다.
        hasShotThisTurn = false;
        UpdateUI();

        CameraController.Instance.SetTopView(false);


    }


    void HandleTurnEnd()
    {
        if (isGameOver) return;

        CameraController.Instance.SetTopView(false); //공이 다 멈췄으므로 탑뷰를 끄고 다음 사람 조준 모드로 복귀

        if (scoredThisTurn)
        {
            // 자신의 공을 넣었다면 턴을 바꾸지 않고 안내 문구만 띄워줍니다.
            Debug.Log($"플레이어 {currentPlayer} 점수 획득 성공! 한 번 더 공격합니다.");
            UpdateUI(); // UI를 다시 그려서 "Player X Turn"을 유지
        }
        else
        {
            // 공을 못 넣었다면 원래대로 다음 플레이어에게 턴을 넘깁니다.
            SwitchTurn();
        }
    }

    public void SwitchTurn()
    {
        if (isGameOver) return;

        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (isGameOver) return;
        p1ScoreText.text = $"P1: {player1Score}";
        p2ScoreText.text = $"P2: {player2Score}";
        turnText.text = $"Player {currentPlayer} Turn";
    }

    // 씬에 존재하는 모든 공 오브젝트의 Rigidbody를 배열로 가져오는 함수
    public void UpdateAllBallReferences()
    {
        // 꼼꼼한 체크를 위해 씬 안의 모든 Rigidbody를 찾습니다.
        allBallRigidbodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
    }
}