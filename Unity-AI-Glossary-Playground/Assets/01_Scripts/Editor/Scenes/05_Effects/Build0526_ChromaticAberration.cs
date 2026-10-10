using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0526_ChromaticAberration
{
    [GlossaryScene("0526")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0526_ChromaticAberration")]
    public static void Build()
    {
        var ctx = Stage("0526", "Space / 클릭 : 피격 → 0.2초 동안 Chromatic Aberration\nS : 작은 쉐이크 같이 ON/OFF\n슬라이더로 지속 시간을 바꿔 짧을 때와 길 때의 느낌을 비교",
            new Vector3(0f, 2f, -7f), new Vector3(5f, 0f, 0f));
        EnablePostProcessing(ctx.Camera);

        // 가장자리까지 대비가 강한 흰/검 기둥 (색 갈라짐이 잘 보임)
        for (int i = -7; i <= 7; i++)
        {
            bool white = i % 2 == 0;
            Prim(PrimitiveType.Cube, $"Pillar_{i}", new Vector3(i * 1.3f, 2f, 4f + Mathf.Abs(i) * 0.3f), new Vector3(0.5f, 4f, 0.5f),
                white ? new Color(0.9f, 0.9f, 0.9f) : new Color(0.08f, 0.08f, 0.1f));
        }
        Prim(PrimitiveType.Capsule, "Player", new Vector3(0f, 1f, 0f), Vector3.one, PlayerColor);

        var profile = Profile("Profile_0526_CA");
        var ca = AddOverride<ChromaticAberration>(profile);
        ca.intensity.Override(0f);
        SaveProfile(profile);
        var volume = GlobalVolume("Global Volume", profile);

        var shake = ctx.Camera.gameObject.AddComponent<EffectsCameraShake>();
        var demo = volume.gameObject.AddComponent<HitAberrationDemo>();
        Set(demo, "volume", volume);
        Set(demo, "cameraShake", shake);
        var b = Buttons(ctx, "피격!", "쉐이크 ON/OFF");
        OnClick(b[0], demo.Hit);
        OnClick(b[1], demo.ToggleShake);
        OnValueChanged(RightSlider(ctx, 0, "지속 시간 (초)", 0.05f, 1f, 0.2f), demo.SetDuration);
        Save(ctx);
    }
}
