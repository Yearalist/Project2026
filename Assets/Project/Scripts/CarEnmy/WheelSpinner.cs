using UnityEngine;

/// <summary>
/// Tekerlek döndürme scripti
/// Her tekerlek objesine ayrý ayrý ekle
/// 
/// NOT: Tekerleklerin pivot noktasý merkezde olmalý.
/// Eðer yanlýþ eksende dönüyorsa Inspector'dan rotationAxis'i deðiþtir.
/// </summary>
public class WheelSpinner : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    [Tooltip("Hangi eksende dönsün (çoðu araba modeli için X)")]
    public RotationAxis rotationAxis = RotationAxis.X;

    [Tooltip("Hýz çarpaný (tekerlek büyüklüðüne göre ayarla)")]
    public float spinMultiplier = 50f;

    private bool isMoving = false;
    private float currentSpeed = 0f;
    private float smoothSpeed = 0f;

    public void SetMoving(bool moving)
    {
        isMoving = moving;
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
    }

    void Update()
    {
        // Yumuþak geçiþ
        float targetSpeed = isMoving ? currentSpeed * spinMultiplier : 0f;
        smoothSpeed = Mathf.Lerp(smoothSpeed, targetSpeed, 8f * Time.deltaTime);

        if (smoothSpeed > 0.1f)
        {
            Vector3 axis = rotationAxis switch
            {
                RotationAxis.X => Vector3.right,
                RotationAxis.Y => Vector3.up,
                RotationAxis.Z => Vector3.forward,
                _ => Vector3.right
            };

            transform.Rotate(axis * smoothSpeed * Time.deltaTime, Space.Self);
        }
    }
}
