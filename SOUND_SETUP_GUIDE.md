# 사운드 시스템 설정 가이드

SoundManager를 사용하여 배경음악(BGM)과 효과음(SFX)을 게임에 추가하는 방법입니다.

## 📋 구현된 기능

- ✅ Managers 패턴으로 사운드 관리
- ✅ BGM과 효과음 분리 재생
- ✅ Dictionary 기반 사운드 관리 (이름으로 재생)
- ✅ 볼륨 개별 조절 (BGM, SFX)
- ✅ 플레이어 액션에 사운드 자동 재생

## 🎵 설정 방법

### 1. 사운드 파일 준비

먼저 Unity 프로젝트에 사운드 파일을 추가하세요:

1. **Assets 폴더**에 "Sounds" 폴더 생성
2. 하위 폴더 생성:
   - `Sounds/BGM` - 배경음악
   - `Sounds/SFX` - 효과음
3. 오디오 파일(.mp3, .wav, .ogg)을 해당 폴더에 드래그 앤 드롭

### 2. SoundManager 설정

게임을 실행하면 `@Manager` 하위에 **SoundManager**가 자동으로 생성됩니다.

1. **Play Mode 진입**
2. Hierarchy → `@Manager` → `SoundManager` 선택
3. Inspector에서 사운드 등록

### 3. BGM 등록하기

**SoundManager** Inspector에서:

1. **BGM Settings** 섹션 찾기
2. **BGM Sounds** 배열 크기 설정 (예: 2)
3. 각 요소 설정:
   ```
   Element 0:
     Name: "메인BGM"
     Clip: [BGM 오디오 파일 드래그]
     Volume: 1.0

   Element 1:
     Name: "보스BGM"
     Clip: [보스 BGM 파일 드래그]
     Volume: 1.0
   ```
4. **BGM Volume**: 0.3 (전체 BGM 볼륨, 0~1)

### 4. 효과음 등록하기

**SoundManager** Inspector에서:

1. **SFX Settings** 섹션 찾기
2. **SFX Sounds** 배열 크기 설정 (예: 10)
3. 플레이어 액션에 필요한 사운드 등록:

```
Element 0:
  Name: "공격"
  Clip: [공격 사운드 파일]
  Volume: 0.8

Element 1:
  Name: "점프"
  Clip: [점프 사운드 파일]
  Volume: 0.6

Element 2:
  Name: "구르기"
  Clip: [구르기 사운드 파일]
  Volume: 0.7

Element 3:
  Name: "아이템픽업"
  Clip: [픽업 사운드 파일]
  Volume: 0.5

Element 4:
  Name: "아이템투척"
  Clip: [투척 사운드 파일]
  Volume: 0.7

Element 5:
  Name: "피격"
  Clip: [피격 사운드 파일]
  Volume: 0.9

Element 6:
  Name: "타격"
  Clip: [타격 사운드 파일]
  Volume: 0.8
```

4. **SFX Volume**: 0.7 (전체 효과음 볼륨, 0~1)

### 5. 설정 저장하기

Play Mode에서 설정한 값을 저장하려면:

1. Play Mode를 **종료하기 전에** SoundManager 컴포넌트를 복사
2. Play Mode 종료
3. `Assets/2. Scripts/Manager/SoundManager.cs`의 기본값 수정

   또는

4. **ScriptableObject로 사운드 데이터 관리** (고급)

> **중요:** Play Mode에서 설정한 값은 Play Mode 종료 시 사라집니다!

## 🎮 사용 방법

### BGM 재생

```csharp
// BGM 재생
Managers.Sound.PlayBGM("메인BGM");

// BGM 정지
Managers.Sound.StopBGM();

// BGM 일시정지
Managers.Sound.PauseBGM();

// BGM 재개
Managers.Sound.ResumeBGM();
```

### 효과음 재생

```csharp
// 기본 재생
Managers.Sound.Play("공격");

// 볼륨 조절하여 재생 (0.5배)
Managers.Sound.Play("공격", 0.5f);
```

### 볼륨 조절

```csharp
// BGM 볼륨 설정 (0~1)
Managers.Sound.SetBGMVolume(0.5f);

// 효과음 볼륨 설정 (0~1)
Managers.Sound.SetSFXVolume(0.8f);

// 현재 볼륨 가져오기
float bgmVol = Managers.Sound.GetBGMVolume();
float sfxVol = Managers.Sound.GetSFXVolume();
```

## 🎯 적용된 사운드

PlayerController에 이미 다음 사운드가 적용되어 있습니다:

| 액션 | 사운드 이름 | 위치 |
|------|-------------|------|
| 점프 | "점프" | PlayerController.cs:61 |
| 공격 | "공격" | PlayerController.cs:90 |
| 구르기 | "구르기" | PlayerController.cs:118 |
| 아이템 픽업 | "아이템픽업" | PlayerController.cs:153 |
| 아이템 투척 | "아이템투척" | PlayerController.cs:166 |
| 피격 | "피격" | PlayerController.cs:205 |

## 🔧 추가 사운드 적용하기

### CharacterCombat에 타격음 추가

```csharp
// CharacterCombat.cs의 OnSlashFrame() 메서드에 추가
public void OnSlashFrame()
{
    // 기존 코드...

    if (적을 타격했다면)
    {
        Managers.Sound.Play("타격");
    }
}
```

### Animation Event로 발소리 추가

1. **PlayerController.cs**에 메서드 추가:
```csharp
public void PlayFootstepSound()
{
    Managers.Sound.Play("발소리");
}
```

2. **Animation 창**에서:
   - 걷기/달리기 애니메이션 선택
   - 발이 땅에 닿는 프레임에 Animation Event 추가
   - Function: `PlayFootstepSound` 선택

### 적 사운드 추가

```csharp
// Enemy.cs에 추가
void Attack()
{
    Managers.Sound.Play("적공격");
    // 공격 로직...
}

void Die()
{
    Managers.Sound.Play("적사망");
    // 사망 로직...
}
```

## 💡 팁과 권장사항

### 사운드 이름 규칙

일관성 있는 이름을 사용하세요:
- **한글**: "공격", "점프", "피격" (현재 사용 중)
- **영어**: "Attack", "Jump", "Hit"
- **접두사**: "sfx_attack", "bgm_main"

### 볼륨 가이드

| 사운드 종류 | 권장 볼륨 |
|-------------|-----------|
| BGM | 0.2 ~ 0.4 |
| 공격/타격 | 0.7 ~ 0.9 |
| 이동/점프 | 0.4 ~ 0.6 |
| UI | 0.5 ~ 0.7 |
| 환경음 | 0.3 ~ 0.5 |

### 오디오 파일 형식

- **BGM**: .ogg (용량 작음, 루프 지원)
- **효과음**: .wav (품질 좋음, 짧은 사운드)
- **모바일**: .mp3 (호환성 좋음)

### 메모리 최적화

**Import Settings** (오디오 파일 선택 → Inspector):

**BGM:**
- Load Type: Streaming
- Compression Format: Vorbis
- Quality: 70%

**효과음:**
- Load Type: Decompress On Load
- Compression Format: ADPCM
- Quality: 100%

## 🐛 문제 해결

### 사운드가 재생되지 않음

1. **사운드 이름 확인:**
   ```csharp
   Managers.Sound.Play("공격"); // 이름이 정확한지 확인
   ```

2. **SoundManager 생성 확인:**
   - Play Mode에서 @Manager → SoundManager가 있는지 확인

3. **AudioClip 할당 확인:**
   - SoundManager Inspector에서 Clip 필드가 비어있지 않은지 확인

4. **볼륨 확인:**
   - BGM Volume / SFX Volume이 0이 아닌지 확인

### Warning: "효과음 'XXX'을 찾을 수 없습니다!"

- SoundManager의 SFX Sounds 배열에 해당 이름으로 등록했는지 확인
- 대소문자, 띄어쓰기 정확히 일치하는지 확인

### BGM이 반복되지 않음

- BGM용 AudioSource의 Loop가 자동으로 true로 설정되어 있습니다
- 문제 있다면 SoundManager.cs의 `InitializeAudioSources()` 확인

### Play Mode 종료 후 설정이 사라짐

**해결 방법 1: 기본값 수정**
```csharp
// SoundManager.cs 수정
[SerializeField][Range(0f, 1f)] private float bgmVolume = 0.3f;
[SerializeField][Range(0f, 1f)] private float sfxVolume = 0.7f;
```

**해결 방법 2: Prefab 사용**
1. Play Mode에서 SoundManager 설정
2. @Manager GameObject를 Prefab으로 저장
3. 씬에서 삭제 후 Prefab 배치

## 📝 무료 사운드 리소스

사운드 파일이 없다면:

- **Freesound**: https://freesound.org/
- **OpenGameArt**: https://opengameart.org/
- **Unity Asset Store**: Free Audio 검색
- **itch.io**: Free Game Assets → Audio

## 🎵 예제: 게임 시작 시 BGM 자동 재생

**GameManager.cs** 수정:

```csharp
void Start()
{
    Debug.Log("START GAME MANAGER");

    // BGM 자동 재생
    Managers.Sound.PlayBGM("메인BGM");
}
```

## 📋 체크리스트

사운드 시스템 설정이 완료되었는지 확인:

- [ ] Sounds 폴더에 오디오 파일 추가됨
- [ ] SoundManager에 BGM 등록됨
- [ ] SoundManager에 효과음 등록됨 (최소 6개)
- [ ] 게임 실행 시 점프 사운드 재생됨
- [ ] 공격 시 사운드 재생됨
- [ ] BGM이 반복 재생됨
- [ ] Console에 Warning이 없음

모두 완료되었다면 사운드 시스템 설정 완료입니다! 🎉
