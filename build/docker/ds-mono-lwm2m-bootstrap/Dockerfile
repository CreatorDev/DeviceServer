FROM creatordev/alpine-mono:4.2.2.30-r1

COPY output/publish/Imagination.LWM2M.Bootstrap /app/

WORKDIR /app

EXPOSE 15683/udp 15684/udp

ENTRYPOINT ["mono","Imagination.LWM2M.Bootstrap.exe"]