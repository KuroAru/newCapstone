# newCapstone AI 백엔드 서버 프로젝트

## 프로젝트 개요

- **목적**: Unity 게임 `newCapstone`의 AI 챗봇용 HTTP API.
- **AI 엔진**: Groq(우선), Gemini(폴백).
- **프레임워크**: FastAPI, uvicorn.

## 새 PC / 클론 직후 필수 설정 (API 키)

API 키는 **Git에 올리지 않습니다**. 다른 컴퓨터에서는 반드시 아래를 한 번 실행하세요.

```bash
cd backend_ai
copy .env.example .env
   # macOS/Linux: cp .env.example .env
```

`.env` 파일을 열어 `GROQ_API_KEY`, `GOOGLE_API_KEY` 를 입력합니다.  
(레거시로 `capstone` 이름만 쓰는 경우도 `.env.example` 주석 참고.)

서버 실행:

```bash
pip install -r requirements.txt
uvicorn main:app --host 0.0.0.0 --port 8000
```

`config`는 **`backend_ai` 폴더의 `.env`** 를 자동으로 읽습니다 (작업 디렉터리와 무관).

## 운영·Docker·EC2

자세한 내용은 [DEPLOY.md](DEPLOY.md) 를 참고하세요.

## 환경 변수

| 변수 | 설명 |
|------|------|
| `GROQ_API_KEY` | Groq API 키 (권장) |
| `GOOGLE_API_KEY` | Google AI Studio (Gemini) 폴백 |
| `capstone` | 예전 Groq 키 변수명 (`GROQ_API_KEY` 가 비었을 때만 사용) |

## API

- **헬스**: `GET /`
- **채팅**: `POST /chat` — JSON `{ "prompt": "...", "system": "...", "use_tools": true }`
- **스트리밍**: `POST /chat/stream`

Unity 클라이언트의 `BaseChatbot.localServerUrl` 은 배포한 서버 주소로 맞춥니다.

## 파일 구조

- `main.py` — FastAPI 앱
- `config.py` — 설정·`.env` 로드
- `Dockerfile` — 컨테이너 빌드
- `.env.example` — 키 이름 템플릿 (커밋됨)
- `.env` — 실제 키 (커밋 금지, `.gitignore`)
