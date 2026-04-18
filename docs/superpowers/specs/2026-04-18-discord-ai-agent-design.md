# Discord AI 게임개발 자동화 에이전트 설계

**날짜:** 2026-04-18  
**상태:** 승인됨

---

## 목적

Discord 봇으로 항상 실행 중인 Claude 에이전트를 만든다. Discord 채널에서 자연어로 명령을 보내면 게임 프로젝트 파일 분석, 코드 검색, Git 이력 조회, QA 시트 업데이트 등을 자동으로 처리한다.

---

## 전체 구조

```
Discord 메시지
      ↓
  Discord Bot (discord.py)
      ↓
  Agent Core (Claude Sonnet + Tool Use + Prompt Caching)
      ↓
  [도구 실행]
  read_file / list_files / search_code / run_git / update_sheet
      ↓
  결과 종합 → Discord 답변
```

---

## 파일 구조

```
ai-agent/
  bot.py           # Discord 봇 진입점 — 메시지 수신 및 에이전트 호출
  agent.py         # Claude Tool Use 에이전트 코어
  tools/
    file_tools.py  # read_file, list_files, search_code
    git_tools.py   # run_git
    sheet_tools.py # update_sheet (gws CLI 호출)
  config.py        # 환경변수 로드 및 경로 상수
  requirements.txt
  .env             # 시크릿 (gitignore)
```

---

## 도구 (Tool Use API)

| 도구 | 시그니처 | 설명 |
|------|---------|------|
| `read_file` | `(path: str)` | 게임 프로젝트 파일 내용 반환 |
| `list_files` | `(pattern: str)` | glob 패턴으로 파일 목록 반환 |
| `search_code` | `(query: str, file_type: str = "cs")` | 키워드 포함 파일·줄 반환 |
| `run_git` | `(command: str)` | git 읽기 명령 실행 결과 반환 |
| `update_sheet` | `(range: str, values: list)` | gws CLI로 QA 시트 업데이트 |

### 보안 규칙

- `run_git`: `log`, `diff`, `status`, `show` 만 허용. `push`, `reset`, `checkout` 등 차단
- `read_file`: `GAME_PROJECT_PATH` 하위 경로만 접근 허용
- Discord 봇: `ALLOWED_CHANNEL_IDS` 에 명시된 채널에서만 응답

---

## 기술 스택

| 항목 | 선택 |
|------|------|
| 언어 | Python 3.10+ |
| Discord | discord.py |
| AI | anthropic SDK (claude-sonnet-4-6) |
| Workspace | gws CLI (기설치) |
| 비용 최적화 | Prompt Caching — 시스템 프롬프트·프로젝트 구조를 캐시에 고정 |

---

## 환경 변수

```env
DISCORD_TOKEN=...
ANTHROPIC_API_KEY=...
GAME_PROJECT_PATH=C:\Users\user\Documents\GitHub\newCapstone\disputatio
QA_SHEET_ID=1o3kEoitP5-7Z5ncy9TxH_5FUQdB8Hy_--fB5u0nk8S8
ALLOWED_CHANNEL_IDS=채널ID1,채널ID2
```

---

## 사용 예시

```
나:  아들방 씬에서 문제 있는 스크립트 찾아줘
봇:  SealManager.cs Update()에서 매 프레임 FindObjectsByType 호출 3곳 발견.
     위치: line 49, 67, 83

나:  최근 커밋 10개 요약해줘
봇:  (git log 기반 요약)

나:  TC-050 버그 QA 시트에 추가해줘 — 인장 드래그 중 씬 전환 시 크래시
봇:  추가 완료 → https://docs.google.com/spreadsheets/d/...
```

---

## 범위 외 (이번 버전에서 제외)

- 파일 직접 수정 (쓰기 도구 없음 — 안전을 위해 읽기 전용)
- 멀티 에이전트 병렬 실행
- Google Workspace 이외 외부 API 연동
- 웹 UI
