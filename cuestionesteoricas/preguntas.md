1. **Cliente y servidor**

Cuando abro /status en el navegador, en el servidor se ejecuta nginx, que recibe la petición en el puerto 8080 del host y la reenvía con proxy_pass http://backend:8080 al contenedor backend. Allí Kestrel recibe la petición, ASP.NET Core la enruta con app.MapGet("/status", ...) y mi código C# construye el HTML con DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"). En el cliente solo se ejecuta el navegador: el navegador recibe el HTML ya generado, pide /css/site.css y /img/logo.png, y los renderiza. No hay lógica de negocio en el navegador.
La regla que dice “no se pueden reservar más kilos de los disponibles” debe verificarse siempre en el servidor, específicamente en el backend desarrollado con C# El motivo es que el cliente no es confiable. Un usuario puede alterar la solicitud usando las herramientas de desarrollo del navegador, usar comandos como curl, o enviar un JSON directamente con cualquier cantidad. Solo el servidor puede garantizar que los datos sean correctos. Por eso, el servidor es la única fuente de verdad.

2. **Estático o dinámico**

De mis tres páginas, /about podría ser estática. El contenido de /about, que incluye proyecto, IES Valle del Jerte, autor y versión 0.1.0, no cambia entre peticiones. / también sería estática en esta fase 3, que es una presentación fija. Sin embargo, dejará de ser estática cuando se publiquen excedentes reales. /status no puede ser estática. Muestra DateTime.Now, que cambia en cada recarga, tal como se comprobó en C11.

Dos páginas que NovaHarvest necesitará y que no pueden ser estáticas:

Listado de excedentes disponibles (/excedentes): esta página se actualiza en tiempo real según la base de datos, que cambia cuando un productor sube excedentes o un comprador hace una reserva.

Mis reservas (/reservas): esta página muestra información personalizada según el usuario que está conectado y el estado actual de sus reservas.

3. **Ejecución en el servidor**

Paso a paso una petición a http://localhost:8080/status:

El navegador hace una solicitud GET a /status en el puerto 8080 del host.

Docker redirige ese puerto al 80 del contenedor web, que ejecuta nginx 1.27-alpine.

nginx lee el archivo web/nginx.conf, donde la directiva location / con proxy_pass http://backend:8080; reenvía todas las peticiones, y añade la cabecera Host original usando proxy_set_header Host $host;.

La petición llega al contenedor backend, al puerto 8080 donde escucha Kestrel.

Kestrel pasa la petición al pipeline de middleware de ASP.NET Core. El método MapStaticAssets() no la captura porque la ruta no es /css/... ni /img/....

El enrutador llega a app.MapGet("/status", ...).

Se ejecuta el delegado C#; genera el HTML con la fecha actual y devuelve el tipo text/html.

La respuesta sube por la cadena inversa: Kestrel → nginx → navegador.

Si NovaHarvest funcionara con CGI, cada petición arrancaría un proceso nuevo desde cero (cargar runtime, leer config, etc.), lo que sería mucho más lento, y no habría pipeline de middleware.

Con un módulo embebido en el servidor web, al estilo mod_aspdotnet en Apache, el código .NET se ejecutaría dentro del propio proceso del servidor web, sin Kestrel ni reverse proxy. La ventaja sería menor latencia, pero se perdería portabilidad, porque nginx no lo soporta, y el backend no se podría mover a otro servidor.


4. **Reverse proxy**

La diferencia está en quién responde primero. Con Docker, nginx está adelante y firma la respuesta. Con dotnet run, no hay nginx, así que firma Kestrel.

Con Docker:

 $ curl -I http://localhost:8080/status
HTTP/1.1 200 OK
Server: nginx/1.27.3
Content-Type: text/html

Con dotnet run puerto 5296

$ curl -I http://localhost:5296/status

HTTP/1.1 200
Server: Kestrel
Content-Type: text/html

Después de ejecutar docker compose stop backend, las rutas /status y /css/site.css devuelven un error 502 Bad Gateway. Nginx recibe la solicitud, intenta conectarse con backend:8080, no lo encuentra y responde con 502. Estas rutas fallan porque el archivo nginx.conf reenvía todas las peticiones al backend, incluidos los archivos estáticos.

El servicio backend no publica ningún puerto en compose.yaml (no tiene la sección ports:, solo es accesible dentro de la red interna de Docker). Por lo tanto, el único punto de entrada desde el exterior es nginx, que controla y filtra todo el tráfico. Kestrel permanece aislado.