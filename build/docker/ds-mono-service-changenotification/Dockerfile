FROM creatordev/alpine-mono:4.2.2.30-r1

COPY output/publish/Imagination.Service.ChangeNotification /app/

WORKDIR /app

EXPOSE 14050

ENTRYPOINT ["mono","Imagination.Service.ChangeNotification.exe"]