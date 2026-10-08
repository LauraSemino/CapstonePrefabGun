using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public static SceneManagement Instance { get { return _instance; } }
    private static SceneManagement _instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        _instance = this;
    }
    public void LoadScene(int scene)
    {
        //additional code for a fade in/fade out to make the loads less awkward
        SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
    }
}
