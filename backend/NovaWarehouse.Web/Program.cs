var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapStaticAssets();

app.MapGet("/", () => "Hello World!");
app.MapGet("/estado", () => Results.Content(
$"""
<link rel="stylesheet" href="/css/estilos.css">
<h1>NovaWarehouse</h1>
<p>Hora del servidor: {DateTime.Now:HH:mm:ss}</p>
""",
"text/html"));

app.Run();
