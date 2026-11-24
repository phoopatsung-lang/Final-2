using UnityEngine;
using TMPro;

public class PlayerEat : MonoBehaviour
{
    public enum PlayerSide { Player1, Player2 }

    [Header("Player Settings")]
    public PlayerSide playerSide = PlayerSide.Player1; // ตัวละครฝั่ง Player 1 เลือก Player1, ฝั่ง Player 2 เลือก Player2
    public string targetTag = "Player2";                // ชี้ไปที่ Tag ของฝั่งตรงข้าม
    public GameUIController gameUI;

    public float growAmount = 0.1f;

    [Header("Score Settings")]
    public int score = 0;
    public int scorePerEnemy = 10;
    public TextMeshProUGUI scoreText;

    private bool hasWon = false;

    void Awake()
    {
        if (gameUI == null)
        {
            gameUI = FindFirstObjectByType<GameUIController>();
        }
    }

    void Start()
    {
        UpdateScore();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasWon) return;

        // 1. ตรวจสอบว่าสิ่งที่ชนคือ Enemy หรือไม่
        Transform enemy = other.transform;
        while (enemy != null && !enemy.CompareTag("Enemy"))
        {
            enemy = enemy.parent;
        }

        if (enemy != null)
        {
            score += scorePerEnemy;
            UpdateScore();
            Grow();

            Debug.Log($"{gameObject.name} ({playerSide}) กิน Enemy ได้คะแนน: {score}");
            Destroy(enemy.gameObject);
            return;
        }

        // 2. ตรวจสอบว่าสิ่งที่ชนคือ Player ฝ่ายตรงข้ามหรือไม่
        PlayerEat opponent = other.GetComponentInParent<PlayerEat>();
        if (opponent == null || opponent == this)
        {
            return;
        }

        bool isTargetTag = other.CompareTag(targetTag) || opponent.gameObject.CompareTag(targetTag);
        if (!isTargetTag)
        {
            Debug.Log($"{gameObject.name} ชน {opponent.gameObject.name} แต่ Tag ไม่ตรงกับ targetTag ('{targetTag}')");
            return;
        }

        // 3. ตรวจสอบว่า Enemy หมดฉากหรือยัง
        GameObject remainingEnemy = GameObject.FindGameObjectWithTag("Enemy");
        if (remainingEnemy != null)
        {
            Debug.Log($"ยังกินกันเองไม่ได้! เหลือ Enemy: {remainingEnemy.name}");
            return;
        }

        // 4. เปรียบเทียบคะแนนเมื่อ Enemy หมดแล้ว
        Debug.Log($"พร้อมดวล! {gameObject.name} คะแนน={score} vs {opponent.gameObject.name} คะแนน={opponent.score}");

        if (score < opponent.score)
        {
            Debug.Log("คะแนนน้อยกว่าฝ่ายตรงข้าม! กินเขาไม่ได้");
            return;
        }
        else if (score == opponent.score)
        {
            Debug.Log("คะแนนเท่ากันเป๊ะ! ไม่มีใครโดนกิน");
            return;
        }

        // 5. คะแนนมากกว่า -> กินอีกฝ่ายและชนะตามฝั่งของตัวเอง
        hasWon = true;
        Debug.Log($"{gameObject.name} ({playerSide}) ชนะ!");

        Grow();
        Destroy(opponent.gameObject);

        if (gameUI != null)
        {
            if (playerSide == PlayerSide.Player1)
                gameUI.VictoryPlayer1();
            else
                gameUI.VictoryPlayer2();
        }
        else
        {
            Debug.LogError("ไม่พบ GameUIController!");
        }
    }

    void UpdateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    void Grow()
    {
        transform.localScale += new Vector3(growAmount, growAmount, 0f);
    }
}


