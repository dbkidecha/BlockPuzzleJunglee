using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(ChangeScene), 3f);
    }

    private void ChangeScene()
    {
        SceneManager.LoadScene(1);
    }
}