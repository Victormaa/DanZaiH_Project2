using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get { return instance; } private set { } }
    private static GameManager instance;

    [Header("Fade Settings")]
    public float fadeDuration = 1f; 
    public ImageFader imageFader;
    private bool isTransitioning;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(SceneTransition(sceneName));
    }
    private IEnumerator SceneTransition(string sceneName)
    {
        isTransitioning = true;
        imageFader.Appear();
        yield return new WaitForSeconds(fadeDuration);

        // ÇÐ³¡¾°
        SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(0.5f);

        imageFader.Disappear();
        yield return new WaitForSeconds(fadeDuration);
        isTransitioning = false;
    }
}
