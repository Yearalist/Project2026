using UnityEngine;

/// <summary>
/// Polis sireni yanıp sönme efekti
/// 
/// KURULUM:
/// 1) Car altında boş obje oluştur → "SirenLight" adını ver
/// 2) SirenLight altına 2 Point Light ekle (biri kırmızı, biri mavi)
/// 3) Bu scripti SirenLight objesine ekle
/// 4) Inspector'dan redLight ve blueLight'ı ata
/// 5) PoliceCarEnemy'deki "Siren Light" alanına SirenLight objesini sürükle
/// </summary>
public class SirenFlash : MonoBehaviour
{
    [Header("Işıklar")]
    public Light redLight;
    public Light blueLight;

    [Header("Ayarlar")]
    public float flashSpeed = 4f;
    public float maxIntensity = 3f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime * flashSpeed;

        // Kırmızı ve mavi dönüşümlü yanıp söner
        float wave = Mathf.Sin(timer * Mathf.PI);

        if (redLight != null)
            redLight.intensity = Mathf.Max(0, wave) * maxIntensity;

        if (blueLight != null)
            blueLight.intensity = Mathf.Max(0, -wave) * maxIntensity;
    }
}
