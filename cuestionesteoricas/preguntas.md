1. **Cliente y servidor**

Cuando abro `/status`, el navegador (el cliente) solicita la página y muestra el HTML que recibe. En Docker, nginx recibe la petición en el puerto 8080 del equipo y la reenvía al servicio `backend` por el puerto 8080 interno. Kestrel recibe la petición y `Program.cs` ejecuta `app.MapGet("/status", ...)`, que genera la fecha y hora del servidor. El navegador también solicita `/css/site.css` y `/img/logo.png`; ASP.NET Core los sirve desde `wwwroot` mediante `MapStaticAssets()`. La regla que impide reservar más kilos de los disponibles debe comprobarse en el backend. El cliente no es de confianza: se puede modificar o enviar una petición sin usar la página, así que el servidor debe validar la disponibilidad.

2. **Estático o dinámico**

En esta fase, `/` y `/about` tienen contenido principalmente fijo y podrían prepararse como páginas estáticas. Sin embargo, las tres páginas incluyen el año actual con `{DateTime.Now.Year}`, así que tal como están implementadas el servidor inserta un dato dinámico. `/status` es claramente dinámica: muestra la fecha y la hora actuales, que cambian al recargar. Más adelante, `/excedentes` tendría que mostrar la disponibilidad real y actualizada de los productos. También `/reservas` mostraría las reservas y su estado para cada comprador conectado; ambas páginas dependerían de los datos y del usuario, por lo que no bastaría con un HTML fijo.

3. **Ejecución en el servidor**

Al abrir `http://localhost:8080/status`, el navegador envía una petición HTTP al puerto 8080 del equipo. En `compose.yaml`, ese puerto se publica en el puerto 80 del contenedor `web`, donde está nginx. La regla `location /` de `web/nginx.conf` reenvía la petición a `http://backend:8080` y conserva la cabecera `Host`. Kestrel escucha en ese puerto dentro del contenedor backend y ASP.NET Core busca la ruta `/status`. `Program.cs` ejecuta el delegado de `app.MapGet`, construye el HTML con la fecha actual y devuelve `text/html`. La respuesta vuelve por Kestrel y nginx hasta el navegador. Con CGI, normalmente se iniciaría un proceso para atender cada petición, con más coste de arranque. Con un módulo embebido, el código se ejecutaría dentro del proceso del servidor web; en este proyecto no usamos ninguno de esos dos modelos: usamos ASP.NET Core con Kestrel detrás de nginx.

4. **Reverse proxy**

Con Docker, la petición entra primero por nginx, así que la respuesta normalmente muestra `Server: nginx`; nginx pasa la petición al backend y devuelve su respuesta. Si ejecuto el proyecto con `dotnet run`, el navegador llega directamente a Kestrel, por lo que la cabecera normalmente indica `Server: Kestrel`. En mi configuración, `dotnet run` utiliza `http://localhost:5296` según `Properties/launchSettings.json`; debo usar ese puerto para la prueba. Si paro el backend con `docker compose stop backend`, tanto `/status` como `/css/site.css` devuelven `502 Bad Gateway`, porque nginx reenvía ambas rutas al servicio que ya no está disponible. `compose.yaml` no publica ningún puerto del backend al equipo: solo `web` publica `8080:80`, y los contenedores se comunican por la red interna de Docker. La versión exacta que aparece en la cabecera `Server` puede variar según la imagen instalada.

5. **nginx**

nginx no sirve directamente `site.css` ni el logotipo porque `web/nginx.conf` no configura una carpeta propia con esos archivos: reenvía todas las rutas al backend mediante `proxy_pass`. ASP.NET Core encuentra los recursos estáticos bajo `backend/NovaHarvest.Web/wwwroot`, y `MapStaticAssets()` los publica. Si NovaHarvest estuviera en Internet, nginx podría gestionar la terminación HTTPS para recibir conexiones seguras y reenviar tráfico al backend. También podría limitar peticiones por IP con reglas de rate limiting para reducir abusos y picos de tráfico.

6. **Tecnologías**

Si cambiara ASP.NET Core por otra tecnología server-side, reemplazaría el proyecto `backend/NovaHarvest.Web` y adaptaría `backend/Dockerfile` para construir y ejecutar esa tecnología. También tendría que revisar el servicio `backend` de `compose.yaml`, especialmente la imagen y el puerto al que nginx reenvía las peticiones. `web/Dockerfile` y la función de reverse proxy de `web/nginx.conf` podrían mantenerse si el nuevo backend siguiera disponible con el mismo nombre de servicio y puerto. El CSS y el logotipo podrían reutilizarse, aunque habría que servirlos desde la nueva aplicación o ajustar dónde se alojan. También actualizaría el README y estas respuestas si cambiasen los comandos, las rutas o la arquitectura.

7. **Marcado y código**

En `Program.cs`, la ruta `/status` devuelve HTML mediante una cadena interpolada de C#:

```csharp
app.MapGet("/status", () => Results.Content($"""
    <h1>Estado del servidor</h1>
    <p>Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
    <p>Página generada por ASP.NET Core (Kestrel)</p>
    """, "text/html"));
```

Un problema es el mantenimiento: el HTML está mezclado con C#, así que puede ser más difícil de editar, revisar y validar con herramientas de HTML. Otro problema aparecería si interpolásemos texto enviado por usuarios: habría que codificarlo correctamente para evitar que se interprete como HTML o JavaScript. En las tres rutas se repiten partes de la estructura, como el encabezado, el menú, el enlace al CSS y el pie de página. En estas cadenas raw no hace falta duplicar las comillas del HTML como en una cadena C# normal.

8. **Docker**

Con `docker compose up`, el proyecto define dos servicios y, si ambos arrancan correctamente, hay dos contenedores: `web` (nginx) y `backend` (ASP.NET Core con Kestrel). Puedo comprobarlos con `docker compose ps`; los nombres concretos los asigna Compose y pueden variar. Una imagen es la plantilla desde la que se crea un contenedor; el contenedor es una instancia que se ejecuta. El Dockerfile del backend tiene una etapa de compilación con `mcr.microsoft.com/dotnet/sdk:10.0` y una etapa final basada en `mcr.microsoft.com/dotnet/aspnet:10.0`, que solo necesita el runtime. Así el SDK no se incluye en la imagen final. El número de imágenes que muestra `docker images` depende de las imágenes base y de la caché local, por lo que no es siempre un número fijo. `backend/.dockerignore` excluye `bin/` y `obj/` del contexto de compilación, evitando enviar resultados locales innecesarios al construir.

9. **Herramientas**

Estas son las herramientas y tecnologías que he utilizado para montar y comprobar NovaHarvest:

| Herramienta | Para qué la he usado en este proyecto |
|---|---|
| Docker | Construir y ejecutar los contenedores del entorno. |
| Docker Compose | Definir los servicios `web` y `backend`, iniciarlos juntos y detenerlos. |
| ASP.NET Core y C# | Definir las rutas y generar las páginas en el servidor. |
| Kestrel | Recibir y procesar las peticiones de la aplicación ASP.NET Core. |
| nginx | Recibir las peticiones externas y reenviarlas al backend como reverse proxy. |
| Git | Guardar el historial de cambios y los hitos del proyecto. |
| Navegador web | Abrir las páginas y comprobar su contenido, estilos y navegación. |
| Terminal y `curl` | Ejecutar comandos del proyecto y consultar respuestas HTTP y sus cabeceras. |

10. **Observación**

La página `/status` muestra la hora que obtiene `DateTime.Now` en el proceso del servidor. Al ejecutar con `dotnet run`, ese proceso usa la zona horaria del equipo; al ejecutar en Docker, usa la del contenedor Linux. En esta configuración no se establece una zona horaria para el contenedor, que normalmente usa UTC, mientras que en Plasencia en noviembre se usa CET (UTC+1). Por eso, lo esperable es que la hora de Docker vaya una hora por detrás de la de Windows en esta fecha; en verano la diferencia sería de dos horas. Para comprobarlo, comparo `/status` en ambos modos casi al mismo tiempo y tengo en cuenta que debo parar o cambiar la ejecución entre pruebas para no confundir los puertos.
