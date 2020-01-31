using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public GameObject fadescreen;
    public float timer;
    public string SceneToLoad;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void LoadScene()
    {
        StartCoroutine(loadsceneCo());
    }
    IEnumerator loadsceneCo()
    {
       fadescreen.SetActive(true);
       yield return new WaitForSeconds(timer);
       SceneManager.LoadScene(SceneToLoad);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            LoadScene();
        }
    }
}
