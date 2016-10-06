FROM creatordev/alpine-mono

COPY output/publish/Imagination.WebService.DeviceServer /app/

WORKDIR /app

EXPOSE 8080

ENTRYPOINT ["mono","Imagination.WebService.DeviceServer.exe"]