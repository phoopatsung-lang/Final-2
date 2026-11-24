
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Player Settings")]
    public PlayerEat player1;
    public PlayerEat player2;

    [Header("Enemy Settings")]
    public int totalEnemies = 20;

    private int eatenEnemies = 0;

    [Header("UI Settings")]
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI enemyCountText;

    private bool gameEnded = false;

    void Start()
    {
        if (winnerText != null)
            winnerText.gameObject.SetActive(false);

        UpdateEnemyCount();
    }

    public void EnemyEaten()
    {
        if (gameEnded) return;

        eatenEnemies++;

        UpdateEnemyCount();

        Debug.Log("Enemy Eaten: " + eatenEnemies);

        if (eatenEnemies >= totalEnemies)
        {
            EndGame();
        }
    }

    void UpdateEnemyCount()
    {
        if (enemyCountText != null)
        {
            int remaining = Mathf.Max(
                0, totalEnemies - eatenEnemies
            );

            enemyCountText.text =
                "Enemy Remaining: " + remaining;
        }
    }

    void EndGame()
    {
        if (gameEnded) return;

        gameEnded = true;

        if (player1 == null || player2 == null)
        {
            Debug.LogError("กรุณากำหนด Player ทั้งสองตัว!");
            return;
        }

        if (winnerText == null)
        {
            Debug.LogError("ไม่พบ Winner Text!");
            return;
        }

        winnerText.gameObject.SetActive(true);

        if (player1.score > player2.score)
        {
            winnerText.text = "PLAYER 1 WINS!";
        }
        else if (player2.score > player1.score)
        {
            winnerText.text = "PLAYER 2 WINS!";
        }
        else
        {
            winnerText.text = "DRAW!";
        }

        Debug.Log(winnerText.text);
    }
}

