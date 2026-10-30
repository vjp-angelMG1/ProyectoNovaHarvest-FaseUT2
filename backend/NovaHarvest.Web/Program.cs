var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapStaticAssets();

app.MapGet("/", () => Results.Content($"""
    <!DOCTYPE html>
    <html lang="es">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>NovaHarvest</title>
        <link rel="stylesheet" href="/css/site.css">
    </head>
    <body>
        <header>
            <img src="/img/logo.png" alt="Logo de NovaHarvest" width="150">
            <nav>
                <a href="/">Inicio</a>
                <a href="/about">Sobre el proyecto</a>
                <a href="/status">Estado del servidor</a>
            </nav>
        </header>
        <main>

        <h1>NovaHarvest</h1>
        <p>Bienvenido a NovaHarvest.</p>
        <p>Una plataforma que busca aprovechar los excedentes agrícolas del Valle del Jerte.</p>
        <p>Está dirigida a agricultores que necesitan dar salida a sus productos sobrantes.</p>
        <p>También permite a los consumidores encontrar productos agrícolas locales y reducir el desperdicio alimentario.</p>
        </main>
         <footer>
           <p>NovaHarvest · IES Valle del Jerte · {DateTime.Now.Year}</p>
        </footer>
    </body>
       
    </html>
    """, "text/html"));
    app.MapGet("/about", () => Results.Content($""" 
    <!DOCTYPE html>
    <html lang="es">
    <head>
        <meta charset="UTF-8">
        <title>Sobre NovaHarvest</title>
        <link rel="stylesheet" href="/css/site.css">
        
    </head>
    <body>
         <header>
            <img src="/img/logo.png" alt="Logo de NovaHarvest" width="150">
             <nav>
                <a href="/">Inicio</a>
                <a href="/about">Sobre el proyecto</a>
                <a href="/status">Estado del servidor</a>
            </nav>
        </header>
        <main>
        <h1>Sobre NovaHarvest</h1>
        <p>Proyecto desarrollado en el IES Valle del Jerte.</p>
        <p>Autor: Raúl Albalat Pérez</p>
        <p>Versión: 0.1.0</p>
        </main>
         <footer>
            <p>NovaHarvest · IES Valle del Jerte · {DateTime.Now.Year}</p>
        </footer>
    </body>
       
    </html>
    """, "text/html"));
    app.MapGet("/status", () => Results.Content($"""
    <!DOCTYPE html>
    <html lang="es">
    <head>
        <meta charset="UTF-8">
        <title>Estado del servidor</title>
        <link rel="stylesheet" href="/css/site.css">
    </head>
    <body>
        <header>
            <img src="/img/logo.png" alt="Logo de NovaHarvest" width="150">
           <nav>
                <a href="/">Inicio</a>
                <a href="/about">Sobre el proyecto</a>
                <a href="/status">Estado del servidor</a>
            </nav>
        </header>
        <main>
            <h1>Estado del servidor</h1>
            <p>Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
            <p>Página generada por ASP.NET Core (Kestrel)</p>
        </main>
         <footer>
            <p>NovaHarvest · IES Valle del Jerte · {DateTime.Now.Year}</p>
        </footer>
    </body>
       
    </html>
    """, "text/html"));

app.Run();