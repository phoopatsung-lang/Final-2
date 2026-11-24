using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public enum PlayerSide { Player1, Player2 }

    [Header("Player Side & UI")]
    public PlayerSide playerSide = PlayerSide.Player1; // Player 1 เลือก Player1, Player 2 เลือก Player2
    public GameUIController gameUI;

    [Header("Player Health")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Game UI (Optional)")]
    public GameObject gameOverPanel;

    [Header("Health Bar Images")]
    public Image healthBarImage;
    public Sprite[] healthSprites = new Sprite[4];

    private bool isGameOver = false;

    void Awake()
    {
        if (gameUI == null)
        {
            gameUI = FindFirstObjectByType<GameUIController>();
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        isGameOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateHealth();

        Debug.Log($"{gameObject.name} ({playerSide}) Health Started. HP = {currentHealth}");
    }

    // ตรวจจับการชนแบบ Trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        CheckObstacle(other.gameObject);
    }

    // ตรวจจับการชนแบบ Collider ปกติ
    void OnCollisionEnter2D(Collision2D collision)
    {
        CheckObstacle(collision.gameObject);
    }

    // ตรวจสอบวัตถุที่ชน
    void CheckObstacle(GameObject obj)
    {
        if (isGameOver) return;

        Transform target = obj.transform;
        bool foundObstacle = false;

        // ตรวจสอบ Tag ของวัตถุและวัตถุที่เป็น Parent
        while (target != null)
        {
            if (target.CompareTag("Obstacle"))
            {
                foundObstacle = true;
                break;
            }

            target = target.parent;
        }

        if (foundObstacle)
        {
            Debug.Log($"{gameObject.name} ({playerSide}) ชนสิ่งกีดขวาง/อุกกาบาต: {obj.name}");
            TakeDamage(1);
        }
    }

    // ลด HP
    public void TakeDamage(int damage)
    {
        if (isGameOver || damage <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"{gameObject.name} ({playerSide}) HP เหลือ: {currentHealth}");

        UpdateHealth();

        if (currentHealth <= 0)
        {
            OnPlayerDied();
        }
    }

    // เปลี่ยนรูปหัวใจตาม HP
    void UpdateHealth()
    {
        if (healthBarImage == null)
        {
            Debug.LogError("ยังไม่ได้ใส่ Health Bar Image");
            return;
        }

        if (healthSprites == null ||
            currentHealth >= healthSprites.Length ||
            healthSprites[currentHealth] == null)
        {
            Debug.LogError("กรุณาตรวจสอบ Health Sprites ใน Inspector");
            return;
        }

        healthBarImage.sprite = healthSprites[currentHealth];
    }

    // เมื่อผู้เล่นคนนี้ตาย -> อีกฝั่งชนะทันที
    void OnPlayerDied()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log($"{gameObject.name} ({playerSide}) ตาย! อีกฝั่งเป็นฝ่ายชนะ");

        if (gameUI != null)
        {
            if (playerSide == PlayerSide.Player1)
            {
                gameUI.VictoryPlayer2();
            }
            else
            {
                gameUI.VictoryPlayer1();
            }
        }
        else if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogError("ไม่พบ GameUIController และไม่ได้กำหนด GameOverPanel");
        }

        gameObject.SetActive(false);
    }

    // เริ่มเกมใหม่
    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}