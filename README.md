# Proyecto NovaHarvest - Fase UT2

Este proyecto está inspirado en el proyecto PIATEX que participa el IES Valle del Jerte (Plasencia) en el curso 2026-2027.

---

## Contexto

En el Valle del Jerte, una cooperativa agraria y sus productores tienen cada semana fruta, verdura y otros productos que no llegan a venderse: cereza que madura a la vez, tomate que no aguanta el transporte, calabacín que sobra de una cosecha. Parte acaba en la basura.

Al otro lado hay **compradores** que podrían aprovecharlo: residencias, comedores sociales, restaurantes y particulares. Lo que falta es una herramienta que los conecte.

La cooperativa quiere una aplicación web, **NovaHarvest**, que permita publicar la disponibilidad y los excedentes de los productores, avisar a los compradores interesados, ofrecer recetas para aprovechar esos productos y gestionar reservas.

Ninguna de esas funciones se programa todavía. En esta fase se debe montar **la infraestructura** sobre la que funcionará NovaHarvest:

- Un **servidor de aplicaciones** (ASP.NET Core sobre Kestrel) que genera las páginas en el servidor.
- Un **servidor web** (nginx) delante, como *reverse proxy*.
- Un entorno **reproducible** con Docker, que cualquiera pueda levantar con un único comando.

Es el mismo camino que hemos seguido con **NovaWarehouse**. Puedes revisar conceptos y código ya implementados.

---

## Normas generales

### Solo lo visto en clase

> **Solo puedes usar lo que hemos visto en clase.** Una parte del proyecto hecha con algo no visto en clase **no se puntúa**, aunque funcione. Si dudas de si algo se puede usar, pregúntalo **antes** de usarlo.

> **AUTORÍA**: Si hay duda acerca de si algún código ha sido generado o copiado, se preguntará al alumno directamente acerca de dicha implementación. Si no sabe responder SE CONSIDERA como generado o copiado, por lo que automaticamente tendrá una calificación de 0.

### Código

1. **`Program.cs` parte de la plantilla de `dotnet new web`**: solo se **añaden** líneas, y únicamente se cambia lo que devuelve la ruta `/`.
2. Los textos que ve el usuario en las páginas van **en español**.
3. El proyecto DEBE compilar **sin warnings**.
4. Toda parte de código, infraestructura, etc, debe ir en **inglés**. Quitando el texto de usuario.

### Control de versiones

1. El proyecto es un **repositorio git** desde el primer día.
2. Mínimo **6 commits**, con mensajes que describan el cambio. El historial debe mostrar cómo ha ido creciendo el proyecto: **no se acepta** un único commit con todo hecho.
3. Cada **fase** del apartado 2 termina en un commit cuyo mensaje empieza por `Fase 1:`, `Fase 2:` o `Fase 3:`. Al volver a ese commit, el fase debe **funcionar**.
4. Ningún fichero generado en el repositorio: `bin/` y `obj/` deben estar excluidos con `.gitignore`.

---

## 1. Estructura final

> **Requisito:** cualquiera (el profesor, un compañero, la cooperativa) debe poder clonar el repositorio y saber dónde está cada pieza del entorno.

Al terminar, el repositorio DEBE tener **exactamente** la siguiente estructura:

```text
novaharvest-web/
├── .gitignore
├── README.md
├── compose.yaml
├── cuestiones-teoricas/
├── web/
│   ├── Dockerfile
│   └── nginx.conf
└── backend/
    ├── Dockerfile
    ├── .dockerignore
    └── NovaHarvest.Web/
        ├── NovaHarvest.Web.csproj
        ├── Program.cs
        ├── appsettings.json
        ├── appsettings.Development.json
        ├── Properties/
        │   └── launchSettings.json
        └── wwwroot/
            ├── css/
            │   └── site.css
            └── img/
                └── logo.png
```

| Pieza | Qué es |
|---|---|
| `compose.yaml` | Describe el entorno completo: qué servicios hay y cómo se conectan |
| `web/` | El servidor web: nginx y su configuración. No contiene ningún fichero que servir |
| `backend/` | El servidor de aplicaciones: el proyecto ASP.NET Core y cómo se construye su imagen |
| `wwwroot/` | Los ficheros estáticos de NovaHarvest. |

`logo.png` es una imagen libre (el logotipo que diseñes para NovaHarvest).

---

## 2. Fases

> **Requisito:** el entorno debe construirse **paso a paso**, comprobando que cada pieza funciona antes de añadir la siguiente.

### Fase 1: nginx sirve una página estática

1. Un servicio `web` en `compose.yaml`, construido desde `web/`, con nginx `1.27-alpine`.
2. `web/public/index.html`: una página con el título `NovaHarvest` y el texto `Próximamente: aprovechamiento de excedentes del Valle del Jerte.`
3. `http://localhost:8080/` muestra esa página.

### Fase 2: nginx como *reverse proxy* de un backend simulado

1. `web/` ya no tiene ficheros que servir: solo su `Dockerfile` y su `nginx.conf`. **Todas** las peticiones se reenvían al servicio `backend`.
2. Un servicio `backend`, construido desde `backend/`, que es **otro nginx** que sirve `backend/response/index.html` y `backend/response/css/site.css`.
3. La página dice `Página servida por el backend (simulado)` y se ve con estilos.
4. `backend` **no publica ningún puerto**: solo `web` es accesible desde fuera.

### Fase 3: backend real con ASP.NET Core

1. El backend simulado desaparece (`backend/response/` y su `Dockerfile` de nginx) y se sustituye por el proyecto `NovaHarvest.Web`, creado con `dotnet new web`.
2. Los estilos pasan a `wwwroot/css/site.css`, y se añade `wwwroot/img/logo.png`.
3. Las páginas del apartado 4 se generan en C#.
4. La estructura queda **exactamente** como en el apartado 1.

---

## 3. Entorno

> **Requisito:** NovaHarvest debe levantarse **con un único comando**, en cualquier sistema operativo, sin instalar nada más que Docker, y el proceso .NET no debe estar expuesto directamente al exterior.

### 3.1. Servicios

| Servicio (`compose.yaml`) | Qué es | Puertos |
|---|---|---|
| `web` | nginx `1.27-alpine` con su propio `nginx.conf` | Publica el **8080** del equipo anfitrión hacia su puerto 80 |
| `backend` | `NovaHarvest.Web` (ASP.NET Core sobre Kestrel), escuchando en el **8080** del contenedor | **Ninguno** publicado al exterior |

### 3.2. Requisitos

1. `docker compose up -d --build`, ejecutado en la raíz de un clon recién descargado, levanta todo **a la primera**.
2. `web` arranca después de `backend`.
3. nginx **no sirve ningún fichero propio**: reenvía **todas** las peticiones (páginas y estáticos) al `backend`, y le pasa la cabecera `Host` original.
4. El `Dockerfile` del backend es **multi-etapa**: compila con la imagen del SDK de .NET 10 y ejecuta con la imagen de ASP.NET Core 10.
5. `.dockerignore` evita que `bin/` y `obj/` entren en la imagen.
6. La aplicación también funciona **sin Docker**: `dotnet run` dentro de `backend/NovaHarvest.Web/` muestra las mismas páginas, con sus estilos y el logotipo.

---

## 4. Páginas

> **Requisito:** las páginas de NovaHarvest se **generan en el servidor**, en cada petición, con código C#. Los estáticos (estilos e imágenes) los sirve también la aplicación, y deben verse igual con o sin nginx delante.

### 4.1. Rutas

Todas responden a `GET` y son **exactamente** estas:

| Ruta | Título | Texto del enlace en el menú | Contenido |
|---|---|---|---|
| `/` | Inicio | Inicio | Presentación de NovaHarvest: qué problema quiere resolver y para quién (3 o 4 frases) |
| `/about` | Acerca de | Acerca de | Proyecto, centro (`IES Valle del Jerte`), autor (tu nombre) y versión (`0.1.0`) |
| `/status` | Estado del servidor | Estado | Fecha y hora del servidor, con formato `16/10/2026 09:31:05`, y el texto `Página generada por ASP.NET Core (Kestrel)` |

| Estático | URL |
|---|---|
| Hoja de estilos | `/css/site.css` |
| Logotipo | `/img/logo.png` |

Cualquier otra ruta responde **404**.

### 4.2. Estructura de cada página

Las tres páginas tienen la misma estructura:

1. HTML válido: `<!DOCTYPE html>`, `<html lang="es">` y `<meta charset="UTF-8">`.
2. `<title>`: `NovaHarvest - {título de la página}`.
3. Enlace a la hoja de estilos `/css/site.css`.
4. `<header>` con el logotipo (`/img/logo.png`), el nombre `NovaHarvest` y un `<nav>` con enlaces a las tres páginas, en el orden de la tabla 4.1.
5. `<main>` con el contenido, que empieza por un `<h1>` con el título de la página.
6. `<footer>`: `NovaHarvest · IES Valle del Jerte · {año actual}`.

El diseño de `site.css` es libre.

---

## 5. Memoria (`README.md`)

> **Requisito:** quien reciba el proyecto debe poder arrancarlo sin preguntarte nada, y entender **por qué** está montado así.

El `README.md` de la raíz del repositorio debe tener estos apartados:

1. **Arranque**: cómo levantarlo con Docker, cómo pararlo y cómo ejecutarlo con `dotnet run`, con los comandos y las URL.
2. **Arquitectura**: un diagrama (imagen o texto) con el navegador, `web` y `backend`, los puertos y el recorrido de una petición.

## 5.1 Cuestiones UT2 (`cuestiones-ut2.md`)

> El propósito de este documento es entender qué estamos haciendo y por qué.

El archivo `cuestiones-ut2.md` estará en la carpeta `cuestiones-teoricas` del repositorio y debe responder a las siguientes preguntas:

> Responde **sobre tu proyecto**, no en general (entre 5 y 10 líneas cada una):

1. **Cliente y servidor.** Indica qué se ejecuta en el **cliente** y qué en el **servidor** cuando abres `/status`. Cuando NovaHarvest permita reservar producto, la regla *"no se pueden reservar más kilos de los disponibles"*, ¿dónde debe comprobarse obligatoriamente? ¿Por qué?
2. **Estático o dinámico.** ¿Cuál de tus tres páginas podría ser un HTML estático y cuál no? Justifícalo. Pon dos ejemplos de páginas que NovaHarvest necesitará y que **no** podrían ser estáticas.
3. **Ejecución en el servidor.** Explica paso a paso cómo una petición a `http://localhost:8080/status` acaba ejecutando tu código C#. ¿Qué cambiaría si NovaHarvest funcionara con **CGI**? ¿Y con un **módulo embebido** en el servidor web?
4. **Reverse proxy.** Haz `curl -I` (o mira la pestaña Red del navegador) a `/status` con Docker y con `dotnet run`. ¿Qué cabecera `Server` aparece en cada caso y por qué? Para el backend (`docker compose stop backend`) y pide `/status` y `/css/site.css`: ¿qué código HTTP recibes y por qué? ¿Por qué `backend` no publica ningún puerto?
5. **nginx.** ¿Por qué nginx no sirve `site.css` ni `logo.png` por su cuenta? Cita dos funciones que nginx podría aportar si NovaHarvest se publicara en Internet.
6. **Tecnologías.** Si la cooperativa pidiera hacer el backend con otra de las tecnologías *server-side* vistas en clase, ¿qué ficheros de tu repositorio cambiarían y cuáles se quedarían igual?
7. **Marcado y código.** Copia de tu `Program.cs` el código de una de las páginas y señala **dos** problemas concretos de generar HTML a base de cadenas. ¿Qué parte se repite en las tres páginas?
8. **Docker.** ¿Cuántas imágenes y cuántos contenedores tienes con el entorno levantado? Explica la diferencia usando tu proyecto. ¿Por qué la imagen final del backend no incluye el SDK? ¿Qué pasaría sin el `.dockerignore`?
9. **Herramientas.** Haz una tabla con las herramientas que has usado en el proyecto y el problema que resuelve cada una en él.
10. **Observación.** Compara la hora de `/status` con `dotnet run` y con Docker. ¿Coinciden? Explica por qué.

---

## 6. Comprobaciones

Antes de entregar, ejecuta estas comprobaciones **en orden**, en un clon limpio del repositorio. El proyecto se corregirá con **esta misma tabla**: cada comprobación se marca como superada o no, y cuenta en el CE de la última columna (ver apartado 9).

### Repositorio y código

| # | Acción | Resultado esperado | CE |
|---|---|---|---|
| C1 | `git ls-files` | Coincide **exactamente** con la estructura del apartado 1. No aparece nada de `bin/` ni `obj/` | g |
| C2 | `git log --oneline` | Al menos 3 commits con mensajes que describen el cambio, entre ellos `Hito 1:`, `Hito 2:` y `Hito 3:` | g |
| C3 | Revisar `backend/Dockerfile` y `backend/.dockerignore` | Dos etapas: compila con la imagen del SDK de .NET 10 y ejecuta con la de ASP.NET Core 10. `.dockerignore` excluye `bin/` y `obj/` | g |
| C4 | Revisar `web/nginx.conf` | Reenvía **todas** las peticiones a `backend` y pasa la cabecera `Host`. No sirve ningún fichero propio | d |
| C5 | Revisar `Program.cs` | Parte de la plantilla de `dotnet new web` (solo se añaden líneas). Las tres páginas se generan con código C# | b |
| C6 | Revisar `wwwroot/` y `Program.cs` | `site.css` y `logo.png` están en `wwwroot/` y se sirven con `MapStaticAssets()` | d |

### Con Docker

| # | Acción | Resultado esperado | CE |
|---|---|---|---|
| C7 | `docker compose up -d --build` | Se construyen las dos imágenes y arrancan los dos contenedores **a la primera**, sin errores | g |
| C8 | `docker compose ps` | Dos servicios en ejecución. Solo `web` publica un puerto (`8080->80`) | d |
| C9 | Abrir `http://localhost:8080/` y recorrer el menú | Las tres páginas cargan con estilos, logotipo y la misma cabecera, menú y pie | f |
| C10 | Ver el código fuente de cada página | Cumple los 6 puntos del apartado 4.2 (incluidos `<title>`, orden del menú, `<h1>` y año actual) | f |
| C11 | Abrir `/status` y recargar varias veces | Fecha y hora del servidor con formato `16/10/2026 09:31:05`, que cambia en cada recarga | b |
| C12 | Abrir `/css/site.css` y `/img/logo.png` | Se descargan la hoja de estilos y la imagen | d |
| C13 | Abrir `/nada` | 404 | f |
| C14 | `curl -I http://localhost:8080/status` | `200`, con `Content-Type: text/html` y `Server: nginx` | d |
| C15 | `docker compose stop backend` y abrir `/` y `/css/site.css` | nginx responde 502 en ambos | d |
| C16 | `docker compose start backend` y abrir `/` | Todo vuelve a funcionar | d |
| C17 | `docker compose down` | Se paran y eliminan los contenedores | g |

### Sin Docker

| # | Acción | Resultado esperado | CE |
|---|---|---|---|
| C18 | `dotnet build` en `backend/NovaHarvest.Web/` | 0 errores y 0 advertencias | f |
| C19 | `dotnet run` en `backend/NovaHarvest.Web/` y abrir la URL que muestra la consola | Las mismas páginas, con estilos y logotipo. `curl -I` muestra `Server: Kestrel` | c |

### Memoria

| # | Acción | Resultado esperado | CE |
|---|---|---|---|
| C20 | Leer el apartado **Arranque** del `README.md` | Están los comandos para levantar y parar con Docker, para `dotnet run`, y las URL | g |
| C21 | Leer el apartado **Arquitectura** del `README.md` | El diagrama muestra navegador, `web`, `backend`, los puertos y el recorrido de una petición | d |

### Fases

| # | Acción | Resultado esperado | CE |
|---|---|---|---|
| C22 | Volver al commit de cada hito y hacer `docker compose up -d --build` | Cada hito funciona como describe el apartado 2 | g |

> Así se trabaja en entornos de programación reales, los commits deben representar un cambio que funciona y a poder ser, aislado del resto.

---

## 7. Orden de trabajo sugerido

Cada paso termina, como mínimo, en un commit. **Por ejemplo**:

| Paso | Tarea |
|---|---|
| 1 | Repositorio y `.gitignore` |
| 2 | **Hito 1**: nginx sirviendo `index.html` con Docker Compose |
| 3 | **Hito 2**: `nginx.conf` con *reverse proxy* y backend simulado |
| 4 | Proyecto `NovaHarvest.Web` funcionando con `dotnet run`: estáticos y las tres páginas |
| 5 | **Hito 3**: `Dockerfile` multi-etapa y `.dockerignore`; el backend real sustituye al simulado |
| 6 | `README.md`: arranque, arquitectura y `cuestiones-ut2.md` |

Comprueba a menudo que sigue arrancando **en Docker**, no solo con `dotnet run`.

---

## 8. Entrega

- Debéis incluirme como colaborador en el proyecto, tenéis mi GitHub en el Moodle.

---

## 9. Rúbrica de evaluación (RA1)

Este proyecto evalúa el **RA1**: *Selecciona las arquitecturas y tecnologías de programación Web en entorno servidor, analizando sus capacidades y características propias.*

Cada CE se evalúa con dos tipos de evidencia:

- **Comprobaciones** (C1 a C22, apartado 6): superada o no superada.
- **Preguntas** (P1 a P10, apartado 5): se valora la respuesta.

### 9.1. Pasos

La nota de cada CE se calcula en tres pasos, con la misma regla para todos.

**Paso 1. Nota de cada pregunta.**

| Nota | ¿Qué responde? | ¿En qué se apoya? |
|---|---|---|
| 0 | Nada, o es incorrecta | — |
| 3 | Menos de la mitad de lo que se pregunta | — |
| 5 | Casi todo, pero le falta alguna parte | Genérica |
| 6 | Casi todo, pero le falta alguna parte | Cita su proyecto |
| 7 | Casi todo, pero le falta alguna parte | Con evidencias reales |
| 8 | Todo lo que se pregunta | Genérica |
| 9 | Todo lo que se pregunta | Cita su proyecto |
| 10 | Todo lo que se pregunta | Con evidencias reales |

- **Genérica**: valdría igual para cualquier otro proyecto.
- **Cita su proyecto**: nombra **sus** ficheros, servicios, puertos o rutas.
- **Con evidencias reales**: además, incluye código, comandos y salidas de **su** proyecto (`curl`, `docker ps`, fragmentos de `Program.cs`…).

**Paso 2. Nota de las preguntas del CE.** Es la media de sus preguntas. Si el CE tiene una sola pregunta, es la nota de esa pregunta.

**Paso 3. Comprobaciones del CE.** Si las supera todas (o el CE no tiene), la nota del CE es la de sus preguntas. Si falla alguna, es la de sus preguntas con un **máximo de 4**.

> **Ejemplo.** CE **d)**: P4 = 8 y P5 = 7, así que las preguntas valen 7,5. Si superan C4, C6, C8, C12, C14, C15, C16 y C21, el CE vale **7,5**; si falla C14, vale **4**.

### 9.2. Criterios

| CE | Comprobaciones | Preguntas | Nota |
|---|---|---|---|
| **a)** Modelos de ejecución en el servidor y en el cliente | — | P1, P10 | |
| **b)** Generación dinámica de páginas | C5, C11 | P2 | |
| **c)** Mecanismos de ejecución de código en el servidor web | C19 | P3 | |
| **d)** Servidor de aplicaciones e integración con el servidor web | C4, C6, C8, C12, C14, C15, C16, C21 | P4, P5 | |
| **e)** Lenguajes y tecnologías del entorno servidor | — | P6 | |
| **f)** Integración del lenguaje de marcas con el lenguaje del servidor | C9, C10, C13, C18 | P7 | |
| **g)** Herramientas de programación en entorno servidor | C1, C2, C3, C7, C17, C20, C22 | P8, P9 | |

**Nota del RA1** = media de las notas de los CE (todos valen igual).

### 9.3. Normas de corrección

- Una comprobación que se basa en algo no visto en clase cuenta como **no superada**.
- Las comprobaciones se ejecutan con la estructura, los nombres, los puertos y las rutas del enunciado; si no se respetan, la comprobación cuenta como **no superada**.
- **Autoría**: si el alumno no sabe explicar su código, la calificación del proyecto es **0** (ver *Normas generales*).