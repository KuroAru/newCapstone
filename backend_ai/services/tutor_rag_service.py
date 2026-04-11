from __future__ import annotations

import json
import logging
import math
from pathlib import Path
from typing import Any

logger = logging.getLogger(__name__)


def _cosine_similarity(a: list[float], b: list[float]) -> float:
    if len(a) != len(b) or not a:
        return 0.0
    dot = sum(x * y for x, y in zip(a, b, strict=True))
    na = math.sqrt(sum(x * x for x in a))
    nb = math.sqrt(sum(y * y for y in b))
    if na == 0 or nb == 0:
        return 0.0
    return dot / (na * nb)


def _truncate_block(text: str, max_chars: int) -> str:
    if len(text) <= max_chars:
        return text
    return text[: max_chars - 3] + "..."


class TutorRAGService:
    """Loads precomputed embeddings and retrieves context for tutor queries."""

    def __init__(
        self,
        index_path: Path,
        *,
        api_key: str,
        embedding_model: str,
    ) -> None:
        self._index_path = index_path
        self._api_key = api_key
        self._embedding_model = embedding_model
        self._chunks: list[dict[str, Any]] = []
        self._load_index()

    def _load_index(self) -> None:
        if not self._index_path.is_file():
            logger.warning("Tutor RAG index missing: %s — retrieval disabled", self._index_path)
            self._chunks = []
            return
        try:
            data = json.loads(self._index_path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError) as e:
            logger.error("Failed to load tutor RAG index: %s", e)
            self._chunks = []
            return
        self._chunks = data.get("chunks") or []
        logger.info("Loaded tutor RAG index: %d chunks from %s", len(self._chunks), self._index_path)

    @property
    def enabled(self) -> bool:
        return bool(self._chunks)

    def _embed_query(self, text: str) -> list[float] | None:
        if not self._api_key:
            logger.warning("GOOGLE_API_KEY empty — cannot embed tutor RAG query")
            return None
        try:
            import google.generativeai as genai

            genai.configure(api_key=self._api_key)
            res = genai.embed_content(
                model=self._embedding_model,
                content=text,
                task_type="retrieval_query",
            )
            emb = res.get("embedding")
            if isinstance(emb, list):
                return emb
        except Exception as e:
            logger.error("Gemini embed_query failed: %s", e)
        return None

    def build_context_block(self, query_text: str, *, top_k: int, max_context_chars: int) -> str:
        if not self._chunks:
            return ""

        query_vec = self._embed_query(query_text)
        if query_vec is None:
            return ""

        scored: list[tuple[float, dict[str, Any]]] = []
        for ch in self._chunks:
            emb = ch.get("embedding")
            if not isinstance(emb, list):
                continue
            sim = _cosine_similarity(query_vec, emb)
            scored.append((sim, ch))

        scored.sort(key=lambda x: x[0], reverse=True)
        picked = scored[:top_k]

        lines: list[str] = [
            "[참고 자료 — 아래 인용문만 퀴즈 출제·해설·근거로 사용하세요. 없는 내용은 상식으로 보충하지 마세요.]",
        ]
        total = 0
        for rank, (sim, ch) in enumerate(picked, start=1):
            cid = ch.get("id", f"chunk_{rank}")
            body = (ch.get("text") or "").strip()
            if not body:
                continue
            piece = f"--- [{rank}] (id={cid}, score={sim:.3f})\n{body}"
            if total + len(piece) > max_context_chars:
                remain = max_context_chars - total - 50
                if remain > 80:
                    piece = f"--- [{rank}] (id={cid})\n{_truncate_block(body, remain)}"
                else:
                    break
            lines.append(piece)
            total += len(piece)

        return "\n\n".join(lines)
