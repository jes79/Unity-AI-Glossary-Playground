using UnityEngine;

namespace GlossaryDemo.ShaderDemo
{
    // 04_Shader 데모 공용 런타임 헬퍼
    public static class ShaderDemoUtil
    {
        // 빌더가 만든 머티리얼 "에셋"을 실행 중에 직접 바꾸면 플레이를 멈춰도 에셋 파일에 값이 남음
        // → 실행할 때 복사본을 하나 만들어 같은 머티리얼을 쓰던 렌더러들에 다시 넣어 줌 (공유 관계는 그대로 유지)
        public static Material CloneShared(Material source, params Renderer[] renderers)
        {
            if (source == null) return null;
            var clone = new Material(source) { name = source.name + " (Runtime)" };
            foreach (var r in renderers)
            {
                if (r == null) continue;
                var mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] != source) continue;
                    mats[i] = clone;
                    changed = true;
                }
                if (changed) r.sharedMaterials = mats;
            }
            return clone;
        }

        public static string OnOff(bool value) => value ? "<color=#8EF58E>ON</color>" : "<color=#FF8080>OFF</color>";

        // 라이트를 굽지 않은 씬에서도 스카이박스 기준 환경광/반사가 계산되도록 갱신
        public static void RefreshEnvironment() => DynamicGI.UpdateEnvironment();
    }
}
