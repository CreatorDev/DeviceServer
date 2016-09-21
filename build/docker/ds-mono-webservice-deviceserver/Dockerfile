FROM creatordev/alpine-mono:4.2.2.30-r1

COPY output/publish/Imagination.WebService.DeviceServer /app/

WORKDIR /app

EXPOSE 8080

ENTRYPOINT ["mono","Imagination.WebService.DeviceServer.exe"]