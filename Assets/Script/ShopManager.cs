using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;

// Ollama API 통신을 위한 JSON 데이터 구조체
[System.Serializable]
public class OllamaRequest
{
    public string model;
    public string prompt;
    public bool stream;
}

[System.Serializable]
public class OllamaResponse
{
    public string response;
}

public class ShopManager : MonoBehaviour
{
    [Header("유물 풀")]
    public List<Relic> allRelics;

    [Header("상점 슬롯")]
    public List<RelicShopItem> shopSlots;

    [Header("가격 범위")]
    public int minPrice = 50;
    public int maxPrice = 150;

    [Header("AI 설정")]
    public string ollamaUrl = "http://localhost:11434/api/generate";
    public string modelName = "llama3"; // 사용 중인 모델 이름으로 유니티 인스펙터에서 수정

    [Header("고정 기능 슬롯")]
    public CardDeleteShopItem cardDeleteSlot;
    public int cardDeletePrice = 80;
    public HealShopItem healSlot;
    public int healAmount = 30;
    public int healPrice = 50;

    [Header("UI")]
    public Button continueButton;

    private List<Relic> recommendedRelics = new List<Relic>();

    void Start()
    {
        AudioManager.Instance.PlayShopEnter();

        // 팀원분이 추가하신 고정 슬롯 초기화 유지
        SetupHealSlot();
        if (cardDeleteSlot != null)
        {
            cardDeleteSlot.Setup(cardDeletePrice);
        }

        continueButton.onClick.AddListener(OnClickContinue);

        // 상점 유물 AI 추천 로직 시작 (비동기)
        StartCoroutine(GetAIRecommendation());
    }

    void SetupHealSlot()
    {
        if (healSlot != null)
        {
            healSlot.Setup(healAmount, healPrice);
        }
    }

    // AI에게 플레이어 정보를 보내고 추천을 받아오는 비동기 코루틴
    IEnumerator GetAIRecommendation()
    {
        Player p = Player.Instance;

        // 1. 플레이어 상태 문자열 변환
        string playerStatus = $"HP: {p.currentHealth}/{p.maxHealth}, Gold: {p.gold}, Defense: {p.ConsumeDefense()}, StatusEffects: ";
        if (p.activeEffects.Count > 0)
        {
            playerStatus += string.Join(", ", p.activeEffects.Select(e => e.effectName));
        }
        else
        {
            playerStatus += "None";
        }

        // 2. 플레이어 보유 유물 리스트화
        string playerRelics = p.relics.Count > 0
            ? string.Join("; ", p.relics.Select(r => $"{r.relicName}({r.description})"))
            : "None";

        // 3. 덱 정보 요약 (가장 중요한 부분: 무늬별 개수 파악)
        string deckSummary = "Unknown";
        if (DeckManager.Instance != null && DeckManager.Instance.allCards.Count > 0)
        {
            var cards = DeckManager.Instance.allCards;
            int totalCards = cards.Count;

            // 무늬(Suit)별로 카드가 몇 장 남았는지 묶어서 세기
            var suitGroups = cards.GroupBy(c => c.suit).Select(g => $"{g.Key}: {g.Count()}");
            string suitDistribution = string.Join(", ", suitGroups);

            deckSummary = $"Total {totalCards} cards remaining. Suit Distribution: [{suitDistribution}]";
        }

        // 4. 상점 후보 유물 리스트화 (효과 설명 포함)
        List<Relic> available = allRelics.FindAll(r => !p.relics.Contains(r));
        string availableRelics = string.Join("\n", available.Select(r => $"- {r.relicName}: {r.description}"));

        // 5. 프롬프트 엔지니어링: 중복 금지 및 포커 시너지 지시
        string promptText =
            $"You are an AI shopkeeper in a roguelike poker deck-building game.\n" +
            $"The player modifies their deck to easily form high-value poker hands (Flush, Full House, Straight, etc.).\n\n" +
            $"[Player Status]\n{playerStatus}\n\n" +
            $"[Current Deck Composition]\n{deckSummary}\n\n" +
            $"[Player's Current Relics]\n{playerRelics}\n\n" +
            $"[Available Relics in Shop]\n{availableRelics}\n\n" +
            $"Task: Based on the suit distribution in the deck and current relics, choose exactly {shopSlots.Count} UNIQUE relics from the 'Available Relics' list that maximize the player's synergy for poker hands.\n" +
            $"CRITICAL RULES:\n" +
            $"1. Output ONLY the EXACT names of the chosen relics, separated by a comma.\n" +
            $"2. DO NOT suggest the same relic more than once (NO DUPLICATES).\n" +
            $"3. Do not explain your reasoning. Do not add any extra text.";

        OllamaRequest requestData = new OllamaRequest
        {
            model = modelName,
            prompt = promptText,
            stream = false
        };
        string jsonRequest = JsonUtility.ToJson(requestData);

        // 6. 로컬 서버(Ollama) 호출
        UnityWebRequest request = new UnityWebRequest(ollamaUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequest);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 연결 대기 (게임은 멈추지 않음)
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("AI 통신 에러 발생. 기본 랜덤 로직으로 상점을 엽니다: " + request.error);
            GenerateRandomShop(available);
        }
        else
        {
            // [추가된 완벽 방어막: try-catch]
            try
            {
                OllamaResponse responseData = JsonUtility.FromJson<OllamaResponse>(request.downloadHandler.text);
                ParseAIResponseAndGenerateShop(responseData.response, available);
            }
            catch (System.Exception e)
            {
                Debug.LogError("AI 데이터 읽기 실패! 기본 랜덤 로직으로 대체합니다: " + e.Message);
                GenerateRandomShop(available);
            }
        }
    }

    // AI 결과를 분석해 슬롯에 진열하는 함수
    void ParseAIResponseAndGenerateShop(string aiResponse, List<Relic> availablePool)
    {
        Debug.Log("AI 추천 결과: " + aiResponse); // 콘솔창에서 결과 확인용

        string[] chosenNames = aiResponse.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        recommendedRelics.Clear();

        foreach (string name in chosenNames)
        {
            string cleanName = name.Trim();
            // 이름 매칭 (대소문자 무시, 포함 여부로 유연하게 검사)
            Relic foundRelic = availablePool.Find(r => r.relicName.ToLower().Contains(cleanName.ToLower()));

            // 코드 레벨에서도 한 번 더 중복 진열 방지 필터링
            if (foundRelic != null && !recommendedRelics.Contains(foundRelic))
            {
                recommendedRelics.Add(foundRelic);
            }
        }

        // AI가 개수를 덜 채웠거나 엉뚱한 대답을 했을 경우 빈자리는 랜덤으로 마저 채움
        while (recommendedRelics.Count < shopSlots.Count && availablePool.Count > recommendedRelics.Count)
        {
            List<Relic> remaining = availablePool.Except(recommendedRelics).ToList();
            Relic randomRelic = remaining[Random.Range(0, remaining.Count)];
            recommendedRelics.Add(randomRelic);
        }

        DisplayShop(recommendedRelics);
    }

    // 기존 랜덤 진열 로직 (통신 실패 시 작동하는 안전망)
    void GenerateRandomShop(List<Relic> available)
    {
        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        List<Relic> randomSelection = new List<Relic>();
        for (int i = 0; i < shopSlots.Count && i < available.Count; i++)
        {
            randomSelection.Add(available[i]);
        }

        DisplayShop(randomSelection);
    }

    // 실제 화면에 띄우는 역할
    void DisplayShop(List<Relic> itemsToDisplay)
    {
        for (int i = 0; i < shopSlots.Count; i++)
        {
            if (i < itemsToDisplay.Count)
            {
                int price = Random.Range(minPrice, maxPrice + 1);
                price = Mathf.RoundToInt(price / 10f) * 10;

                shopSlots[i].gameObject.SetActive(true);
                shopSlots[i].Setup(itemsToDisplay[i], price);
            }
            else
            {
                // 진열할 유물이 모자라면 슬롯 끄기
                shopSlots[i].gameObject.SetActive(false);
            }
        }
    }

    void OnClickContinue()
    {
        GameManager.Instance.OnShopDone();
    }
}