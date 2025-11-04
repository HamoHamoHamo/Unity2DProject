using UnityEngine;

/// <summary>
/// 플레이어를 부드럽게 따라다니며 지정된 범위를 벗어나지 않는 카메라
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // 따라갈 대상 (플레이어)

    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 0.125f; // 카메라 이동 부드러움 (낮을수록 부드러움)
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); // 카메라와 타겟 사이의 거리

    [Header("Mouse Influence")]
    [SerializeField] private bool useMouseOffset = true; // 마우스 오프셋 사용 여부
    [SerializeField] private float mouseInfluence = 0.3f; // 마우스 영향력 (0~1)
    [SerializeField] private float maxMouseOffset = 3f; // 마우스로 인한 최대 오프셋 거리

    [Header("Camera Bounds")]
    [SerializeField] private bool useBounds = true; // 범위 제한 사용 여부
    [SerializeField] private Vector2 minBounds = new Vector2(-10, -10); // 최소 범위 (좌하단)
    [SerializeField] private Vector2 maxBounds = new Vector2(10, 10); // 최대 범위 (우상단)

    private Camera cam;
    private Vector3 velocity = Vector3.zero; // SmoothDamp용 속도 변수

    void Awake()
    {
        cam = GetComponent<Camera>();

        // 타겟 자동 찾기
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;

        // 마우스 오프셋 추가
        if (useMouseOffset)
        {
            Vector3 mouseOffset = GetMouseOffset();
            desiredPosition += mouseOffset;
        }

        // 부드럽게 이동
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

        // 범위 제한 적용
        if (useBounds)
        {
            smoothedPosition = ClampCameraPosition(smoothedPosition);
        }

        transform.position = smoothedPosition;
    }

    /// <summary>
    /// 마우스 위치 기반 카메라 오프셋 계산
    /// </summary>
    private Vector3 GetMouseOffset()
    {
        // 마우스의 월드 좌표 계산
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // Z축은 무시

        // 플레이어에서 마우스로의 방향 벡터
        Vector3 directionToMouse = mouseWorldPos - target.position;

        // 오프셋 크기 제한
        Vector3 mouseOffset = directionToMouse * mouseInfluence;
        mouseOffset = Vector3.ClampMagnitude(mouseOffset, maxMouseOffset);

        // Z축은 0으로 유지
        mouseOffset.z = 0;

        return mouseOffset;
    }

    /// <summary>
    /// 카메라 위치를 지정된 범위 안으로 제한
    /// </summary>
    private Vector3 ClampCameraPosition(Vector3 position)
    {
        // 카메라의 화면 크기 계산 (Orthographic 기준)
        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;

        // 카메라가 범위를 벗어나지 않도록 X, Y 좌표 제한
        float clampedX = Mathf.Clamp(position.x, minBounds.x + cameraWidth, maxBounds.x - cameraWidth);
        float clampedY = Mathf.Clamp(position.y, minBounds.y + cameraHeight, maxBounds.y - cameraHeight);

        return new Vector3(clampedX, clampedY, position.z);
    }

    // Gizmos로 카메라 범위 시각화
    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;

        // 범위 테두리 그리기
        Gizmos.color = Color.green;
        Vector3 bottomLeft = new Vector3(minBounds.x, minBounds.y, 0);
        Vector3 bottomRight = new Vector3(maxBounds.x, minBounds.y, 0);
        Vector3 topLeft = new Vector3(minBounds.x, maxBounds.y, 0);
        Vector3 topRight = new Vector3(maxBounds.x, maxBounds.y, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}
