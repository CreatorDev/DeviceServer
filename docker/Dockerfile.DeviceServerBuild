
FROM creatordev/dotnet-mono-base

COPY . /app

WORKDIR /app

RUN dotnet restore && \
  dotnet build --configuration=Release src/*

# copy libuv.so to /usr/lib to work around the DllNotFoundException that
# is thrown by Kestrel on startup. Ideally when this nuget package is installed
# it should copy the library to the correct location, so this can hopefully be
# removed in the future.
RUN ["bash", "-c", "cp /root/.nuget/packages/Libuv/*/runtimes/debian-x64/native/libuv.so /usr/lib"]
