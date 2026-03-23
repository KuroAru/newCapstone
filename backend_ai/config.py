from __future__ import annotations

import os
from functools import lru_cache
from pathlib import Path

from pydantic_settings import BaseSettings, SettingsConfigDict

# 저장소 클론 후 실행 위치(cwd)와 무관하게 backend_ai/.env 를 읽는다.
_BACKEND_DIR = Path(__file__).resolve().parent
_ENV_PATH = _BACKEND_DIR / ".env"


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        env_file=_ENV_PATH if _ENV_PATH.is_file() else None,
        env_file_encoding="utf-8",
        extra="ignore",
    )

    groq_api_key: str = ""
    google_api_key: str = ""

    default_model_groq: str = "llama-3.3-70b-versatile"
    default_model_gemini: str = "gemini-2.0-flash"

    default_temperature: float = 0.7
    max_tokens: int = 512


@lru_cache
def get_settings() -> Settings:
    # Let pydantic load .env (GROQ_API_KEY, GOOGLE_API_KEY). Legacy env name for Groq only if still empty.
    s = Settings()
    if not s.groq_api_key:
        legacy = os.getenv("capstone", "")
        if legacy:
            s = s.model_copy(update={"groq_api_key": legacy})
    return s
