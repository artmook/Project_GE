using UnityEngine;
using TMPro;

public class KeypadDigit : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI numberText;

    // 현재 이 슬롯의 숫자 (0~9)
    private int currentNumber = 0;

    void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// 숫자를 1 증가시킵니다. (9 -> 0)
    /// </summary>
    public void IncreaseNumber()
    {
        currentNumber++;
        if (currentNumber > 9) currentNumber = 0; // 루프
        UpdateUI();
    }

    /// <summary>
    /// 숫자를 1 감소시킵니다. (0 -> 9)
    /// </summary>
    public void DecreaseNumber()
    {
        currentNumber--;
        if (currentNumber < 0) currentNumber = 9; // 루프
        UpdateUI();
    }

    /// <summary>
    /// 현재 숫자를 반환합니다. (매니저가 가져갈 용도)
    /// </summary>
    public int GetNumber()
    {
        return currentNumber;
    }

    /// <summary>
    /// UI 초기화 (UI 열릴 때 호출)
    /// </summary>
    public void ResetDigit()
    {
        currentNumber = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        numberText.text = currentNumber.ToString();
    }
}