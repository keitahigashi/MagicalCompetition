using UnityEngine;

namespace MagicalCompetition.Utils
{
    /// <summary>
    /// 効果音をコードで動的生成・再生するシングルトン。
    /// 外部音源ファイル不要。AudioClipはすべてランタイムで波形合成する。
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        private const int SampleRate = 44100;

        private static SoundManager _instance;
        public static SoundManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("SoundManager");
                    _instance = go.AddComponent<SoundManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private AudioSource _sfxSource;

        // キャッシュ済みAudioClip
        private AudioClip _clipCardSelect;
        private AudioClip _clipCardDeselect;
        private AudioClip _clipCardPlay;
        private AudioClip _clipCardFlip;
        private AudioClip _clipPass;
        private AudioClip _clipButtonClick;
        private AudioClip _clipTurnStart;
        private AudioClip _clipWin;
        private AudioClip _clipLose;
        private AudioClip _clipDraw;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = 0.5f;

            GenerateAllClips();
        }

        private void GenerateAllClips()
        {
            _clipCardSelect   = GenerateCardSelect();
            _clipCardDeselect = GenerateTone(440f, 0.06f, ToneType.Sine, 0.3f);
            _clipCardPlay     = GenerateCardPlay();
            _clipCardFlip     = GenerateSweep(300f, 800f, 0.12f, 0.4f);
            _clipPass         = GeneratePassSound();
            _clipButtonClick  = GenerateButtonClick();
            _clipTurnStart    = GenerateChord(new[] { 440f, 554f }, 0.12f, 0.3f);
            _clipWin          = GenerateFanfare();
            _clipLose         = GenerateTone(220f, 0.4f, ToneType.Triangle, 0.4f, true);
            _clipDraw         = GenerateSweep(600f, 400f, 0.1f, 0.3f);
        }

        // ─── 公開API ─────────────────────────────────────

        public void PlayCardSelect()   => Play(_clipCardSelect);
        public void PlayCardDeselect() => Play(_clipCardDeselect);
        public void PlayCardPlay()     => Play(_clipCardPlay);
        public void PlayCardFlip()     => Play(_clipCardFlip);
        public void PlayPass()         => Play(_clipPass);
        public void PlayButtonClick()  => Play(_clipButtonClick);
        public void PlayTurnStart()    => Play(_clipTurnStart);
        public void PlayWin()          => Play(_clipWin);
        public void PlayLose()         => Play(_clipLose);
        public void PlayDraw()         => Play(_clipDraw);

        private void Play(AudioClip clip)
        {
            if (clip != null && _sfxSource != null)
                _sfxSource.PlayOneShot(clip);
        }

        // ─── 波形生成 ─────────────────────────────────────

        private enum ToneType { Sine, Square, Triangle }

        private static AudioClip GenerateTone(float freq, float duration, ToneType type,
            float volume, bool fadeDown = false)
        {
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            var clip = AudioClip.Create("tone", sampleCount, 1, SampleRate, false);
            var data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;
                float phase = t * freq;
                float sample;

                switch (type)
                {
                    case ToneType.Square:
                        sample = Mathf.Sin(2f * Mathf.PI * phase) >= 0 ? 1f : -1f;
                        break;
                    case ToneType.Triangle:
                        sample = 2f * Mathf.Abs(2f * (phase - Mathf.Floor(phase + 0.5f))) - 1f;
                        break;
                    default:
                        sample = Mathf.Sin(2f * Mathf.PI * phase);
                        break;
                }

                // エンベロープ（Attack-Decay）
                float env = 1f;
                float attackTime = 0.005f;
                float progress = (float)i / sampleCount;
                if (t < attackTime)
                    env = t / attackTime;
                else
                    env = 1f - (fadeDown ? progress * 0.7f : progress * progress);

                data[i] = sample * volume * Mathf.Max(env, 0f);
            }

            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GenerateChord(float[] freqs, float duration, float volume)
        {
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            var clip = AudioClip.Create("chord", sampleCount, 1, SampleRate, false);
            var data = new float[sampleCount];
            float ampPerVoice = volume / freqs.Length;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / sampleCount;
                float env = 1f - progress * progress;

                float sample = 0f;
                for (int f = 0; f < freqs.Length; f++)
                    sample += Mathf.Sin(2f * Mathf.PI * freqs[f] * t);

                data[i] = sample * ampPerVoice * Mathf.Max(env, 0f);
            }

            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GenerateSweep(float freqStart, float freqEnd, float duration, float volume)
        {
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            var clip = AudioClip.Create("sweep", sampleCount, 1, SampleRate, false);
            var data = new float[sampleCount];

            float phase = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / sampleCount;
                float freq = Mathf.Lerp(freqStart, freqEnd, progress);
                phase += freq / SampleRate;

                float env = 1f - progress * progress;
                data[i] = Mathf.Sin(2f * Mathf.PI * phase) * volume * Mathf.Max(env, 0f);
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>カード選択音（柔らかいポップ音、2音の上昇）。</summary>
        private static AudioClip GenerateCardSelect()
        {
            float duration = 0.12f;
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            var clip = AudioClip.Create("cardSelect", sampleCount, 1, SampleRate, false);
            var data = new float[sampleCount];

            // 2音: E5(523) → G5(659) 木琴風
            float freq1 = 523f, freq2 = 659f;
            float splitTime = 0.05f;
            int splitSample = Mathf.CeilToInt(SampleRate * splitTime);

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / sampleCount;
                bool isSecond = i >= splitSample;
                float freq = isSecond ? freq2 : freq1;
                float localProgress = isSecond
                    ? (float)(i - splitSample) / (sampleCount - splitSample)
                    : (float)i / splitSample;

                // 正弦波 + 軽い倍音（木琴風の減衰）
                float sample = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f
                             + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.2f
                             + Mathf.Sin(2f * Mathf.PI * freq * 3f * t) * 0.1f;

                // 各音の急速減衰エンベロープ
                float env = Mathf.Exp(-localProgress * 5f);
                data[i] = sample * 0.35f * env;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>カード出し音（華やかな上昇アルペジオ＋シュワ感）。</summary>
        private static AudioClip GenerateCardPlay()
        {
            float noteLen = 0.06f;
            float[] notes = { 392f, 523f, 659f, 784f }; // G4→C5→E5→G5
            float tailLen = 0.1f;
            int totalSamples = Mathf.CeilToInt(SampleRate * (noteLen * notes.Length + tailLen));
            var clip = AudioClip.Create("cardPlay", totalSamples, 1, SampleRate, false);
            var data = new float[totalSamples];

            for (int n = 0; n < notes.Length; n++)
            {
                int startSample = Mathf.CeilToInt(SampleRate * noteLen * n);
                int noteSamples = Mathf.CeilToInt(SampleRate * (noteLen + tailLen));

                for (int i = 0; i < noteSamples && (startSample + i) < totalSamples; i++)
                {
                    float t = (float)i / SampleRate;
                    float progress = (float)i / noteSamples;

                    // 急アタック＋自然な減衰
                    float env = Mathf.Exp(-progress * 6f);
                    if (t < 0.003f) env *= t / 0.003f;

                    // 基音＋倍音（キラキラ感）
                    float sample = Mathf.Sin(2f * Mathf.PI * notes[n] * t) * 0.6f
                                 + Mathf.Sin(2f * Mathf.PI * notes[n] * 2f * t) * 0.25f
                                 + Mathf.Sin(2f * Mathf.PI * notes[n] * 3f * t) * 0.15f;

                    // 後半の音ほど音量を少し上げる（クレシェンド感）
                    float volScale = 0.8f + 0.2f * ((float)n / notes.Length);
                    data[startSample + i] += sample * 0.4f * env * volScale;
                }
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>パス音（柔らかい2音の下降フレーズ）。</summary>
        private static AudioClip GeneratePassSound()
        {
            float duration = 0.2f;
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            var clip = AudioClip.Create("pass", sampleCount, 1, SampleRate, false);
            var data = new float[sampleCount];

            // E5(659) → C5(523) 下降2音
            float freq1 = 659f, freq2 = 523f;
            int splitSample = sampleCount / 2;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;
                bool isSecond = i >= splitSample;
                float freq = isSecond ? freq2 : freq1;
                float localProgress = isSecond
                    ? (float)(i - splitSample) / (sampleCount - splitSample)
                    : (float)i / splitSample;

                // Triangle波（柔らかい音色）＋軽い倍音
                float phase = t * freq;
                float tri = 2f * Mathf.Abs(2f * (phase - Mathf.Floor(phase + 0.5f))) - 1f;
                float sample = tri * 0.7f
                             + Mathf.Sin(2f * Mathf.PI * freq * t) * 0.3f;

                float env = Mathf.Exp(-localProgress * 4f);
                if (!isSecond && localProgress < 0.05f) env *= localProgress / 0.05f;
                if (isSecond && localProgress < 0.03f) env *= localProgress / 0.03f;

                data[i] = sample * 0.35f * env;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>ボタンクリック音（柔らかい打鍵音、倍音付き）。</summary>
        private static AudioClip GenerateButtonClick()
        {
            float duration = 0.08f;
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            var clip = AudioClip.Create("buttonClick", sampleCount, 1, SampleRate, false);
            var data = new float[sampleCount];

            float freq = 520f;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / sampleCount;

                // Sine基音 + 高次倍音で「コッ」という質感
                float sample = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f
                             + Mathf.Sin(2f * Mathf.PI * freq * 2.5f * t) * 0.25f
                             + Mathf.Sin(2f * Mathf.PI * freq * 4f * t) * 0.15f;

                // 瞬間アタック＋急速減衰
                float env = Mathf.Exp(-progress * 12f);
                if (t < 0.002f) env *= t / 0.002f;

                // 最後にノイズ成分を微量（打鍵感）
                float noise = (Random.value * 2f - 1f) * 0.1f * Mathf.Exp(-progress * 20f);

                data[i] = (sample * env + noise) * 0.3f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>勝利ファンファーレ（C-E-G-C上昇アルペジオ）。</summary>
        private static AudioClip GenerateFanfare()
        {
            float noteLen = 0.12f;
            float[] notes = { 523f, 659f, 784f, 1047f };
            int totalSamples = Mathf.CeilToInt(SampleRate * noteLen * notes.Length);
            var clip = AudioClip.Create("fanfare", totalSamples, 1, SampleRate, false);
            var data = new float[totalSamples];

            for (int n = 0; n < notes.Length; n++)
            {
                int startSample = Mathf.CeilToInt(SampleRate * noteLen * n);
                int noteSamples = Mathf.CeilToInt(SampleRate * noteLen);
                bool isLast = n == notes.Length - 1;

                for (int i = 0; i < noteSamples && (startSample + i) < totalSamples; i++)
                {
                    float t = (float)i / SampleRate;
                    float progress = (float)i / noteSamples;
                    float env;
                    if (isLast)
                        env = progress < 0.05f ? progress / 0.05f : 1f - progress * 0.3f;
                    else
                        env = progress < 0.05f ? progress / 0.05f : 1f - progress * progress;

                    float sample = Mathf.Sin(2f * Mathf.PI * notes[n] * t);
                    data[startSample + i] += sample * 0.45f * Mathf.Max(env, 0f);
                }
            }

            clip.SetData(data, 0);
            return clip;
        }
    }
}
