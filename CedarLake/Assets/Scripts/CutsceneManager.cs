using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour {
    [SerializeField] PlayableDirector introDirector;
    [SerializeField] PlayableDirector campfireDirector;

    public void ToggleCutscene(string name, bool enabled) {
        switch (name) {
            case "Intro":
                if (enabled)
                    introDirector.Play();
                else
                    introDirector.Pause();
                break;
            case "Campfire":
                if (enabled)
                    campfireDirector.Play();
                else
                    campfireDirector.Pause();
                break;
        }
    }
}