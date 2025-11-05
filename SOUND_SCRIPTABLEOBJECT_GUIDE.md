# ScriptableObject로 사운드 데이터 관리하기

SoundData ScriptableObject를 사용하여 사운드를 관리하는 방법입니다.

## 🎯 ScriptableObject를 사용하는 이유

### 기존 방식의 문제점:
- ❌ Play Mode에서 설정한 값이 종료 시 사라짐
- ❌ 매번 게임 실행 후 수동으로 설정해야 함
- ❌ 사운드 설정을 여러 씬에서 공유하기 어려움

### ScriptableObject의 장점:
- ✅ **영구적 저장**: 설정값이 에셋으로 저장되어 사라지지 않음
- ✅ **재사용성**: 여러 씬에서 같은 데이터 공유
- ✅ **편집 용이**: Project 창에서 바로 수정 가능
- ✅ **버전 관리**: Git으로 사운드 설정 관리 가능

## 📋 설정 방법

### 1단계: SoundData 에셋 생성

1. **Project 창**에서 원하는 폴더로 이동
   - 권장: `Assets/ScriptableObjects/Sound`

2. **우클릭** → **Create** → **ScriptableObjects** → **SoundData**

3. 파일 이름 설정 (예: "MainSoundData")

### 2단계: SoundData 설정

생성된 **MainSoundData** 파일을 선택하면 Inspector에 다음이 표시됩니다:

#### BGM Settings

1. **BGM Sounds** 배열 크기 설정 (예: 2)
2. 각 BGM 등록:
   ```
   Element 0:
     Name: "메인BGM"
     Clip: [메인 BGM 오디오 파일 드래그]
     Volume: 1.0

   Element 1:
     Name: "보스BGM"
     Clip: [보스 BGM 오디오 파일 드래그]
     Volume: 1.0
   ```

3. **Default BGM Volume**: 0.3 (전체 BGM 기본 볼륨)

#### SFX Settings

1. **SFX Sounds** 배열 크기 설정 (예: 10)
2. 각 효과음 등록:
   ```
   Element 0:
     Name: "공격"
     Clip: [공격 사운드]
     Volume: 0.8

   Element 1:
     Name: "점프"
     Clip: [점프 사운드]
     Volume: 0.6

   Element 2:
     Name: "구르기"
     Clip: [구르기 사운드]
     Volume: 0.7

   Element 3:
     Name: "아이템픽업"
     Clip: [픽업 사운드]
     Volume: 0.5

   Element 4:
     Name: "아이템투척"
     Clip: [투척 사운드]
     Volume: 0.7

   Element 5:
     Name: "피격"
     Clip: [피격 사운드]
     Volume: 0.9

   Element 6:
     Name: "타격"
     Clip: [타격 사운드]
     Volume: 0.8
   ```

3. **Default SFX Volume**: 0.7 (전체 효과음 기본 볼륨)

### 3단계: SoundManager에 연결

**방법 1: 게임 실행 후 연결 (임시)**

1. **Play Mode 진입**
2. Hierarchy → `@Manager` → `SoundManager` 선택
3. Inspector → **Sound Data** 필드에 MainSoundData 드래그

⚠️ 이 방법은 Play Mode 종료 시 연결이 해제됩니다!

**방법 2: Prefab 사용 (권장)**

1. **@Manager Prefab 생성:**
   - Play Mode에서 @Manager GameObject를 Project 창으로 드래그
   - Prefab으로 저장됨

2. **Prefab 수정:**
   - Project 창에서 @Manager Prefab 더블클릭
   - SoundManager → Sound Data에 MainSoundData 할당
   - Prefab 저장 (Ctrl+S)

3. **씬에 배치:**
   - Hierarchy에서 기존 @Manager 삭제
   - Prefab을 씬에 배치

**방법 3: SceneInitializer에서 자동 할당 (고급)**

```csharp
// SceneInitializer.cs 수정
public class SceneInitializer : MonoBehaviour
{
    [SerializeField] private SoundData soundData;

    void Awake()
    {
        // SoundManager 생성 및 SoundData 할당
        var soundManager = Managers.Sound;

        // Reflection으로 soundData 설정 (또는 public 메서드 추가)
        // ...
    }
}
```

## 🎮 완성된 구조

```
Assets/
  ├─ Sounds/
  │   ├─ BGM/
  │   │   ├─ main_bgm.ogg
  │   │   └─ boss_bgm.ogg
  │   └─ SFX/
  │       ├─ attack.wav
  │       ├─ jump.wav
  │       └─ ...
  │
  ├─ ScriptableObjects/
  │   └─ Sound/
  │       └─ MainSoundData.asset  ← ScriptableObject
  │
  └─ 2. Scripts/
      ├─ ScriptableObjects/
      │   └─ SoundData.cs
      └─ Manager/
          └─ SoundManager.cs
```

## 💡 사용 예시

### 여러 SoundData 만들기

**메인 씬용:**
- MainSoundData.asset
  - 메인 BGM, 기본 효과음

**보스 씬용:**
- BossSoundData.asset
  - 보스 BGM, 보스 전투 효과음

**메뉴 씬용:**
- MenuSoundData.asset
  - 메뉴 BGM, UI 효과음

각 씬에 맞는 SoundData를 설정하면 씬별로 다른 사운드 구성 가능!

### 코드에서 사용

```csharp
// 효과음 재생
Managers.Sound.Play("공격");

// BGM 재생
Managers.Sound.PlayBGM("메인BGM");
```

## 🔧 고급 기능

### SoundData 런타임에 변경하기

SoundManager에 public 메서드 추가:

```csharp
// SoundManager.cs에 추가
public void LoadSoundData(SoundData newSoundData)
{
    soundData = newSoundData;

    // Dictionary 초기화
    bgmDictionary.Clear();
    sfxDictionary.Clear();
    InitializeDictionaries();

    Debug.Log($"SoundData '{newSoundData.name}' 로드 완료!");
}
```

사용:
```csharp
// 보스 씬 진입 시
SoundData bossData = Resources.Load<SoundData>("BossSoundData");
Managers.Sound.LoadSoundData(bossData);
Managers.Sound.PlayBGM("보스BGM");
```

### 여러 개의 SFX AudioSource 사용

동시에 여러 효과음을 재생하려면:

```csharp
// SoundManager.cs 수정
[SerializeField] private int sfxSourceCount = 5;
private AudioSource[] sfxSources;

private void InitializeAudioSources()
{
    // ...

    // 여러 개의 SFX AudioSource 생성
    sfxSources = new AudioSource[sfxSourceCount];
    for (int i = 0; i < sfxSourceCount; i++)
    {
        sfxSources[i] = gameObject.AddComponent<AudioSource>();
        sfxSources[i].loop = false;
        sfxSources[i].volume = sfxVolume;
    }
}

public void Play(string soundName)
{
    // 재생 가능한 AudioSource 찾기
    foreach (var source in sfxSources)
    {
        if (!source.isPlaying)
        {
            source.PlayOneShot(clip, volume);
            return;
        }
    }

    // 모두 재생 중이면 첫 번째에 재생
    sfxSources[0].PlayOneShot(clip, volume);
}
```

## 📝 체크리스트

ScriptableObject 설정 완료 확인:

- [ ] SoundData.cs 스크립트 생성됨
- [ ] MainSoundData.asset 생성됨
- [ ] BGM 사운드 등록됨 (최소 1개)
- [ ] SFX 사운드 등록됨 (최소 6개: 점프, 공격, 구르기, 아이템픽업, 아이템투척, 피격)
- [ ] SoundManager에 SoundData 연결됨
- [ ] 게임 실행 시 사운드 재생됨
- [ ] Play Mode 종료 후에도 설정이 유지됨

모두 완료되었다면 ScriptableObject 설정 완료입니다! 🎉

## 🐛 문제 해결

### "SoundData가 할당되지 않았습니다!" 에러

**원인:** SoundManager에 SoundData가 연결되지 않음

**해결:**
1. Play Mode에서 @Manager → SoundManager 선택
2. Sound Data 필드에 MainSoundData.asset 드래그
3. 또는 Prefab에 미리 설정

### 사운드가 재생되지 않음

1. SoundData의 Name이 코드와 정확히 일치하는지 확인
2. AudioClip이 제대로 할당되었는지 확인
3. Volume이 0이 아닌지 확인

### Play Mode 종료 후 SoundData 연결 해제됨

- @Manager를 Prefab으로 만들어 사용하거나
- SceneInitializer에서 자동 할당 로직 추가

## 💼 실전 팁

### 사운드 이름 관리

SoundData에서 사용하는 이름을 상수로 관리:

```csharp
// SoundNames.cs
public static class SoundNames
{
    // BGM
    public const string BGM_MAIN = "메인BGM";
    public const string BGM_BOSS = "보스BGM";

    // SFX - Player
    public const string SFX_JUMP = "점프";
    public const string SFX_ATTACK = "공격";
    public const string SFX_DODGE = "구르기";

    // SFX - Item
    public const string SFX_PICKUP = "아이템픽업";
    public const string SFX_THROW = "아이템투척";

    // SFX - Combat
    public const string SFX_HIT = "타격";
    public const string SFX_DAMAGED = "피격";
}
```

사용:
```csharp
Managers.Sound.Play(SoundNames.SFX_JUMP);
Managers.Sound.PlayBGM(SoundNames.BGM_MAIN);
```

이렇게 하면 오타를 방지하고 자동 완성 기능을 활용할 수 있습니다!
