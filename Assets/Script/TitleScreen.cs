using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameManager.Instance.StartGame();  // GameManager의 함수 호출
        }
    }
}