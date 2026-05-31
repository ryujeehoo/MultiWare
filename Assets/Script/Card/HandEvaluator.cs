using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandEvaluator
{
    public static HandResult Evaluate(List<CardData> selectedCards)
    {
        Debug.Log($"Evaluate 받은 카드들: {string.Join(", ", selectedCards.Select(c => $"{c.cardName}({c.cardNumber})"))}");

        if (selectedCards == null || selectedCards.Count == 0)
            return new HandResult(HandRankType.HighCard,
                HandResult.CalculateDamage(HandRankType.HighCard, selectedCards),
                selectedCards);

        if (IsRoyalFlush(selectedCards))
            return new HandResult(HandRankType.RoyalFlush,
                HandResult.CalculateDamage(HandRankType.RoyalFlush, selectedCards),
                selectedCards);

        if (IsStraightFlush(selectedCards))
            return new HandResult(HandRankType.StraightFlush,
                HandResult.CalculateDamage(HandRankType.StraightFlush, selectedCards),
                selectedCards);

        if (IsFourOfAKind(selectedCards))
            return new HandResult(HandRankType.FourOfAKind,
                HandResult.CalculateDamage(HandRankType.FourOfAKind, selectedCards),
                selectedCards);

        if (IsFullHouse(selectedCards))
            return new HandResult(HandRankType.FullHouse,
                HandResult.CalculateDamage(HandRankType.FullHouse, selectedCards),
                selectedCards);

        if (IsFlush(selectedCards))
            return new HandResult(HandRankType.Flush,
                HandResult.CalculateDamage(HandRankType.Flush, selectedCards),
                selectedCards);

        if (IsStraight(selectedCards))
            return new HandResult(HandRankType.Straight,
                HandResult.CalculateDamage(HandRankType.Straight, selectedCards),
                selectedCards);

        if (IsThreeOfAKind(selectedCards))
            return new HandResult(HandRankType.ThreeOfAKind,
                HandResult.CalculateDamage(HandRankType.ThreeOfAKind, selectedCards),
                selectedCards);

        if (IsTwoPair(selectedCards))
            return new HandResult(HandRankType.TwoPair,
                HandResult.CalculateDamage(HandRankType.TwoPair, selectedCards),
                selectedCards);

        if (IsOnePair(selectedCards))
            return new HandResult(HandRankType.OnePair,
                HandResult.CalculateDamage(HandRankType.OnePair, selectedCards),
                selectedCards);

        return new HandResult(HandRankType.HighCard,
            HandResult.CalculateDamage(HandRankType.HighCard, selectedCards),
            selectedCards);
    }

    // 로얄 플러시: 같은 문양 10, J, Q, K, A (정확히 5장)
    private static bool IsRoyalFlush(List<CardData> cards)
    {
        if (cards.Count != 5) return false;
        if (!IsFlush(cards)) return false;
        var numbers = cards.Select(c => c.cardNumber).OrderBy(n => n).ToList();

        // A가 1로 저장된 경우
        bool royalWithAce1 = numbers.SequenceEqual(new List<int> { 1, 10, 11, 12, 13 });
        // A가 14로 저장된 경우
        bool royalWithAce14 = numbers.SequenceEqual(new List<int> { 10, 11, 12, 13, 14 });

        return royalWithAce1 || royalWithAce14;
    }

    // 스트레이트 플러시: 같은 문양 + 연속 숫자 (정확히 5장)
    private static bool IsStraightFlush(List<CardData> cards)
    {
        if (cards.Count != 5) return false;
        return IsFlush(cards) && IsStraight(cards);
    }

    // 포카드: 같은 숫자 4장
    private static bool IsFourOfAKind(List<CardData> cards)
    {
        if (cards.Count < 4) return false;
        return cards.GroupBy(c => c.cardNumber)
                    .Any(g => g.Count() >= 4);
    }

    // 풀하우스: 트리플 + 페어 (정확히 5장)
    private static bool IsFullHouse(List<CardData> cards)
    {
        if (cards.Count != 5) return false;
        var groups = cards.GroupBy(c => c.cardNumber).ToList();
        return groups.Any(g => g.Count() == 3) &&
               groups.Any(g => g.Count() == 2);
    }

    // 플러시: 같은 문양 (정확히 5장)
    private static bool IsFlush(List<CardData> cards)
    {
        if (cards.Count != 5) return false;
        return cards.All(c => c.suit == cards[0].suit);
    }

    // 스트레이트: 연속된 숫자 (정확히 5장)
    private static bool IsStraight(List<CardData> cards)
    {
        if (cards.Count != 5) return false;

        var numbers = cards.Select(c => c.cardNumber)
                          .OrderBy(n => n).ToList();

        Debug.Log($"스트레이트 체크 숫자들: {string.Join(", ", numbers)}");

        // 일반 스트레이트 체크
        bool normalStraight = true;
        for (int i = 1; i < numbers.Count; i++)
        {
            if (numbers[i] != numbers[i - 1] + 1)
            {
                normalStraight = false;
                break;
            }
        }
        if (normalStraight) return true;

        // A(14)를 1로 처리해서 A,2,3,4,5 스트레이트 체크
        var numbersWithAceLow = numbers
            .Select(n => n == 14 ? 1 : n)
            .OrderBy(n => n).ToList();

        Debug.Log($"A 낮은 스트레이트 체크: {string.Join(", ", numbersWithAceLow)}");

        for (int i = 1; i < numbersWithAceLow.Count; i++)
        {
            if (numbersWithAceLow[i] != numbersWithAceLow[i - 1] + 1)
                return false;
        }
        return true;
    }

    // 트리플: 같은 숫자 3장
    private static bool IsThreeOfAKind(List<CardData> cards)
    {
        if (cards.Count < 3) return false;
        return cards.GroupBy(c => c.cardNumber)
                    .Any(g => g.Count() >= 3);
    }

    // 투페어: 페어 2개
    private static bool IsTwoPair(List<CardData> cards)
    {
        if (cards.Count < 4) return false;
        return cards.GroupBy(c => c.cardNumber)
                    .Count(g => g.Count() >= 2) >= 2;
    }

    // 원페어: 같은 숫자 2장
    private static bool IsOnePair(List<CardData> cards)
    {
        if (cards.Count < 2) return false;
        return cards.GroupBy(c => c.cardNumber)
                    .Any(g => g.Count() >= 2);
    }
}