using UnityEngine;

public class LoadLevel : MonoBehaviour
{
    public int levelToLoad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void LoadLevelByIndex()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelToLoad);
    }
}
