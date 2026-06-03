using UnityEngine;
using TMPro;
using DG.Tweening;
using ToySiege.Core;

namespace ToySiege.UI
{
    /// <summary>
    /// Anahtar sayısını ekranda gösteren UI.
    /// LevelManager.OnKeyCollected event'ini dinler.
    /// </summary>
    public class KeyCountUI : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private TextMeshProUGUI _keyText;

        [Header("Animasyon")]
        [SerializeField] private float _punchScale = 0.3f;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _completeColor = new Color(0.3f, 1f, 0.4f);

        private Tweener _punch;
        private bool _subscribed;

        private void Start()
        {
            // Start'ta bağlan — OnEnable'da LevelManager henüz hazır olmayabilir
            Subscribe();

            if (_keyText != null)
            {
                _keyText.color = _normalColor;

                if (LevelManager.Instance != null)
                    _keyText.text = $"0 / {LevelManager.Instance.RequiredKeys}";
            }
        }

        private void Subscribe()
        {
            if (_subscribed) return;

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnKeyCollected += UpdateKeyCount;
                LevelManager.Instance.OnAllKeysCollected += ShowComplete;
                _subscribed = true;
            }
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnKeyCollected -= UpdateKeyCount;
                LevelManager.Instance.OnAllKeysCollected -= ShowComplete;
            }
            _subscribed = false;
        }

        private void OnDisable() => Unsubscribe();
        private void OnDestroy()
        {
            Unsubscribe();
            _punch?.Kill();
        }

        private void UpdateKeyCount(int current, int required)
        {
            if (_keyText == null) return;

            _keyText.text = $"{current} / {required}";

            _punch?.Kill();
            _keyText.transform.localScale = Vector3.one;
            _punch = _keyText.transform
                .DOPunchScale(Vector3.one * _punchScale, 0.3f, vibrato: 6)
                .SetUpdate(true);
        }

        private void ShowComplete()
        {
            if (_keyText != null)
                _keyText.color = _completeColor;
        }
    }
}
