# 에너지 바 UI 설정 가이드

EnergyBarUI를 사용하여 시간 감속 에너지를 화면에 표시하는 방법을 단계별로 설명합니다.

## 📋 UI 구조 개요

```
Canvas (화면 전체)
 └─ EnergyBarPanel (에너지 바 컨테이너)
     ├─ Background (배경 이미지 - 어두운 색)
     ├─ Fill (채워지는 이미지 - 청록색, Filled 타입)
     └─ EnergyText (에너지 숫자 표시 - 선택 사항)
```

## 🎨 단계별 UI 만들기

### 1단계: Canvas 만들기

1. **Hierarchy 창**에서 우클릭
2. **UI → Canvas** 선택
3. Canvas가 생성되면 다음 설정 확인:
   - **Render Mode**: Screen Space - Overlay
   - **Canvas Scaler** 컴포넌트가 없으면 추가:
     - Add Component → Canvas Scaler
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920 x 1080 (또는 원하는 해상도)

### 2단계: 에너지 바 패널 만들기

1. **Canvas**를 우클릭
2. **Create Empty** 선택
3. 이름을 **"EnergyBarPanel"**로 변경
4. **EnergyBarPanel** 선택 후 인스펙터에서:
   - **Add Component** → **EnergyBarUI** 스크립트 추가

5. **RectTransform** 설정 (화면 왼쪽 위에 배치):
   ```
   Anchors: Top-Left (왼쪽 위 모서리)
   Pivot: X: 0, Y: 1
   Pos X: 20
   Pos Y: -20
   Width: 200
   Height: 30
   ```

### 3단계: Background 이미지 만들기

1. **EnergyBarPanel**을 우클릭
2. **UI → Image** 선택
3. 이름을 **"Background"**로 변경
4. **Image 컴포넌트** 설정:
   - **Source Image**: None (또는 원하는 스프라이트)
   - **Color**: 검정색 또는 어두운 회색 (R:0.2, G:0.2, B:0.2, A:1)

5. **RectTransform** 설정 (패널 전체 채우기):
   ```
   Anchors: Stretch (양쪽 모두)
   Left: 0
   Right: 0
   Top: 0
   Bottom: 0
   ```

### 4단계: Fill 이미지 만들기 (중요!)

1. **EnergyBarPanel**을 우클릭
2. **UI → Image** 선택
3. 이름을 **"Fill"**로 변경
4. **Image 컴포넌트** 설정:
   - **Source Image**: None (또는 원하는 스프라이트)
   - **Color**: 청록색 (R:0.2, G:0.8, B:1, A:1) 또는 #33CCFF
   - ⭐ **Image Type**: **Filled** (매우 중요!)
   - **Fill Method**: Horizontal
   - **Fill Origin**: Left
   - **Fill Amount**: 1.0

5. **RectTransform** 설정 (Background와 동일하게):
   ```
   Anchors: Stretch (양쪽 모두)
   Left: 0
   Right: 0
   Top: 0
   Bottom: 0
   ```

> **중요:** Image Type을 "Filled"로 설정해야 에너지가 줄어들 때 바가 감소하는 애니메이션이 보입니다!

### 5단계: 텍스트 추가 (선택 사항)

숫자로 에너지를 표시하고 싶다면:

1. **EnergyBarPanel**을 우클릭
2. **UI → Text - TextMeshPro** 선택 (또는 일반 Text)
   - TMP Importer 창이 뜨면 "Import TMP Essentials" 클릭
3. 이름을 **"EnergyText"**로 변경
4. **TextMeshPro - Text (UI)** 설정:
   - **Text**: 100
   - **Font Size**: 20
   - **Alignment**: Center (가운데 정렬)
   - **Color**: 흰색

5. **RectTransform** 설정:
   ```
   Anchors: Stretch (양쪽 모두)
   Left: 0
   Right: 0
   Top: 0
   Bottom: 0
   ```

### 6단계: EnergyBarUI 컴포넌트 연결

1. **EnergyBarPanel** 선택
2. **Inspector** 창에서 **EnergyBarUI (Script)** 찾기
3. 참조 연결:
   - **Energy Fill Image**: Fill 오브젝트를 드래그 앤 드롭
   - **Energy Text**: EnergyText 오브젝트를 드래그 앤 드롭 (만들었다면)

4. 색상 설정:
   - **Full Color**: 청록색 (R:0.2, G:0.8, B:1, A:1)
   - **Low Color**: 빨간색 (R:1, G:0.3, B:0.3, A:1)
   - **Low Energy Threshold**: 0.3

5. 애니메이션 설정:
   - **Enable Pulse**: ✅ 체크
   - **Pulse Speed**: 3

## ✅ 완료!

이제 게임을 실행하면:
- Shift 키를 누르면 에너지 바가 줄어듭니다
- 에너지가 30% 이하가 되면 빨간색으로 변하고 깜빡입니다
- Shift를 떼면 에너지가 천천히 회복됩니다

## 🎨 커스터마이징 옵션

### 위치 변경

화면 오른쪽 위로 이동하려면:
```
EnergyBarPanel RectTransform:
  Anchors: Top-Right
  Pivot: X: 1, Y: 1
  Pos X: -20
  Pos Y: -20
```

화면 하단 가운데로 이동하려면:
```
EnergyBarPanel RectTransform:
  Anchors: Bottom-Center
  Pivot: X: 0.5, Y: 0
  Pos X: 0
  Pos Y: 20
```

### 크기 변경

더 큰 에너지 바:
```
Width: 300
Height: 40
```

더 작은 에너지 바:
```
Width: 150
Height: 20
```

### 색상 변경

**사이버펑크 스타일 (핑크/보라):**
- Full Color: R:1, G:0, B:1, A:1 (마젠타)
- Low Color: R:1, G:0.2, B:0.8, A:1 (핑크)

**레트로 게임 스타일 (초록/빨강):**
- Full Color: R:0, G:1, B:0, A:1 (초록)
- Low Color: R:1, G:0, B:0, A:1 (빨강)

### 테두리 추가

에너지 바에 테두리를 추가하려면:

1. **EnergyBarPanel** 우클릭 → UI → Image
2. 이름을 "Border"로 변경
3. Image Type: Sliced (Border 스프라이트 사용 시)
4. Color: 흰색 또는 밝은 색
5. RectTransform을 Background보다 약간 크게 설정

### 아이콘 추가

에너지 바 옆에 아이콘을 추가하려면:

1. **EnergyBarPanel** 우클릭 → UI → Image
2. 이름을 "Icon"로 변경
3. Source Image: 번개, 시계 등의 아이콘 스프라이트
4. RectTransform:
   ```
   Anchors: Left
   Width: 30
   Height: 30
   Pos X: -40 (에너지 바 왼쪽에 배치)
   ```

## 🔧 문제 해결

### 에너지 바가 보이지 않음
- Canvas가 활성화되어 있는지 확인
- Fill 이미지의 Color Alpha 값이 1인지 확인
- Camera에 UI Layer가 렌더링되는지 확인

### 에너지 바가 움직이지 않음
- Fill 이미지의 Image Type이 "Filled"인지 확인
- EnergyBarUI의 Energy Fill Image에 Fill이 연결되어 있는지 확인
- Managers.TimeSlow가 null이 아닌지 확인 (Play Mode 중)

### 에너지가 업데이트되지 않음
- TimeSlowManager가 자동으로 생성되었는지 확인 (@Manager 하위)
- Shift 키를 누르고 있는지 확인
- Console에 에러가 없는지 확인

## 💡 빠른 설정 (최소 구성)

시간이 없다면 이 최소 구성으로 시작하세요:

1. Canvas → EnergyBarPanel (Empty Object) → EnergyBarUI 스크립트 추가
2. EnergyBarPanel → Fill (Image, Type: Filled) 추가
3. EnergyBarUI에서 Fill 연결
4. 완료!

텍스트와 배경은 선택 사항입니다. Fill만 있어도 작동합니다.

## 📸 체크리스트

설정이 끝났다면 다음을 확인하세요:

- [ ] Canvas가 Hierarchy에 있음
- [ ] EnergyBarPanel에 EnergyBarUI 스크립트가 있음
- [ ] Fill 이미지의 Image Type이 "Filled"로 설정됨
- [ ] EnergyBarUI의 Energy Fill Image에 Fill이 연결됨
- [ ] 게임을 실행하면 에너지 바가 화면에 표시됨
- [ ] Shift를 누르면 에너지 바가 줄어듦
- [ ] Shift를 떼면 에너지가 회복됨

모두 체크되었다면 설정 완료입니다! 🎉
