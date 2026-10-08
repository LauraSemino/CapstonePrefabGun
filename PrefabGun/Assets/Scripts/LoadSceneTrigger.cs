using UnityEngine;

public class LoadSceneTrigger : MonoBehaviour
{
    [SerializeField] int sceneToLoad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (SceneManagement.Instance != null && other.tag == "Player")
        {
            SceneManagement.Instance.LoadScene(sceneToLoad);
        }

    }
}
