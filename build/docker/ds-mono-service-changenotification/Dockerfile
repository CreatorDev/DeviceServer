FROM creatordev/alpine-mono

COPY output/publish/Imagination.Service.ChangeNotification /app/

WORKDIR /app

EXPOSE 14050

ENTRYPOINT ["mono","Imagination.Service.ChangeNotification.exe"]