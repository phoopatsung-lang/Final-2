using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIController : MonoBehaviour
{
    public enum PlayerSide { Player1, Player2 }

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject player1VictoryPanel;
    public GameObject player2VictoryPanel;

    [Header("Audio")]
    public AudioSource bgmSource;
    public AudioClip bgmClip;
    public AudioClip victoryClip;

    private bool gameEnded = false;

    void Start()
    {
        Time.timeScale = 0f;
        gameEnded = false;

        if (startPanel != null)
            startPanel.SetActive(true);

        if (player1VictoryPanel != null)
            player1VictoryPanel.SetActive(false);

        if (player2VictoryPanel != null)
            player2VictoryPanel.SetActive(false);

        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
        }
    }

    public void StartGame()
    {
        if (startPanel != null)
            startPanel.SetActive(false);

        if (bgmSource != null && bgmClip != null && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }

        Time.timeScale = 1f;
    }

    // เรียกเมื่อฝั่ง loserSide ตาย -> อีกฝั่งจะเป็นฝ่ายชนะทันที
    public void WinByOpponentDeath(PlayerSide loserSide)
    {
        if (gameEnded) return;

        if (loserSide == PlayerSide.Player1)
            VictoryPlayer2();
        else
            VictoryPlayer1();
    }

    public void VictoryPlayer1()
    {
        if (gameEnded) return;
        gameEnded = true;

        PlayVictorySound();

        if (player1VictoryPanel != null)
            player1VictoryPanel.SetActive(true);

        Time.timeScale = 0f;
        Debug.Log("PLAYER 1 ชนะ!");
    }

    public void VictoryPlayer2()
    {
        if (gameEnded) return;
        gameEnded = true;

        PlayVictorySound();

        if (player2VictoryPanel != null)
            player2VictoryPanel.SetActive(true);

        Time.timeScale = 0f;
        Debug.Log("PLAYER 2 ชนะ!");
    }

    void PlayVictorySound()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
            if (victoryClip != null)
            {
                bgmSource.PlayOneShot(victoryClip);
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}