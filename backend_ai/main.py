from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import os
from groq import Groq
import google.generativeai as genai
from dotenv import load_dotenv
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

load_dotenv()
app = FastAPI()

# 1. API 키 설정 (Render Environment에서 설정한 이름과 일치해야 함)
GROQ_API_KEY = os.getenv("capstone")  
GOOGLE_API_KEY = os.getenv("GOOGLE_API_KEY")

# Groq/Gemini 클라이언트 초기화
groq_client = Groq(api_key=GROQ_API_KEY) if GROQ_API_KEY else None
if GOOGLE_API_KEY: genai.configure(api_key=GOOGLE_API_KEY)

# ✅ 유니티와 통신할 데이터 규격 (이름 중요!)
class ChatRequest(BaseModel):
    prompt: str
    system: str = "당신은 저택의 도우미입니다."

@app.get("/")
def health_check():
    return {"status": "online", "message": "Server is Running!"}

@app.post("/chat")
async def chat(request: ChatRequest):
    # 1순위: Groq (Llama 3.1)
    if groq_client:
        try:
            logger.info(f"🚀 Groq 호출: {request.prompt[:20]}...")
            completion = groq_client.chat.completions.create(
                model="llama-3.1-8b-instant", # ✅ 최신 모델로 교체
                messages=[
                    {"role": "system", "content": request.system},
                    {"role": "user", "content": request.prompt}
                ],
                temperature=0.7,
            )
            return {"response": completion.choices[0].message.content}
        except Exception as e:
            logger.error(f"❌ Groq 에러: {e}")

    # 2순위: Gemini (Fallback)
    if GOOGLE_API_KEY:
        try:
            logger.info("🔄 Gemini 엔진 전환")
            model = genai.GenerativeModel('gemini-1.5-flash-latest')
            response = model.generate_content(f"System: {request.system}\nUser: {request.prompt}")
            return {"response": response.text}
        except Exception as e:
            raise HTTPException(status_code=500, detail="모든 AI 엔진 실패")

    raise HTTPException(status_code=500, detail="API 키 설정 필요")

if __name__ == "__main__":
    import uvicorn
    port = int(os.environ.get("PORT", 8000))
    uvicorn.run(app, host="0.0.0.0", port=port)