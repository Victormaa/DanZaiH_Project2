using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSceneSwitch : MonoBehaviour
{
    [Header("玩家的 Tag")]
    public string playerTag = "Player";

    [Header("切换的场景名称（可留空自动跳下一关）")]
    public string nextSceneName = "";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            Debug.Log("玩家进入区域，切换场景中...");

            // 如果未填写 nextSceneName，就自动加载 Build Index + 1
            if (string.IsNullOrEmpty(nextSceneName))
            {
                int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
