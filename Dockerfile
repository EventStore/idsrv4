FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build

WORKDIR /build

COPY ./IdentityServer.sln ./src/*/*.csproj ./

RUN for file in $(ls *.csproj); do mkdir -p ./src/${file%.*}/ && mv $file ./src/${file%.*}/; done

RUN dotnet restore --runtime=linux-musl-x64

WORKDIR /build/src

COPY ./src .

WORKDIR /build

RUN dotnet build --configuration=Release --runtime=linux-musl-x64 --no-restore --framework=netcoreapp3.1

FROM build as publish

#RUN dotnet dev-certs https --export-path ./src/IdentityServer/idsrv4.pfx

RUN dotnet publish --configuration=Release --runtime=linux-musl-x64 --self-contained \
     --framework=netcoreapp3.1 --output /publish /p:PublishTrimmed=true src/IdentityServer

FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine AS runtime
ARG UID=1000
ARG GID=1000

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
