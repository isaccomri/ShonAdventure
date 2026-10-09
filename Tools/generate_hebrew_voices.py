#!/usr/bin/env python3
"""Render draft Hebrew dialogue to MP3 with distinct synthetic character profiles.

Usage:
  python -m pip install edge-tts
  python Tools/generate_hebrew_voices.py
"""
import asyncio
import csv
from pathlib import Path

import edge_tts

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "Design" / "DIALOGUE_HE.csv"
TARGET = ROOT / "LocalVoiceOutput"

PROFILES = {
    "Shon": ("he-IL-AvriNeural", "+7%", "+12Hz"),
    "Romi": ("he-IL-HilaNeural", "+5%", "+10Hz"),
    "James": ("he-IL-AvriNeural", "+18%", "+0Hz"),
    "Gregory": ("he-IL-AvriNeural", "-12%", "-24Hz"),
    "Itzik": ("he-IL-AvriNeural", "-5%", "-10Hz"),
    "Guard": ("he-IL-AvriNeural", "-10%", "-35Hz"),
    "Doll": ("he-IL-HilaNeural", "-20%", "-18Hz"),
}

async def main():
    voices = await edge_tts.list_voices()
    available = {v["ShortName"] for v in voices}
    missing = sorted({p[0] for p in PROFILES.values()} - available)
    if missing:
        raise RuntimeError(
            "Missing expected Hebrew voices: " + ", ".join(missing)
            + ". Run 'edge-tts --list-voices' and update PROFILES."
        )
    TARGET.mkdir(parents=True, exist_ok=True)
    with SOURCE.open("r", encoding="utf-8-sig", newline="") as fp:
        rows = list(csv.DictReader(fp))
    for row in rows:
        line_id = row["id"]
        speaker = row["speaker"]
        voice, rate, pitch = PROFILES[speaker]
        output = TARGET / (line_id + ".mp3")
        await edge_tts.Communicate(row["text"], voice, rate=rate, pitch=pitch).save(str(output))
        print(f"{line_id} -> {output.name} [{speaker}]")
    print(f"Rendered {len(rows)} files in {TARGET}")

if __name__ == "__main__":
    asyncio.run(main())
