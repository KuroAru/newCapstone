from __future__ import annotations

import os
from functools import lru_cache

from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    groq_api_key: str = ""
    google_api_key: str = ""

    default_model_groq: str = "llama-3.3-70b-versatile"
    default_model_gemini: str = "gemini-2.0-flash"

    default_temperature: float = 0.7
    max_tokens: int = 512

    class Config:
        env_file = ".env"
        extra = "ignore"


@lru_cache
def get_settings() -> Settings:
    return Settings(
        groq_api_key=os.getenv("capstone", ""),
        google_api_key=os.getenv("GOOGLE_API_KEY", ""),
    )
