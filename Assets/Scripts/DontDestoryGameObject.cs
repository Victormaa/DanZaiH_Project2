using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestoryGameObject : MonoBehaviour
{
    public static DontDestoryGameObject Instance { get { return _instance; } }
    private static DontDestoryGameObject _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
