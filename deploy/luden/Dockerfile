# Imagen de runtime de CarniSys WebCore (ASP.NET Core 10) para la VPS luden.
# NO compila: copia el `dotnet publish` hecho en local (ver deploy/luden/README.md), igual criterio
# que el resto de los deploys de CarniSys (publish precompilado, sin pipeline en el servidor).
# El contexto de build es la carpeta del publish. Ese publish NO debe traer WebCore.dll.config ni
# App.config: traen las credenciales de la base de DEV y quedarian horneadas en la imagen. La config
# real se monta en runtime desde /srv/carnisys/WebCore.dll.config (ver docker-compose.yml).
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# QuestPDF (PDF de facturas/etiquetas) necesita fontconfig + una fuente en Linux: la imagen base no
# las trae y sin ellas falla al generar el primer PDF, no al arrancar.
RUN apt-get update \
 && apt-get install -y --no-install-recommends libfontconfig1 fonts-dejavu-core \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --chown=app:app . .

# Claves de Data Protection (firman la cookie de login): si viven solo en el contenedor, cada
# recreate cierra todas las sesiones. Se persisten en un volumen montado en esta ruta (el directorio
# se crea aca, con dueno `app`, para que el volumen nombrado herede el permiso).
# /app/AFIP: punto de montaje del volumen carnisys-afip (certificados); creado con dueno app para que el volumen lo herede y la app pueda escribir.
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys /app/AFIP && chown -R app:app /home/app /app/AFIP

USER app
# Puerto 8080 = default de las imagenes .NET 8+ (ASPNETCORE_HTTP_PORTS). Nadie lo publica al host.
ENTRYPOINT ["dotnet", "WebCore.dll"]
