## Memoria

## Arranque

Para iniciar NovaHarvest con Docker, abro una terminal en la carpeta raíz del proyecto usando Ctrl+Shift+ñ y ejecuto:

``` git

docker compose up -d --build
´´´

Docker construye las imágenes y arranca los servicios en segundo plano. Cuando termina, ya puedo acceder al navegador a estas direcciones:

- Inicio: http://localhost:8080/

- Excedentes: http://localhost:8080/surplus

- Sobre el proyecto: http://localhost:8080/about

- Estado del servidor: http://localhost:8080/status

- Justificante: http://localhost:8080/ticket

- Hoja de estilos: http://localhost:8080/css/site.css

- Logotipo: http://localhost:8080/img/logo.png

Cuando ya he finalizado las pruebas de NovaHarvest, detengo NovaHarvest y elimino los contenedores con:

``` git

docker compose down
´´´

Si prefiero no usar Docker ni nginx, también puedo arrancar el backend por separado. Necesito tener instalado el SDK de .NET 10. Me dirijo a backend/NovaHarvest.Web/ y lanzo:

``` git

dotnet run
´´´


Por defecto me permite entrar en http://localhost:5240/. NovaHarvest muestra las mismas páginas y los mismos archivos estáticos que con Docker. Así, el navegador llega directamente a Kestrel y nginx no interviene.
---


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