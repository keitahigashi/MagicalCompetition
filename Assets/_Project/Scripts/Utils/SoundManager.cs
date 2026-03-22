using UnityEngine;

namespace MagicalCompetition.Utils
{
    /// <summary>
    /// 効果音をコードで動的生成・再生するシングルトン。
    /// 外部音源ファイル不要。AudioClipはすべてランタイムで波形合成する。
    /// ファンタジー/魔法テーマの音色パレット。
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
            _clipCardDeselect = GenerateCardDeselect();
            _clipCardPlay     = GenerateCardPlay();
            _clipCardFlip     = GenerateCardFlip();
            _clipPass         = GeneratePassSound();
            _clipButtonClick  = GenerateButtonClick();
            _clipTurnStart    = GenerateTurnStart();
            _clipWin          = GenerateWinFanfare();
            _clipLose         = GenerateLoseSound();
            _clipDraw         = GenerateDrawSound();
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

        // ─── 波形合成ユーティリティ ─────────────────────────

        private static float Sin(float phase) => Mathf.Sin(2f * Mathf.PI * phase);

        /// <summary>エンベロープ: アタック→サスティン→ディケイ。</summary>
        private static float Envelope(float progress, float attack, float decayPower)
        {
            if (progress < attack)
                return progress / attack;
            float decayProgress = (progress - attack) / (1f - attack);
            return Mathf.Exp(-decayProgress * decayPower);
        }

        /// <summary>シンプルなリバーブ風テイル（遅延加算）。</summary>
        private static void AddReverbTail(float[] data, float delay, float decay)
        {
            int delaySamples = Mathf.CeilToInt(SampleRate * delay);
            for (int i = delaySamples; i < data.Length; i++)
                data[i] += data[i - delaySamples] * decay;
        }

        // ─── 個別サウンド生成 ────────────────────────────

        /// <summary>カード選択: クリスタルチャイム（D6→F#6、倍音豊かで澄んだ音）。</summary>
        private static AudioClip GenerateCardSelect()
        {
            float duration = 0.2f;
            int count = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[count];

            float[] freqs = { 1175f, 1480f }; // D6, F#6
            float splitTime = 0.07f;
            int splitSample = Mathf.CeilToInt(SampleRate * splitTime);

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                bool isSecond = i >= splitSample;
                float freq = isSecond ? freqs[1] : freqs[0];
                float localP = isSecond
                    ? (float)(i - splitSample) / (count - splitSample)
                    : (float)i / splitSample;

                // ベル風: 基音 + 非整数倍音でメタリックな響き
                float sample = Sin(freq * t) * 0.5f
                             + Sin(freq * 2.76f * t) * 0.2f
                             + Sin(freq * 5.4f * t) * 0.1f;

                float env = Envelope(localP, 0.01f, 6f);
                data[i] = sample * 0.3f * env;
            }

            AddReverbTail(data, 0.03f, 0.15f);

            var clip = AudioClip.Create("cardSelect", count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>カード選択解除: 柔らかな下降ベル（F#5→D5）。</summary>
        private static AudioClip GenerateCardDeselect()
        {
            float duration = 0.15f;
            int count = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[count];

            float[] freqs = { 740f, 587f }; // F#5, D5
            int splitSample = count / 2;

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                bool isSecond = i >= splitSample;
                float freq = isSecond ? freqs[1] : freqs[0];
                float localP = isSecond
                    ? (float)(i - splitSample) / (count - splitSample)
                    : (float)i / splitSample;

                float sample = Sin(freq * t) * 0.6f
                             + Sin(freq * 2.76f * t) * 0.15f;

                float env = Envelope(localP, 0.01f, 5f);
                data[i] = sample * 0.2f * env;
            }

            var clip = AudioClip.Create("cardDeselect", count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>カードプレイ: 魔法詠唱風アルペジオ（D5→F#5→A5→D6、キラキラ残響）。</summary>
        private static AudioClip GenerateCardPlay()
        {
            float noteLen = 0.07f;
            float[] notes = { 587f, 740f, 880f, 1175f }; // D5→F#5→A5→D6
            float tail = 0.25f;
            int totalSamples = Mathf.CeilToInt(SampleRate * (noteLen * notes.Length + tail));
            var data = new float[totalSamples];

            for (int n = 0; n < notes.Length; n++)
            {
                int startSample = Mathf.CeilToInt(SampleRate * noteLen * n);
                int noteSamples = Mathf.CeilToInt(SampleRate * (noteLen + tail));

                for (int i = 0; i < noteSamples && (startSample + i) < totalSamples; i++)
                {
                    float t = (float)i / SampleRate;
                    float progress = (float)i / noteSamples;

                    float env = Envelope(progress, 0.005f, 4f);

                    // ベル＋キラキラ倍音
                    float f = notes[n];
                    float sample = Sin(f * t) * 0.45f
                                 + Sin(f * 2f * t) * 0.2f
                                 + Sin(f * 3.17f * t) * 0.12f
                                 + Sin(f * 5.43f * t) * 0.06f;

                    // 音量クレシェンド
                    float vol = 0.7f + 0.3f * ((float)n / notes.Length);
                    data[startSample + i] += sample * 0.35f * env * vol;
                }
            }

            AddReverbTail(data, 0.04f, 0.2f);
            AddReverbTail(data, 0.09f, 0.1f);

            var clip = AudioClip.Create("cardPlay", totalSamples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>カードフリップ: 風切り音＋軽いベルヒット。</summary>
        private static AudioClip GenerateCardFlip()
        {
            float duration = 0.18f;
            int count = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / count;

                // ノイズベースのシュワ感（周波数変調で風切り）
                float noiseEnv = Mathf.Exp(-progress * 8f);
                float noise = (Mathf.PerlinNoise(t * 800f, 0f) * 2f - 1f) * 0.3f * noiseEnv;

                // ベルヒット（中間付近で鳴る）
                float bellT = Mathf.Max(0f, progress - 0.3f) / 0.7f;
                float bellEnv = bellT > 0f ? Mathf.Exp(-bellT * 10f) : 0f;
                float bell = Sin(880f * t) * 0.4f + Sin(880f * 2.76f * t) * 0.15f;

                data[i] = (noise + bell * bellEnv) * 0.3f;
            }

            var clip = AudioClip.Create("cardFlip", count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>パス: 低い共鳴音の下降（神秘的な見送り感）。</summary>
        private static AudioClip GeneratePassSound()
        {
            float duration = 0.3f;
            int count = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / count;

                // 下降ピッチ (A4→D4)
                float freq = Mathf.Lerp(440f, 294f, progress * progress);

                // 柔らかい三角波＋倍音
                float phase = 0f;
                // 積算周波数でピッチ変化に対応
                float sample = Sin(freq * t) * 0.5f;
                float tri = 2f * Mathf.Abs(2f * ((freq * t) - Mathf.Floor(freq * t + 0.5f))) - 1f;
                sample += tri * 0.3f;

                float env = Envelope(progress, 0.02f, 3f);
                data[i] = sample * 0.25f * env;
            }

            AddReverbTail(data, 0.05f, 0.15f);

            var clip = AudioClip.Create("pass", count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>ボタンクリック: 魔法の水晶タップ音。</summary>
        private static AudioClip GenerateButtonClick()
        {
            float duration = 0.1f;
            int count = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[count];

            float freq = 1200f;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / count;

                // 高めの音で「キン」とした水晶感
                float sample = Sin(freq * t) * 0.4f
                             + Sin(freq * 1.5f * t) * 0.2f
                             + Sin(freq * 3.07f * t) * 0.1f;

                // 非常に速い減衰
                float env = Mathf.Exp(-progress * 18f);
                if (t < 0.001f) env *= t / 0.001f;

                data[i] = sample * 0.25f * env;
            }

            var clip = AudioClip.Create("buttonClick", count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>ターン開始: 神秘的なベルチャイム（D5+A5、教会ベル風）。</summary>
        private static AudioClip GenerateTurnStart()
        {
            float duration = 0.35f;
            int count = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[count];

            float[] freqs = { 587f, 880f }; // D5, A5（完全五度）

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / count;
                float env = Envelope(progress, 0.005f, 3.5f);

                float sample = 0f;
                foreach (var f in freqs)
                {
                    // ベル倍音（非整数倍音でメタリック感）
                    sample += Sin(f * t) * 0.35f;
                    sample += Sin(f * 2.76f * t) * 0.12f;
                    sample += Sin(f * 5.4f * t) * 0.05f;
                }

                data[i] = sample * 0.25f * env;
            }

            AddReverbTail(data, 0.06f, 0.2f);

            var clip = AudioClip.Create("turnStart", count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>勝利: 壮大な魔法ファンファーレ（D→F#→A→D上昇、和音残響）。</summary>
        private static AudioClip GenerateWinFanfare()
        {
            float noteLen = 0.15f;
            float[] notes = { 587f, 740f, 880f, 1175f }; // D5→F#5→A5→D6
            float tail = 0.4f;
            int totalSamples = Mathf.CeilToInt(SampleRate * (noteLen * notes.Length + tail));
            var data = new float[totalSamples];

            for (int n = 0; n < notes.Length; n++)
            {
                int startSample = Mathf.CeilToInt(SampleRate * noteLen * n);
                int noteSamples = Mathf.CeilToInt(SampleRate * (noteLen + tail));
                bool isLast = n == notes.Length - 1;

                for (int i = 0; i < noteSamples && (startSample + i) < totalSamples; i++)
                {
                    float t = (float)i / SampleRate;
                    float progress = (float)i / noteSamples;

                    float env;
                    if (isLast)
                        env = Envelope(progress, 0.01f, 1.5f); // 最終音は長く残る
                    else
                        env = Envelope(progress, 0.005f, 4f);

                    float f = notes[n];
                    // 豊かな倍音で壮大さ
                    float sample = Sin(f * t) * 0.4f
                                 + Sin(f * 2f * t) * 0.2f
                                 + Sin(f * 3f * t) * 0.1f
                                 + Sin(f * 4f * t) * 0.05f;

                    // 5度のハーモニー追加（最終音）
                    if (isLast)
                        sample += Sin(f * 1.5f * t) * 0.15f;

                    float vol = 0.6f + 0.4f * ((float)n / notes.Length);
                    data[startSample + i] += sample * 0.35f * env * vol;
                }
            }

            AddReverbTail(data, 0.05f, 0.2f);
            AddReverbTail(data, 0.11f, 0.12f);

            var clip = AudioClip.Create("winFanfare", totalSamples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>敗北: 暗い下降和音（Dm→減衰）。</summary>
        private static AudioClip GenerateLoseSound()
        {
            float duration = 0.6f;
            int count = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[count];

            // D-F-A の短三和音、下降
            float[] baseFreqs = { 294f, 349f, 440f }; // D4, F4, A4

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / count;

                // ゆっくり下降するピッチ
                float pitchMult = 1f - progress * 0.15f;
                float env = Envelope(progress, 0.02f, 2f);

                float sample = 0f;
                foreach (var f in baseFreqs)
                {
                    float freq = f * pitchMult;
                    sample += Sin(freq * t) * 0.3f;
                    sample += Sin(freq * 2f * t) * 0.08f;
                }

                data[i] = sample * 0.3f * env;
            }

            AddReverbTail(data, 0.08f, 0.15f);

            var clip = AudioClip.Create("lose", count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>ドロー（カード補充）: 柔らかなページめくり風。</summary>
        private static AudioClip GenerateDrawSound()
        {
            float duration = 0.12f;
            int count = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / count;

                // 上昇する軽いトーン
                float freq = Mathf.Lerp(600f, 900f, progress);
                float sample = Sin(freq * t) * 0.3f;

                // ノイズ成分（紙っぽさ）
                float noise = (Mathf.PerlinNoise(t * 600f, 0.5f) * 2f - 1f) * 0.2f;

                float env = Envelope(progress, 0.005f, 8f);
                data[i] = (sample + noise) * 0.2f * env;
            }

            var clip = AudioClip.Create("draw", count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
