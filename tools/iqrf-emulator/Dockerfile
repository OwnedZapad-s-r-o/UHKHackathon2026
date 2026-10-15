# Emulátory IQRF gatewaye (jedna gateway emulator.py, cluster pěti multi_emulator.py) — bez instalace Pythonu na počítač
FROM python:3.12-slim

WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
COPY *.py ./

CMD ["python", "multi_emulator.py", "--host", "mqtt"]
