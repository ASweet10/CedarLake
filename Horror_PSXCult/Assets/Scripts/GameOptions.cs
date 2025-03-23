using UnityEngine;
using TMPro;
using System.Collections;
using UnityEditor;

public class GameOptions : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown;
    string chosenWindow;
    public void GetWindowDropdownValue(){
        int windowIndex = dropdown.value;
        chosenWindow = dropdown.options[windowIndex].text;
        SetWindowType(chosenWindow);
        Debug.Log(chosenWindow);
    }

    void SetWindowType(string choice) {
        switch (choice) {
            case "Windowed":
                PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
                break;
            case "Fullscreen":
                PlayerSettings.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case "Borderless":
                PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
        }
    }
}
