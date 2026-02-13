using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic; // 리스트 사용

public class KeypadUI : MonoBehaviour
{
    [Header("슬롯 연결")]
    public List<KeypadDigit> digitSlots; 
    
    [Header("패널 연결")]
    public GameObject panelRoot;

    private string correctPassword = "";
    private Action onSuccessCallback;
    private Action onFailureCallback;
    private Action onCancelCallback;

    public void OpenKeypad(string password, Action onUnlock, Action onFail = null, Action onCancel = null)
    {
        this.correctPassword = password;
        this.onSuccessCallback = onUnlock;
        this.onFailureCallback = onFail;
        this.onCancelCallback = onCancel;
        foreach (var slot in digitSlots)
        {
            slot.ResetDigit();
        }

        panelRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnEnterBtnClick()
    {
        string currentInput = "";
        foreach (var slot in digitSlots)
        {
            currentInput += slot.GetNumber().ToString();
        }

        if (currentInput == correctPassword)
        {
            Debug.Log("정답! (" + currentInput + ")");
            CloseKeypad();
            onSuccessCallback?.Invoke();
        }
        else
        {
            Debug.Log("땡! (" + currentInput + ")");
            if (onFailureCallback != null)
            {
                CloseKeypad();
                onFailureCallback?.Invoke();
            }
        }
    }

    public void OnCancelBtnClick()
    {
        onCancelCallback?.Invoke();
        CloseKeypad();
    }

    private void CloseKeypad()
    {
        panelRoot.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}