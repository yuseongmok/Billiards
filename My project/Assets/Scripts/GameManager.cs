using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int player1Score = 0;
    public int player2Score = 0;
    public int currentTurn = 1; // 1 = Player 1, 2 = Player 2

    public Text scoreText;
    public Text turnText;

    [HideInInspector] public bool ballPocketedThisTurn = false; // 이번 턴에 공이 들어갔는지 체크
    private bool isBallMoving = false;
    private Rigidbody[] allBalls;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        allBalls = FindObjectsOfType<Rigidbody>();
        UpdateUI();
    }

    void Update()
    {
        isBallMoving = CheckIfBallsMoving();
    }

    bool CheckIfBallsMoving()
    {
        foreach (Rigidbody rb in allBalls)
        {
            if (rb != null && rb.linearVelocity.magnitude > 0.05f)
                return true;
        }
        return false;
    }

    public bool CanPlayerShoot()
    {
        return !isBallMoving;
    }

    public void ResetTurnState()
    {
        ballPocketedThisTurn = false;
    }

    public void ScorePoint(int playerNum)
    {
        if (playerNum == 1) player1Score++;
        else player2Score++;

        ballPocketedThisTurn = true; // 공이 들어갔음을 기록
        UpdateUI();
    }

    public void CheckAndSwitchTurn()
    {
        // 이번 턴에 공을 넣었다면 턴을 유지, 못 넣었다면 전환
        if (ballPocketedThisTurn)
        {
            ballPocketedThisTurn = false;
            UpdateUI();
            Debug.Log($"Player {currentTurn}이 공을 넣어 한 번 더 칩니다!");
        }
        else
        {
            currentTurn = (currentTurn == 1) ? 2 : 1;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"P1: {player1Score}  |  P2: {player2Score}";
        if (turnText != null) turnText.text = $"Player {currentTurn}'s Turn";
    }
}