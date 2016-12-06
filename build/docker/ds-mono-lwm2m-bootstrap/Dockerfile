FROM creatordev/alpine-mono

COPY output/publish/Imagination.LWM2M.Bootstrap /app/

WORKDIR /app

EXPOSE 15683/udp 15684/udp

ENTRYPOINT ["mono","Imagination.LWM2M.Bootstrap.exe"]