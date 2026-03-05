using Godot;
using System;
using System.Collections.Generic;

public static class SFX
{
    private static Dictionary<string, AudioStreamWav> _cache = new Dictionary<string, AudioStreamWav>();
    private static Random _rand = new Random();

    public static AudioStreamWav GetJumpSound()
    {
        if (_cache.ContainsKey("Jump")) return _cache["Jump"];
        // Tiếng nhảy bật lực: Cú Kick ở đầu + Tiếng lướt gió (Sweep freq)
        var stream = GenerateDSP(0.35f, (t) => {
            float env = Math.Max(0, 1.0f - (t / 0.35f));
            env = env * env; // Exponential decay - Mở nhanh, tắt mượt
            
            float freq = Mathf.Lerp(150f, 400f, t / 0.1f);
            if (t > 0.1f) freq = 400f;
            
            float osc = Mathf.Sin(t * freq * Mathf.Pi * 2f);
            float wind = (float)(_rand.NextDouble() * 2.0 - 1.0) * 0.15f * env;
            
            // Kick chân khi nhún
            float kick = t < 0.05f ? Mathf.Sin(t * 800f * Mathf.Pi * 2f) * (1.0f - t/0.05f) * 0.7f : 0f;

            return (osc * 0.5f + wind + kick) * env;
        });
        _cache["Jump"] = stream;
        return stream;
    }

    public static AudioStreamWav GetDoubleJumpSound()
    {
        if (_cache.ContainsKey("DoubleJump")) return _cache["DoubleJump"];
        // Nhảy đúp: Âm sắc kì ảo, tiếng chém gió chói kết hợp lực nén ma thuật
        var stream = GenerateDSP(0.4f, (t) => {
            float env = Math.Max(0, 1.0f - (t / 0.4f));
            
            float freq = Mathf.Lerp(500f, 1200f, t / 0.2f);
            float osc1 = Mathf.Sin(t * freq * Mathf.Pi * 2f);
            
            // Lớp sóng chói rít không khí xé gió
            float tPhase = (t * freq) % 1.0f;
            float osc2 = (tPhase * 2.0f - 1.0f) * 0.4f;
            
            float magicWind = (float)(_rand.NextDouble() * 2.0 - 1.0) * 0.3f;

            return (osc1 * 0.4f + osc2 + magicWind) * env;
        });
        _cache["DoubleJump"] = stream;
        return stream;
    }

    public static AudioStreamWav GetStepSound()
    {
        if (_cache.ContainsKey("Step")) return _cache["Step"];
        // Bước chân: Lạo xạo đất đá, sỏi ma sát cùng tiếng thụp của đế giày
        var stream = GenerateDSP(0.12f, (t) => {
            float env = Math.Max(0, 1.0f - (t / 0.12f));
            env = env * env * env; 
            
            // Âm thụp (Low hertz)
            float thump = Mathf.Sin(t * 80f * Mathf.Pi * 2f) * 0.7f;
            // Sỏi đá (Noise hertz)
            float dirt = (float)(_rand.NextDouble() * 2.0 - 1.0) * 0.5f;

            return (thump + dirt) * env;
        });
        _cache["Step"] = stream;
        return stream;
    }

    public static AudioStreamWav GetAttackSound(int comboNumber)
    {
        string key = "Attack" + comboNumber;
        if (_cache.ContainsKey(key)) return _cache[key];
        
        AudioStreamWav stream = null;

        if (comboNumber == 1) // Chém 1: Tiếng vũ khí rít qua không khí, bén, vút nhanh
        {
            stream = GenerateDSP(0.25f, (t) => {
                float env = t < 0.05f ? (t/0.05f) : Math.Max(0, 1.0f - ((t-0.05f) / 0.2f)); 
                
                // Kĩ thuật FM Synthesis tạo độ sắc bén kim khí
                float carrierFreq = Mathf.Lerp(900f, 300f, t / 0.25f);
                float modulator = Mathf.Sin(t * 1200f * Mathf.Pi * 2f) * 2.0f; 
                float osc = Mathf.Sin((t * carrierFreq + modulator) * Mathf.Pi * 2f);
                
                float windSword = (float)(_rand.NextDouble() * 2.0 - 1.0) * 0.5f; // Gió rách

                return (osc * 0.6f + windSword) * env;
            });
        }
        else if (comboNumber == 2) // Chém 2: Trọng lượng hơn, mài cắt kinh hồn hơn
        {
            stream = GenerateDSP(0.28f, (t) => {
                float env = t < 0.03f ? (t/0.03f) : Math.Max(0, 1.0f - ((t-0.03f) / 0.25f));
                
                float freq = Mathf.Lerp(1300f, 400f, t / 0.28f);
                float osc = 0;
                for(int i=1; i<=3; i++) {
                    osc += Mathf.Sin(t * freq * i * Mathf.Pi * 2f) / i; // Đa tần giả Răng cưa kim loại
                }
                
                float noise = (float)(_rand.NextDouble() * 2.0 - 1.0) * 0.6f;
                return (osc * 0.7f + noise) * env;
            });
        }
        else // Chém 3 CHÍ MẠNG: Đập vỡ không gian - Nổ rền vang, kim loại ngân dài, trầm siêu lực
        {
            stream = GenerateDSP(0.5f, (t) => {
                // Envelope Impact (Sức chứa lực lúc đập)
                float envImpact = t < 0.02f ? (t/0.02f) : Math.Max(0, 1.0f - ((t-0.02f) / 0.15f));
                // Envelope Ringing (Tiếng lưỡi dao ngân nga sau khi đập)
                float envRing = Math.Max(0, 1.0f - (t / 0.5f));
                envRing *= envRing;

                // Vụ nổ đập phá
                float subC = Mathf.Sin(t * Mathf.Lerp(250f, 30f, t / 0.15f) * Mathf.Pi * 2f); // Thả Bass
                float impactNoise = (float)(_rand.NextDouble() * 2.0 - 1.0);
                float impact = (subC * 0.8f + impactNoise * 0.7f) * envImpact;

                // Dải sóng Kim loại va chạm vang vọng
                float ring1 = Mathf.Sin(t * 850f * Mathf.Pi * 2f);
                float ring2 = Mathf.Sin(t * 1150f * Mathf.Pi * 2f);
                float ring3 = Mathf.Sin(t * 2200f * Mathf.Pi * 2f);
                float ring = (ring1 + ring2 + ring3) * 0.25f * envRing;

                return (impact + ring) * 0.9f;
            });
        }

        _cache[key] = stream;
        return stream;
    }

    // ─── DSP Core Engine: Phân Tích & Biến Đổi Âm Thanh Vòm ──────────
    private static AudioStreamWav GenerateDSP(float duration, Func<float, float> dspFunc)
    {
        int sampleRate = 44100; // Tần số Studio chuẩn 
        int totalSamples = (int)(duration * sampleRate);
        byte[] data = new byte[totalSamples * 2];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            float signal = dspFunc(t);

            // Soft-Clipping (Làm ấm âm thanh, không bị rát tai xé loa, mượt mà volume)
            signal = Mathf.Clamp(signal * 1.25f, -1f, 1f);

            short sample16 = (short)(signal * 32767f);
            data[i * 2] = (byte)(sample16 & 0xFF);
            data[i * 2 + 1] = (byte)((sample16 >> 8) & 0xFF);
        }

        var wav = new AudioStreamWav();
        wav.Data = data;
        wav.Format = AudioStreamWav.FormatEnum.Format16Bits;
        wav.MixRate = sampleRate;
        return wav;
    }
}
