using UnityEngine;
using UnityEngine.Rendering;

public sealed class ScreenFade : MonoBehaviour
{

    public const float FADE_SPEED = 1.7f;
    public const string FADE_PARAMETER_NAME = "_Fade";

    private static int _fadedTimes = 0;
    private static float _fadeInTime = float.NegativeInfinity;

    public static bool IsFadedOut => _fadedTimes > 0 || Time.time < _fadeInTime;

    public static void FadeOutFor(float duration)
    {
        float fadeInTime = Time.time + duration;
        if (fadeInTime > _fadeInTime)
            _fadeInTime = fadeInTime;
    }

    public static void FadeOut()
    {
        _fadedTimes++;
    }

    public static void FadeIn()
    {
        _fadedTimes--;
    }

    [SerializeField] private Material _fadeMaterial;
    private float _currentFade = 0f;

    private void Start()
    {
        _currentFade = IsFadedOut ? 0f : 1f;
    }

    private void OnDestroy()
    {
        _fadedTimes = 0;
        _fadeInTime = float.NegativeInfinity;
        _fadeMaterial.SetFloat(FADE_PARAMETER_NAME, 0f);
    }

    private void Update()
    {
        if (IsFadedOut == true)
        {
            if (_currentFade < 1f)
                _currentFade += Time.deltaTime * FADE_SPEED;
        }
        else
        {
            if (_currentFade > 0f)
                _currentFade -= Time.deltaTime * FADE_SPEED;
        }

        _fadeMaterial.SetFloat(FADE_PARAMETER_NAME, _currentFade);
    }

}
