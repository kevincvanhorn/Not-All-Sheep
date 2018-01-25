using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class UI_LoadScene : MonoBehaviour
{

    public void LoadByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}