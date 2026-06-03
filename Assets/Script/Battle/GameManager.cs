using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private List<int> usedEncounterIndices = new List<int>();
    public static GameManager Instance { get; private set; }
    public int currentStage = 0; // 추가

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void GoToTitle()          
    {
        currentStage = 0;            
        SceneManager.LoadScene("TitleScene");
    }
    public void StartGame()
    {
        SceneManager.LoadScene("BattleScene");
    }

    public void OnVictory()
    {
        currentStage++; // 스테이지 증가
        Debug.Log($"전투 승리! 현재 스테이지: {currentStage}");
        SceneManager.LoadScene("ShopScene 1");
    }

    public void OnDefeat()
    {
        currentStage = 0; // 패배 시 초기화
        Debug.Log("전투 패배!");
        SceneManager.LoadScene("StartScene");
    }

    public void OnShopDone()
    {
        SceneManager.LoadScene("BattleScene test 3");
    }

    public float GetStageScaling()
    {
        return Mathf.Pow(1.1f, currentStage);
    }
    public static void DestroyInstance()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }
    public int GetRandomEncounterIndex(int totalCount)
    {
        // 모든 편성을 다 썼으면 초기화
        if (usedEncounterIndices.Count >= totalCount)
            usedEncounterIndices.Clear();

        int index;
        do
        {
            index = Random.Range(0, totalCount);
        } while (usedEncounterIndices.Contains(index));

        usedEncounterIndices.Add(index);
        return index;
    }
}