FROM creatordev/alpine-mono:4.2.2.30-r1

COPY output/publish/Imagination.Service.Webhook /app/

WORKDIR /app

ENTRYPOINT ["mono","Imagination.Service.Webhook.exe"]