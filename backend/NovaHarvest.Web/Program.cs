var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
var app = builder.Build();

app.UseRequestLocalization("es-ES");
app.MapStaticAssets();
app.MapRazorPages();


app.Run();