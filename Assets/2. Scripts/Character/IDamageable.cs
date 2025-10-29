using UnityEngine;

/// <summary>
/// 데미지를 받을 수 있는 모든 오브젝트가 구현해야 하는 인터페이스
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 데미지를 받았을 때 호출
    /// </summary>
    /// <param name="damage">받은 데미지</param>
    void TakeDamage(int damage);
}