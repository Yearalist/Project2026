using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using ToySiege.Player.Health;

namespace ToySiege.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private CanvasGroup _gameOverPanel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;

        [Header("Ayarlar")]
        [SerializeField] private float _fadeDelay = 2f;
        [SerializeField] private float _fadeDuration = 1f;

        private void Awake()
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.alpha = 0f;
                _gameOverPanel.gameObject.SetActive(false);
                _gameOverPanel.interactable = false;
                _gameOverPanel.blocksRaycasts = false;
            }

            if (_restartButton != null)
                _restartButton.onClick.AddListener(Restart);

            if (_quitButton != null)
                _quitButton.onClick.AddListener(Quit);
        }

        private void OnEnable()
        {
            if (_playerHealth != null)
                _playerHealth.OnDeath += ShowGameOver;
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
                _playerHealth.OnDeath -= ShowGameOver;
        }

        private void ShowGameOver()
        {
            if (_gameOverPanel == null) return;

            _gameOverPanel.gameObject.SetActive(true);

            // Yavaþ slow-motion
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.3f, 1f)
                .SetUpdate(true);

            // Panel fade in
            _gameOverPanel
                .DOFade(1f, _fadeDuration)
                .SetDelay(_fadeDelay)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _gameOverPanel.interactable = true;
                    _gameOverPanel.blocksRaycasts = true;

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                });
        }

        private void Restart()
        {
            Time.timeScale = 1f;
            DOTween.KillAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void Quit()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}