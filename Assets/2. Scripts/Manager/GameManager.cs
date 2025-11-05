using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private UIManager uiManager;

    private bool isGameRunning;
    private float survivalTime = 0f;
    private int killCount = 0;


    public void Initialize(UIManager ui)
    {
        uiManager = ui;
    }

    public void StartGame()
    {
        isGameRunning = true;
        uiManager.UpdateKills(0);
        uiManager.UpdateTime(0);

        uiManager.HideGameOverPanel();
    }

    private void Update()
    {
        if (isGameRunning)
        {
            survivalTime += Time.deltaTime;
            uiManager.UpdateTime(survivalTime);
        }
    }

    public void OnEnemyKilled()
    {
        killCount++;
        uiManager.UpdateKills(killCount);
    }

    public void GameOver()
    {
        isGameRunning = false;

        uiManager.ShowGameOverPanel();
    }

    public void RestartGame()
    {
        // 1. 씬 로드 전 리셋이 필요한 매니저들 호출
        if (Managers.Sound != null)
        {
            Managers.Sound.StopAllSounds(); // 사운드 정지
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
