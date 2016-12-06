FROM creatordev/dotnet-mono-base

COPY . /app

WORKDIR /app

RUN dotnet restore && \
  dotnet build --configuration=Debug src/* && \
  dotnet build --configuration=Debug test/*




