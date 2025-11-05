using UnityEngine;

public class CharacterGlow : MonoBehaviour
{
    private SpriteRenderer characterRenderer;
    private Material materialInstance; // 셰이더 제어를 위한 고유 머티리얼 인스턴스

    void Awake()
    {
        // 렌더러 찾기
        characterRenderer = GetComponent<SpriteRenderer>();
        if (characterRenderer == null)
        {
            Debug.LogError("CharacterGlow: SpriteRenderer가 없습니다!", this);
            return;
        }

        // 중요: .material을 호출하여 고유한 인스턴스(복사본)를 생성합니다.
        // 이래야 이 캐릭터만 제어할 수 있습니다.
        materialInstance = characterRenderer.material;
    }

    void OnEnable()
    {
        // 오브젝트가 활성화될 때 (씬에 처음 배치되거나, 풀에서 나올 때)
        // 1. TimeSlowManager에 이 머티리얼 인스턴스 등록
        if (materialInstance != null && Managers.TimeSlow != null)
        {
            Managers.TimeSlow.RegisterCharacterMaterial(materialInstance);
        }
        else if (materialInstance == null)
        {
            Debug.LogWarning("CharacterGlow: 머티리얼 인스턴스가 없습니다.", this);
        }
    }

    void OnDisable()
    {
        // 오브젝트가 비활성화될 때 (파괴되거나, 풀에 반환될 때)
        // 2. TimeSlowManager에서 이 머티리얼 해제
        if (materialInstance != null && Managers.TimeSlow != null)
        {
            Managers.TimeSlow.UnregisterCharacterMaterial(materialInstance);
        }
    }
}