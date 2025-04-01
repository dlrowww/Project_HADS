FROM python:3.12

# 安装必要依赖
RUN apt-get update && \
    apt-get install -y wget gnupg unzip curl chromium chromium-driver

WORKDIR /app
COPY . .

# 安装 Python 库
RUN pip install -r requirements.txt

# 设置 Chrome 路径
ENV CHROME_BIN=/usr/bin/chromium
ENV CHROMEDRIVER_PATH=/usr/bin/chromedriver

CMD ["python", "main.py"]

