using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Scene1";
    [SerializeField] private AudioClip confirmSfx;

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame) LoadScene();
        if (kb.escapeKey.wasPressedThisFrame) Application.Quit();   // only works in a built game
    }

    public void LoadScene()
    {
        SFX.Play(confirmSfx);
        SceneManager.LoadScene(sceneToLoad);
    }
}