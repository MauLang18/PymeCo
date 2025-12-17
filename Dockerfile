# =========================
# Base image (runtime)
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# Zona horaria
RUN apt-get update && apt-get install -y tzdata
ENV TZ=America/Costa_Rica

# Dependencias necesarias (System.Drawing, PDFs, etc.)
RUN apt-get update && \
  apt-get install -y \
  apt-utils \
  libgdiplus \
  libc6-dev \
  libxrender1 \
  libxext6 \
  libfontconfig1 \
  libfreetype6 \
  fontconfig \
  xfonts-base \
  xfonts-75dpi \
  ca-certificates && \
  ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll && \
  rm -rf /var/lib/apt/lists/*

# Carpeta para archivos estáticos
WORKDIR /app
RUN mkdir -p /app/wwwroot && chmod 755 /app/wwwroot

# Copiar libwkhtmltox.so desde el root del proyecto
COPY libwkhtmltox.so /app/libwkhtmltox.so
RUN chmod 755 /app/libwkhtmltox.so

# Hacer visible la librería al loader dinámico
ENV LD_LIBRARY_PATH=/app

EXPOSE 8080

# =========================
# Build image
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copiar solo los csproj (mejora cache de Docker)
COPY ["POS.Web/POS.Web.csproj", "POS.Web/"]
COPY ["POS.Application/POS.Application.csproj", "POS.Application/"]
COPY ["POS.Domain/POS.Domain.csproj", "POS.Domain/"]
COPY ["POS.Infrastructure/POS.Infrastructure.csproj", "POS.Infrastructure/"]

# Restore
RUN dotnet restore "POS.Web/POS.Web.csproj"

# Copiar el resto del código
COPY . .

# Build
WORKDIR /src/POS.Web
RUN dotnet build "POS.Web.csproj" \
  -c ${BUILD_CONFIGURATION} \
  --no-restore \
  -o /app/build

# =========================
# Publish image
# =========================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

WORKDIR /src/POS.Web

RUN dotnet publish "POS.Web.csproj" \
  -c ${BUILD_CONFIGURATION} \
  --no-restore \
  -o /app/publish \
  -p:UseAppHost=false

# =========================
# Final image
# =========================
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

RUN chmod 755 /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "POS.Web.dll"]

