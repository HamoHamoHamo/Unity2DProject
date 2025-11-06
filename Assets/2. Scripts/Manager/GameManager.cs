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
        survivalTime = 0;
        killCount = 0;

        // 1. 씬 로드 전 리셋이 필요한 매니저들 호출
        Managers.Sound.StopAllSounds(); // 사운드 정지

        Managers.Spawn.ResetManager();

        Managers.Pool.ClearAllPoolsKey();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
