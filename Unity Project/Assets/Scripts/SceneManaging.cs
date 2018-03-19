using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManaging : MonoBehaviour
{

    public void LoadLevel()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void EndApplication()
    {
        Application.Quit();
    }
}
