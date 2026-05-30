using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("카드 효과음 - 전준우")]
    public AudioClip cardDrawSound;   // 카드 뽑는 소리

    // 효과음 추가하실 때 이 스크립트에다 코드 추가하셔서 사용하시면 됩니다.

    //  ex) [Header("전투 효과음 - 이호세")]
    //      public AudioClip attackSound;

    [Header("상점 효과음 - 이호세")]
    public AudioClip shopEnterSound;    // 상점 입장 소리
    public AudioClip goldSpendSound;    // 골드 소비 소리



    private AudioSource audioSource;

    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayCardDraw()
    {
        if (cardDrawSound != null)
            audioSource.PlayOneShot(cardDrawSound);
    }
    public void PlayShopEnter()
    {
        if (shopEnterSound != null)
            audioSource.PlayOneShot(shopEnterSound);
    }

    public void PlayGoldSpend()
    {
        if (goldSpendSound != null)
            audioSource.PlayOneShot(goldSpendSound);
    }
}