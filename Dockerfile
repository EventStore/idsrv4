FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build

WORKDIR /build

# https://github.com/moby/moby/issues/15858
# Docker will flatten out the file structure on COPY
# We don't want to specify each csproj either - it creates pointless layers and it looks ugly
# https://code-maze.com/aspnetcore-app-dockerfiles/
COPY ./*.sln ./
COPY ./src/*/*.csproj ./src/
RUN for file in $(ls src/*.csproj); do mkdir -p ./${file%.*}/ && mv $file ./${file%.*}/; done

RUN dotnet restore --runtime=linux-musl-x64

COPY . .

RUN dotnet build --configuration=Release --runtime=linux-musl-x64 --framework=netcoreapp3.1

FROM build as publish

RUN dotnet publish --configuration=Release --runtime=linux-musl-x64 --self-contained \
     --framework=netcoreapp3.1 --output /publish /p:PublishTrimmed=true

FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine AS runtime
ARG UID=1000
ARG GID=1000

ENV ASPNETCORE_ENVIRONMENT=Development

RUN mkdir /etc/idsrv4 && touch /etc/idsrv4/idsrv4.conf && touch /etc/idsrv4/users.conf

WORKDIR /opt/idsrv4

RUN addgroup --gid ${GID} "idsrv4" && \
    adduser \
    --disabled-password \
    --gecos "" \
    --ingroup "idsrv4" \
    --no-create-home \
    --uid ${UID} \
    "idsrv4"

COPY --from=publish /publish ./

RUN chown -R idsrv4:idsrv4 /opt/idsrv4

USER idsrv4

ENTRYPOINT ["/opt/idsrv4/IdentityServer"]
