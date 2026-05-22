using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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
        SceneManager.LoadScene("ShopScene");
    }

    public void OnDefeat()
    {
        currentStage = 0; // 패배 시 초기화
        Debug.Log("전투 패배!");
    }

    public void OnShopDone()
    {
        SceneManager.LoadScene("BattleScene");
    }

    public float GetStageScaling()
    {
        return Mathf.Pow(1.1f, currentStage);
    }
}