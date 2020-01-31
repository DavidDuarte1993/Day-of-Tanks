using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [SerializeField] private GameObject restartPanel;
    [SerializeField] private Text countdDownTxt;

    public static UIManager Instance { get; private set; }

    void Awake()
    {
        if (Instance)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        restartPanel.SetActive(false);
    }

    public void StartCountDown(int time)
    {
        StartCoroutine(CountingDown(time));
    }
    private IEnumerator CountingDown(int time)
    {
        restartPanel.SetActive(true);
        while (time > 0)
        {
            countdDownTxt.text = time.ToString();
            time -= 1;
            yield return new WaitForSeconds(1);
        }
        restartPanel.SetActive(false);
    }
}
