using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AudioDemo
{
    // 리듬 / 비트 동기화: AudioSettings.dspTime(오디오 하드웨어 시계)으로 BPM 120 박자 위치를 계산해 노트 생성·판정
    // - 음악은 PlayScheduled(dspStart) 로 정확한 시각에 시작, 메트로놈 틱도 박자마다 PlayScheduled 로 예약
    // - 노트는 '도착해야 할 박자 시각 - 현재 곡 위치' 로 위치를 계산 → 프레임이 끊겨도 박자에서 어긋나지 않음
    // 비교: Time.time 시계는 프레임이 크게 멈추면(L 키, 0.5초 렉) maximumDeltaTime 제한 때문에 음악보다 뒤처짐
    public class RhythmSyncDemo : MonoBehaviour
    {
        [Header("박자")]
        [SerializeField] private float bpm = 120f;
        [SerializeField] private bool[] pattern = { true, true, true, false, true, true, true, true };
        [SerializeField] private float spawnAheadBeats = 3f;

        [Header("오디오")]
        [SerializeField] private AudioSource music;
        [SerializeField] private AudioSource[] tickSources;   // 메트로놈 예약 재생용 (번갈아 사용)
        [SerializeField] private AudioClip tick, tickAccent;
        [SerializeField] private bool metronome = true;

        [Header("화면")]
        [SerializeField] private SpriteRenderer noteTemplate;
        [SerializeField] private Transform hitLine;
        [SerializeField] private float spawnX = 8f;
        [SerializeField] private Transform pulseDsp, pulseTime;
        [SerializeField] private SpriteRenderer judgeFlash;

        private class Note { public double time; public SpriteRenderer sr; public bool judged; }

        private readonly List<Note> notes = new List<Note>();
        private double dspStart;
        private float timeStart;
        private double nextTickDsp;
        private int nextTickBeat;
        private int nextSpawnBeat;
        private int tickIndex;
        private bool useDsp = true;
        private string judge = "-";
        private float judgeOffsetMs;
        private int perfect, great, good, miss;

        private double SecPerBeat => 60.0 / bpm;
        private double SongPosDsp => AudioSettings.dspTime - dspStart;     // 오디오 시계 기준 곡 위치
        private double SongPosTime => Time.time - timeStart;              // 게임 시계 기준 (비교용)
        private double SongPos => useDsp ? SongPosDsp : SongPosTime;

        private void Start()
        {
            noteTemplate.gameObject.SetActive(false);
            dspStart = AudioSettings.dspTime + 0.5;                          // 0.5초 뒤 정확히 시작하도록 예약
            timeStart = Time.time + 0.5f;
            music.PlayScheduled(dspStart);
            nextTickDsp = dspStart;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.T)) useDsp = !useDsp;
            if (DemoInput.KeyDown(Key.M)) metronome = !metronome;
            if (DemoInput.KeyDown(Key.L)) System.Threading.Thread.Sleep(500); // 일부러 0.5초 렉

            ScheduleTicks();
            SpawnNotes();
            MoveNotes();
            if (DemoInput.JumpPressed || DemoInput.LeftClick) Judge();

            // 박자마다 커졌다 줄어드는 원 두 개: 위 = dspTime 기준, 아래 = Time.time 기준
            Pulse(pulseDsp, SongPosDsp);
            Pulse(pulseTime, SongPosTime);
            if (judgeFlash != null) judgeFlash.color = new Color(judgeFlash.color.r, judgeFlash.color.g, judgeFlash.color.b, Mathf.MoveTowards(judgeFlash.color.a, 0f, Time.deltaTime * 3f));

            double beat = SongPosDsp / SecPerBeat;
            GlossaryInfoPanel.Status =
                $"BPM {bpm:0}  (1박 = {SecPerBeat:0.000}초)   박자 {System.Math.Max(0, (int)beat + 1)}   기준 시계: {(useDsp ? "dspTime" : "Time.time")}\n" +
                $"판정: {judge} ({judgeOffsetMs:+0;-0} ms)   P {perfect} / G {great} / g {good} / Miss {miss}\n" +
                $"시계 차이 (Time.time - dspTime) = {(SongPosTime - SongPosDsp) * 1000.0:+0;-0} ms";
        }

        // 다음 박자가 0.2초 안으로 다가오면 그 정확한 시각에 틱 소리를 예약
        private void ScheduleTicks()
        {
            while (AudioSettings.dspTime + 0.2 > nextTickDsp)
            {
                if (metronome && tickSources.Length > 0)
                {
                    var src = tickSources[tickIndex++ % tickSources.Length];
                    src.clip = nextTickBeat % 4 == 0 ? tickAccent : tick;
                    src.PlayScheduled(nextTickDsp);
                }
                nextTickBeat++;
                nextTickDsp = dspStart + nextTickBeat * SecPerBeat;
            }
        }

        private void SpawnNotes()
        {
            // 도착 박자보다 spawnAheadBeats 박 먼저 생성
            while (nextSpawnBeat * SecPerBeat - SongPos < spawnAheadBeats * SecPerBeat)
            {
                if (pattern[nextSpawnBeat % pattern.Length])
                {
                    var sr = Instantiate(noteTemplate, noteTemplate.transform.parent);
                    sr.gameObject.SetActive(true);
                    notes.Add(new Note { time = nextSpawnBeat * SecPerBeat, sr = sr });
                }
                nextSpawnBeat++;
            }
        }

        private void MoveNotes()
        {
            float speed = (spawnX - hitLine.position.x) / (float)(spawnAheadBeats * SecPerBeat);
            for (int i = notes.Count - 1; i >= 0; i--)
            {
                var n = notes[i];
                float dt = (float)(n.time - SongPos);         // 도착까지 남은 시간
                var p = n.sr.transform.position;
                n.sr.transform.position = new Vector3(hitLine.position.x + dt * speed, p.y, p.z);
                if (!n.judged && dt < -0.15f)
                {
                    n.judged = true;
                    miss++;
                    judge = "<color=#FF8080>Miss</color>";
                }
                if (dt < -0.6f)
                {
                    Destroy(n.sr.gameObject);
                    notes.RemoveAt(i);
                }
            }
        }

        private void Judge()
        {
            Note best = null;
            double bestDiff = double.MaxValue;
            foreach (var n in notes)
            {
                if (n.judged) continue;
                double d = System.Math.Abs(SongPos - n.time);
                if (d < bestDiff) { bestDiff = d; best = n; }
            }
            if (best == null || bestDiff > 0.2) return;

            best.judged = true;
            judgeOffsetMs = (float)((SongPos - best.time) * 1000.0);
            Color c;
            if (bestDiff <= 0.05) { perfect++; judge = "<color=#FFE066>Perfect</color>"; c = new Color(1f, 0.9f, 0.3f); }
            else if (bestDiff <= 0.1) { great++; judge = "<color=#9BE89B>Great</color>"; c = new Color(0.5f, 1f, 0.5f); }
            else { good++; judge = "<color=#9FD0FF>Good</color>"; c = new Color(0.6f, 0.8f, 1f); }
            best.sr.color = c;
            best.sr.transform.localScale *= 0.6f;
            if (judgeFlash != null) judgeFlash.color = new Color(c.r, c.g, c.b, 0.8f);
        }

        private void Pulse(Transform t, double songPos)
        {
            if (t == null) return;
            double beat = songPos / SecPerBeat;
            float frac = beat < 0 ? 1f : (float)(beat - System.Math.Floor(beat));
            float k = Mathf.Pow(1f - frac, 4f);
            t.localScale = Vector3.one * (0.6f + 0.5f * k);
        }
    }
}
