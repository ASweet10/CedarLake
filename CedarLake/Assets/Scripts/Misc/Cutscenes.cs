using UnityEngine;
using System.Collections;

public class Cutscenes : MonoBehaviour
{
    public IEnumerator HandleIntroCutscene() {
        //First cutscene; player driving down road & title card
        yield return null;
    }
    public IEnumerator HandleDriveToParkCutscene() {
        Debug.Log("drive to park");
        //fade to black
        //cutscene shows player driving to park
        //park car and see friend there
        yield return null;
    }
    public IEnumerator HandleEscapeCutscene() {
        //Fade to black
        yield return new WaitForSeconds(2.5f);
        //Fade in from black
        //Play music
        //Cutscene watching player drive away
        //Credits scroll down screen
    }
}
