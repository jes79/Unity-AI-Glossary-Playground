using UnityEngine;

namespace GlossaryDemo
{
    // 강의 모드에 나오는 '개발실' 팀원 캐릭터 (프로그래머·아트·기획·사운드·디렉터)
    // 그림: Resources/Lecture/{Key}_{상태}.png  (상태: rest 기본 / work 생각 중 / review 반짝 웃음 / ask 손 들고 질문)
    public readonly struct LecturePresenter
    {
        public readonly string Key;      // 파일 이름 앞부분: dev, art, plan, sound, director
        public readonly string Role;     // 이름표에 쓰는 직군
        public readonly string NextLine; // ① 소개 단계를 여는 한마디
        public readonly string DemoLine; // ③ 직접 보기 단계의 짧은 말풍선
        public readonly Color Accent;    // 이름표·진행 막대 색

        public LecturePresenter(string key, string role, string nextLine, string demoLine, Color accent)
        {
            Key = key;
            Role = role;
            NextLine = nextLine;
            DemoLine = demoLine;
            Accent = accent;
        }
    }

    public static class LecturePresenters
    {
        public static readonly LecturePresenter Dev = new LecturePresenter("dev", "프로그래머",
            "좋아요, 다음 용어 갑니다!",
            "직접 해 볼게요! 왼쪽 위 조작법대로 눌러 보세요.",
            new Color32(150, 130, 230, 255));

        public static readonly LecturePresenter Art = new LecturePresenter("art", "아트",
            "다음은 어떤 그림이 나올까요?",
            "눈으로 봐야 알아요! 켜고 끄면서 차이를 보세요.",
            new Color32(240, 130, 170, 255));

        public static readonly LecturePresenter Plan = new LecturePresenter("plan", "기획",
            "다음 시스템, 기획서 넘겨 볼게요.",
            "규칙대로 돌아가는지 직접 확인해 보세요.",
            new Color32(120, 200, 140, 255));

        public static readonly LecturePresenter Sound = new LecturePresenter("sound", "사운드",
            "다음 소리, 들어 볼까요?",
            "스피커 켜고 직접 들어 보세요!",
            new Color32(90, 190, 240, 255));

        public static readonly LecturePresenter Director = new LecturePresenter("director", "디렉터",
            "", "",
            new Color32(250, 190, 80, 255));

        // 분야 번호(1~11) → 담당자. 같은 담당자가 여러 분야를 맡는다
        private static readonly LecturePresenter[] ByCategory =
        {
            Dev,   // 01 캐릭터 이동·조작·카메라
            Dev,   // 02 물리·충돌
            Art,   // 03 애니메이션
            Art,   // 04 셰이더·머티리얼
            Art,   // 05 이펙트
            Art,   // 06 UI·UX
            Dev,   // 07 적 AI·길찾기
            Plan,  // 08 게임 시스템
            Sound, // 09 오디오
            Dev,   // 10 조명·렌더링·최적화
            Dev,   // 11 코드 구조·패턴·에디터
        };

        // 분야가 바뀔 때 담당자가 하는 소개: 회사에서 이 분야를 맡은 사람이 하는 일
        private static readonly string[] CategoryJobs =
        {
            "이 분야에서 회사 프로그래머는 키를 눌렀을 때 캐릭터가 얼마나 빨리 반응하는지, 점프가 얼마나 시원한지, 카메라가 어떻게 따라오는지를 숫자 하나하나 바꿔 가며 다듬습니다. 손맛을 만드는 일이죠.",
            "이 분야에서 회사 프로그래머는 물체가 부딪히고, 구르고, 떨어지는 규칙을 정하고, 총알이 벽을 뚫거나 캐릭터가 바닥에 끼는 버그를 잡습니다. 충돌과 레이어를 정리해서 '왜 안 맞았지?'를 없애는 일이에요.",
            "이 분야에서 회사 아트 팀은 걷기·뛰기·공격 동작을 만들고, 어떤 상황에서 어떤 동작으로 넘어갈지 순서도(Animator 상태)를 짭니다. 동작이 뚝뚝 끊기지 않고 자연스럽게 이어지게 하는 일이에요.",
            "이 분야에서 회사 아트 팀은 그리는 규칙(셰이더)과 그 설정값(머티리얼)으로 물체의 색·빛 반사·질감을 만들고, 원하는 느낌을 성능을 지키면서 화면에 올립니다.",
            "이 분야에서 회사 아트 팀은 불꽃, 폭발, 마법처럼 화면이 '번쩍'하는 순간을 작은 입자(파티클)와 화면 필터(포스트 프로세싱)로 만들어서, 공격 한 번에도 손맛이 느껴지게 합니다.",
            "이 분야에서 회사 아트 팀은 체력바, 메뉴, 팝업, 버튼을 배치하고, 플레이어가 화면을 보고 헤매지 않게, 어떤 해상도에서도 깨지지 않게 다듬습니다.",
            "이 분야에서 회사 프로그래머는 적이 순찰하다 플레이어를 발견하면 쫓아오고, 벽을 돌아 길을 찾는 규칙을 짭니다. 너무 똑똑해도, 너무 바보 같아도 재미가 없어서 조절이 중요해요.",
            "이 분야에서 회사 기획은 체력, 경험치, 인벤토리, 저장·불러오기처럼 게임을 오래 즐기게 하는 규칙과 숫자를 설계하고, 기획서로 정리해 프로그래머와 함께 만듭니다.",
            "이 분야에서 회사 사운드 담당은 발소리, 타격음, 배경 음악을 만들고 상황에 맞게 크기와 높낮이를 바꿔서, 눈을 감아도 무슨 일이 일어나는지 느껴지게 합니다.",
            "이 분야에서 회사 프로그래머는 조명과 그림자, 안개를 설정해 화면을 보기 좋게 만들고, 게임이 느려지면 원인을 찾아 그리는 횟수를 묶어 줄이고(배칭), 멀리 있는 건 대충 그리고(LOD), 만든 물건을 버리지 않고 다시 쓰는(풀링) 식으로 빠르게 만듭니다.",
            "이 분야에서 회사 프로그래머는 여러 사람이 같이 고쳐도 무너지지 않게 코드 구조와 패턴을 정하고, 팀이 쓰기 편한 에디터 도구를 만들고, 코드 리뷰로 품질을 지킵니다.",
        };

        public static LecturePresenter For(int category)
        {
            return ByCategory[Mathf.Clamp(category, 1, ByCategory.Length) - 1];
        }

        public static string CategoryJob(int category)
        {
            return CategoryJobs[Mathf.Clamp(category, 1, CategoryJobs.Length) - 1];
        }

        // 단계별 캐릭터 그림 (없으면 null → 직군 첫 글자 원으로 대체)
        public static Texture2D Sprite(LecturePresenter presenter, string state)
        {
            return Resources.Load<Texture2D>($"Lecture/{presenter.Key}_{state}");
        }
    }
}
