# 실습 7 — 이벤트별 사운드

## 목표
발사·명중·물 착탄·격침을 소리로 구분하고 연속 효과음의 혼잡을 줄입니다.
Audio 폴더의 Shot/Hit/Splash/Sink.wav는 이 프로젝트에서 합성한 짧은 실습용 소리입니다.
기본 씬에서는 재생하지 않습니다. 직접 만든 소리로 교체해도 됩니다.

## 적용 절차
1. CombatAudioFeedback.cs를 아래 코드로 교체합니다.
2. Juice Lab에 AudioSource를 추가합니다.
3. Play On Awake=Off, Loop=Off, Spatial Blend=0(2D), Volume=1, Pitch=1.
4. 같은 오브젝트에 CombatAudioFeedback을 추가합니다.
5. Source에 같은 오브젝트의 AudioSource를 연결합니다.
6. Shot/Hit/Splash/Sink 필드에 Audio 폴더의 같은 이름 WAV를 연결합니다.
7. Volume=0.55, Minimum Interval=0.035초.
8. Main Camera에 AudioListener가 하나만 있는지 확인하고 씬을 저장합니다.

## 코드

```csharp
using UnityEngine;
namespace PirateGame.Juice
{
    public sealed class CombatAudioFeedback : MonoBehaviour
    {
        public AudioSource source;
        public AudioClip shot, hit, splash, sink;
        [Range(0,1)] public float volume = 0.55f;
        public float minimumInterval = 0.035f;
        CombatFeedbackEvents events;
        // Each sound family has its own limit; a shot never suppresses a sinking sound.
        readonly float[] last = {-100,-100,-100,-100};
        void OnEnable()
        {
            events = FindFirstObjectByType<CombatFeedbackEvents>();
            if (events)
            {
                events.ShotFired += OnShot; events.Hit += OnHit;
                events.WaterMiss += OnMiss; events.Sunk += OnSink;
            }
        }
        void Play(AudioClip clip,int channel)
        {
            if (!clip || !source || Time.unscaledTime-last[channel] < minimumInterval) return;
            last[channel] = Time.unscaledTime;
            source.pitch = Random.Range(0.95f,1.05f);
            source.PlayOneShot(clip,volume);
        }
        void OnShot(ShotFeedback value) => Play(shot,0);
        void OnHit(HitFeedback value) => Play(hit,1);
        void OnMiss(Vector2 value) => Play(splash,2);
        void OnSink(SinkFeedback value) => Play(sink,3);
        void OnDisable()
        {
            if (events)
            {
                events.ShotFired -= OnShot; events.Hit -= OnHit;
                events.WaterMiss -= OnMiss; events.Sunk -= OnSink;
            }
            if (source) source.Stop();
        }
    }
}
```

## 핵심
PlayOneShot은 기존 소리를 끊지 않고 겹쳐 재생합니다.
같은 종류 소리에는 최소 간격을 두고 다른 종류는 차단하지 않습니다.
따라서 명중 직후 격침 소리가 누락되지 않습니다.
Time.unscaledTime을 사용하여 Hit Stop 중에도 최소 간격 계산이 진행됩니다.
Time.timeScale만 바꾸는 Hit Stop은 AudioListener.pause를 바꾸지 않으므로 소리가 계속 재생됩니다.
AudioSource 한 개에서 Pitch를 바꾸면 이미 재생 중인 OneShot에도 영향을 줄 수 있습니다.
개별 보이스별 Pitch가 필요하면 AudioSource 풀을 쓰는 것이 확장 과제입니다.

## 완료 확인
발사·명중·빈 바다·격침 때 서로 다른 소리가 들리는지 확인합니다.
Play를 다시 시작하거나 재출항해도 소리 개수가 늘어나지 않아야 합니다.
소리가 없으면 Game 뷰의 Mute Audio, AudioListener 중복, Clip 참조를 확인합니다.
사운드를 끄고 다시 켜 명중을 알아채기 쉬워졌는지 비교합니다.
