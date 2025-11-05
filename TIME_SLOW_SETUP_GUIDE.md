# 시간 감속 시스템 설정 가이드

카타나 제로 스타일의 시간 감속 시스템이 구현되었습니다!

## 📋 구현된 기능

- ✅ Shift 키를 누르면 시간이 느려짐 (기본 20% 속도)
- ✅ 에너지 시스템으로 사용 시간 제한
- ✅ 에너지 자동 회복
- ✅ 플레이어는 정상 속도로 움직임 (적과 투사체만 느려짐)
- ✅ 에너지 바 UI (시각적 피드백)
- ✅ 부드러운 시간 전환 효과

## 🔧 Unity 설정 방법

### 1. TimeSlowManager 설정

**자동 생성됩니다!** `Managers` 시스템을 통해 자동으로 생성되므로 별도로 GameObject를 만들 필요가 없습니다.

TimeSlowManager 설정을 변경하려면:
1. 게임을 실행하면 "@Manager" 하위에 "TimeSlowManager"가 자동 생성됩니다
2. Play Mode에서 해당 GameObject를 선택하여 인스펙터에서 값 확인
3. 원하는 값으로 변경하고 싶다면, TimeSlowManager.cs의 SerializeField 기본값을 수정하세요

기본값 조정 (TimeSlowManager.cs):
   - **Slow Motion Scale**: 0.2 (시간 감속 정도, 낮을수록 더 느림)
   - **Normal Scale**: 1.0 (일반 속도)
   - **Transition Speed**: 5.0 (전환 부드러움)
   - **Max Energy**: 100 (최대 에너지)
   - **Energy Drain Rate**: 20 (초당 에너지 소모)
   - **Energy Regen Rate**: 10 (초당 에너지 회복)
   - **Energy Regen Delay**: 1.0 (회복 시작 전 대기 시간)

### 2. 플레이어 설정

1. Player GameObject 선택
2. `CharacterMovement` 컴포넌트에서:
   - **Ignore Time Scale**: ✅ 체크 (플레이어는 시간 감속 무시)
3. Player GameObject에 "Player" 태그가 있는지 확인

### 3. 에너지 UI 설정

#### Canvas 준비
1. Hierarchy에서 우클릭 → UI → Canvas
2. Canvas 설정:
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler 추가하여 해상도 대응

#### 에너지 바 만들기
1. Canvas 하위에 빈 GameObject 생성, 이름: "EnergyBar"
2. EnergyBar에 `EnergyBarUI.cs` 스크립트 추가

#### 에너지 바 UI 구조 만들기
```
Canvas
 └─ EnergyBar (EnergyBarUI 스크립트)
     ├─ Background (Image)
     ├─ Fill (Image - Fill Amount 타입)
     └─ Text (Text - 선택 사항)
```

#### 상세 설정

**Background 이미지:**
1. EnergyBar 하위에 UI → Image 생성, 이름: "Background"
2. 설정:
   - Color: 어두운 색 (예: 검정 또는 회색)
   - RectTransform: 원하는 위치와 크기 설정

**Fill 이미지:**
1. EnergyBar 하위에 UI → Image 생성, 이름: "Fill"
2. 설정:
   - Image Type: Filled
   - Fill Method: Horizontal (좌→우)
   - Fill Amount: 1.0
   - Color: 청록색 (#33CCFF)
3. RectTransform을 Background와 동일하게 설정

**Text (선택 사항):**
1. EnergyBar 하위에 UI → Text 생성
2. 중앙 정렬 설정

**EnergyBarUI 컴포넌트 연결:**
1. EnergyBar GameObject 선택
2. 인스펙터에서:
   - Energy Fill Image: Fill 드래그 앤 드롭
   - Energy Text: Text 드래그 앤 드롭 (선택 사항)
   - Full Color: 청록색 (#33CCFF)
   - Low Color: 빨간색 (#FF4444)
   - Low Energy Threshold: 0.3
   - Enable Pulse: ✅ 체크
   - Pulse Speed: 3.0

### 4. 적(Enemy) 설정

적 GameObject의 `CharacterMovement` 컴포넌트에서:
- **Ignore Time Scale**: ❌ 해제 (적은 시간 감속 영향을 받음)

## 🎮 사용 방법

### 게임 플레이
1. **Shift 키** (왼쪽 또는 오른쪽)를 누르고 있으면 시간 감속 활성화
2. 에너지가 있는 동안 시간이 느려짐
3. 에너지가 다 떨어지면 자동으로 일반 속도로 복귀
4. Shift를 떼면 에너지가 자동으로 회복됨

### 커스터마이징

#### 더 느리게 만들기
- `TimeSlowManager`의 **Slow Motion Scale**을 낮추기 (예: 0.1)

#### 더 오래 사용하기
- `TimeSlowManager`의 **Max Energy**를 높이기
- 또는 **Energy Drain Rate**를 낮추기

#### 빠른 회복
- `TimeSlowManager`의 **Energy Regen Rate**를 높이기
- 또는 **Energy Regen Delay**를 낮추기

## 🎨 시각 효과 추가 (선택 사항)

### 시간 감속 효과 강화
1. Post Processing 사용 (색수차, 모션 블러 등)
2. 파티클 시스템 속도 조정
3. 사운드 피치 변경 (TimeSlowManager의 Adjust Audio Pitch 활성화)

### UI 개선
1. 에너지 바에 그라데이션 효과 추가
2. 테두리 이미지 추가
3. 아이콘 추가 (시계, 번개 등)

## ⚙️ 고급 설정

### 스크립트에서 에너지 제어
```csharp
// 에너지 추가
Managers.TimeSlow.AddEnergy(50f);

// 에너지 설정
Managers.TimeSlow.SetEnergy(100f);

// 현재 에너지 확인
float currentEnergy = Managers.TimeSlow.CurrentEnergy;

// 슬로우 모션 강제 활성화/비활성화
Managers.TimeSlow.ActivateSlowMotion();
Managers.TimeSlow.DeactivateSlowMotion();
```

### 커스텀 입력키 변경
`TimeSlowManager.cs`의 `HandleTimeSlowInput()` 메서드에서 키 변경:
```csharp
if (Input.GetKey(KeyCode.Space))  // Shift → Space로 변경
{
    // ...
}
```

## 🐛 문제 해결

### 플레이어도 느려지는 경우
- Player의 `CharacterMovement`에서 **Ignore Time Scale** 체크 확인
- Player GameObject에 "Player" 태그가 있는지 확인

### UI가 표시되지 않는 경우
- `Managers.TimeSlow`가 null이 아닌지 확인 (Play Mode에서 자동 생성됨)
- EnergyBarUI의 Image/Text 참조가 올바른지 확인
- Canvas가 활성화되어 있는지 확인

### 시간이 복귀되지 않는 경우
- Play Mode를 종료하면 자동으로 Time.timeScale이 1로 복귀됨
- 에디터에서 직접 확인: Edit → Project Settings → Time → Time Scale

## 📝 참고사항

- 시간 감속은 Physics2D를 포함한 모든 Unity 시스템에 영향을 줍니다
- `Time.unscaledDeltaTime`을 사용하는 시스템은 영향을 받지 않습니다
- 플레이어의 움직임은 정상 속도로 유지되지만, 물리 연산은 여전히 영향을 받을 수 있습니다

## 🎯 추천 설정값

### 느린 감속 (전략적)
- Slow Motion Scale: 0.3
- Max Energy: 150
- Drain Rate: 15

### 빠른 감속 (액션)
- Slow Motion Scale: 0.15
- Max Energy: 80
- Drain Rate: 25
- Regen Rate: 15

### 밸런스 (기본)
- Slow Motion Scale: 0.2
- Max Energy: 100
- Drain Rate: 20
- Regen Rate: 10
