
---

## Memoria

### Arranque

Para levantar NovaHarvest con Docker, abro una terminal en la carpeta raíz del proyecto y ejecuto Ctrl+shif+ñ:

```git
docker compose up -d --build
```

Docker construye las imágenes necesarias y arranca los servicios en segundo plano. Después puedo abrir las páginas desde el navegador:

- Inicio: http://localhost:8080/
- Sobre el proyecto: http://localhost:8080/about
- Estado del servidor: http://localhost:8080/status

Cuando termino de probar la aplicación, paro y elimino los contenedores con:

```git
docker compose down
```

También puedo ejecutar el backend sin Docker ni nginx. Desde la carpeta raíz del proyecto, ejecuto:

```git
dotnet run --project backend\NovaHarvest.Web\NovaHarvest.Web.csproj
```

La aplicación muestra en la terminal la dirección en la que está escuchando. Según la configuración actual de `Properties/launchSettings.json`, puedo abrir http://localhost:5296/. En este modo, el navegador se comunica directamente con Kestrel y nginx no interviene.

### Arquitectura

La aplicación separa el servidor web del servidor que ejecuta la aplicación. Con Docker, el navegador envía la petición a nginx. nginx la reenvía al backend ASP.NET Core, donde Kestrel procesa la ruta y se genera la página. Después, la respuesta vuelve al navegador pasando otra vez por nginx.

```text
Recorrido de una petición:

Navegador
  │  http://localhost:8080
  ▼
web: nginx
  │  proxy_pass http://backend:8080
  ▼
backend: ASP.NET Core sobre Kestrel
  │  procesa la ruta y genera la respuesta
  ▼
web: nginx
  │  devuelve la respuesta
  ▼
Navegador

Puertos:
- Puerto 8080 del equipo → puerto 80 del contenedor web (nginx).
- Puerto 8080 del contenedor backend (Kestrel), accesible desde la red de Docker.
- El backend no publica un puerto directamente en el equipo.
```

En esta configuración, solo nginx publica un puerto hacia el equipo: el 8080, conectado al puerto 80 del contenedor web. nginx reenvía las peticiones al servicio `backend` por el puerto 8080 dentro de la red de Docker. El backend no publica ese puerto al exterior, por lo que las peticiones externas entran primero por nginx. Si ejecuto el backend con `dotnet run`, accedo directamente a Kestrel por el puerto 5296 y nginx no participa.