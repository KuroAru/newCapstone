# newCapstone AI 백엔드 서버 프로젝트

## 🚀 프로젝트 개요
- **목적**: 유니티(Unity) 게임 `newCapstone`의 AI 챗봇 기능을 위한 외부 접속 가능 서버.
- **배포 플랫폼**: AWS (Docker 기반, EC2 등)
- **AI 엔진**: 
    - **Primary**: Groq (Llama 3.1 8B) - `capstone` 환경변수 사용
    - **Fallback**: Gemini (1.5 Flash) - `GOOGLE_API_KEY` 환경변수 사용

## 🛠️ 기술 스택
- **Framework**: FastAPI (Python)
- **Deployment**: Docker, AWS (EC2 등)
- **Libraries**: `groq`, `google-generativeai`, `uvicorn`, `pydantic`

## 🔐 환경변수 설정 (AWS 인스턴스 또는 .env)
- `capstone`: Groq API 키
- `GOOGLE_API_KEY`: Google AI Studio API 키

## 📁 파일 구조 (`projects/newCapstone/backend_ai/`)
- `main.py`: 서버 메인 로직 (하이브리드 엔진 전환 및 API 엔드포인트)
- `Dockerfile`: Docker 이미지 설정 (AWS 등에서 사용)
- `requirements.txt`: 의존성 라이브러리 목록
- `render.yaml`: (참고용) 예전 Render 배포 청사진

## 🔗 접속 정보
- **Endpoint**: `POST /chat`
- **Payload**: `{"prompt": "유저 입력", "system": "시스템 프롬프트"}`
- **Response**: `{"response": "AI 답변"}`

---
*배포 플랫폼을 AWS로 전환 반영. Unity 클라이언트의 `BaseChatbot.localServerUrl`은 AWS 서버 주소로 맞춰 두면 됨.*
