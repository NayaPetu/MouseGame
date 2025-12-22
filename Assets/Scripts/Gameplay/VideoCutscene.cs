using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoCutscene : MonoBehaviour
{
    [SerializeField] private string nextSceneName; // Имя сцены, которая загрузится после окончания видео

    private VideoPlayer _videoPlayer;

    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
        if (_videoPlayer != null)
        {
            // Вызывается, когда видео доигрывает до конца
            _videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    private void OnDestroy()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }

    private void OnVideoEnd(VideoPlayer source)
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}


