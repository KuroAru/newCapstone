from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import os
import google.generativeai as genai
from groq import Groq
from dotenv import load_dotenv
import logging

# 로깅 설정
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

load_dotenv()

app = FastAPI()

# API 키 설정
GROQ_API_KEY = os.getenv("capstone")  # 'capstone'이라는 이름의 환경변수 사용
GOOGLE_API_KEY = os.getenv("GOOGLE_API_KEY")

# Groq 클라이언트 초기화 (Primary)
groq_client = None
if GROQ_API_KEY:
    groq_client = Groq(api_key=GROQ_API_KEY)
    logger.info("✅ Groq API Key 로드 완료 (Primary 엔진: capstone)")
else:
    logger.warning("⚠️ 'capstone' 환경변수를 찾을 수 없습니다. Groq 엔진이 비활성화됩니다.")

# Gemini 설정 (Fallback)
if GOOGLE_API_KEY:
    genai.configure(api_key=GOOGLE_API_KEY)
    logger.info("✅ Google API Key 로드 완료 (Fallback 엔진)")

class ChatRequest(BaseModel):
    prompt: str
    system: str = "You are a helpful assistant."

@app.get("/")
def read_root():
    return {"status": "online", "message": "newCapstone AI Server is Running!"}

@app.post("/chat")
async def chat(request: ChatRequest):
    # 1순위: Groq (Llama 3 70B) - 빠른 응답 속도
    if groq_client:
        try:
            logger.info("🚀 Groq (Llama 3) 엔진 호출 중...")
            completion = groq_client.chat.completions.create(
                model="llama3-70b-8192",
                messages=[
                    {"role": "system", "content": request.system},
                    {"role": "user", "content": request.prompt}
                ],
                temperature=0.7,
                max_tokens=1024,
            )
            return {"response": completion.choices[0].message.content}
        except Exception as e:
            logger.error(f"❌ Groq 오류 발생: {str(e)}")

    # 2순위: Gemini (Fallback)
    if GOOGLE_API_KEY:
        try:
            logger.info("🔄 Gemini 엔진으로 전환 중...")
            model = genai.GenerativeModel('gemini-1.5-flash-latest')
            full_prompt = f"System: {request.system}\nUser: {request.prompt}"
            response = model.generate_content(full_prompt)
            return {"response": response.text}
        except Exception as e:
            logger.error(f"❌ Gemini 오류 발생: {str(e)}")
            raise HTTPException(status_code=500, detail="모든 AI 엔진이 응답하지 않습니다.")

    raise HTTPException(status_code=500, detail="설정된 API 키가 없습니다.")

if __name__ == "__main__":
    import uvicorn
    port = int(os.environ.get("PORT", 8000))
    uvicorn.run(app, host="0.0.0.0", port=port)
