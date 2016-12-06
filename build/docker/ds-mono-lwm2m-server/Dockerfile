FROM creatordev/alpine-mono

COPY output/publish/Imagination.LWM2M.Server /app/

WORKDIR /app

EXPOSE 5683/udp 5684/udp 14080

ENTRYPOINT ["mono","Imagination.LWM2M.Server.exe"]