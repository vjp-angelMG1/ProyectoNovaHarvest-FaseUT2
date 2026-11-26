Preguntas UT3 - NovaHarvest

1. De .cshtml a HTML

Cuando hacemos dotnet build se genera una clase de C# (Razor es el responsable) de Pages/Surplus.cshtml. Esta clase se compila y queda en NovaHarvest.Web.dll.

Cuando entra una petición GET a /surplus se crea una instancia de SurplusModel. Se llama a su método OnGet(). A partir de este modelo se genera el HTML que se envía al navegador.

Para generar código HTML fijo (que no dependa de variables/expressions) se usa WriteLiteral. Para generar el resultado de una expression de C# se usa Write (se encodea para evitar problemas de HTML). Todo esto se inserta en el layout en la posición de RenderBody().

En Status.cshtml tenemos un WriteLiteral (el header fijo) y un Write (la expression @Model.Visits).

En el Dockerfile publicamos la dll. No hace falta copiar los ficheros .cshtml al contenedor final (ya están compilados en la dll por razor).

Evidencias de la prueba

Hemos compilado correctamente (con dotnet build). Para ver cómo genera las clases razor hemos hecho:

bash

dotnet build --no-incremental -p:EmitCompilerGeneratedFiles=true

Y hemos mirado en el directorio obj el fichero generado de Status. Encontramos los siguientes fragmentos:

csharp

WriteLiteral("Estado del servidor

\nFecha y hora del servidor: ");

csharp

Write(Model.ServerTime.ToString("dd/MM/yyyy HH:mm:ss")

Este es el comienzo de la llamada a Write generada por razor (continúa en otras líneas). También encontramos el comienzo de Write(Model.Visits. El texto fijo corresponde al header y al comienzo del párrafo de Status.cshtml. Los dos Write corresponden a @Model.ServerTime.ToString("dd/MM/yyyy HH:mm:ss") y @Model.Visits.


docker compose exec backend ls /app nos muestra el fichero NovaHarvest.Web.dll. No hay una carpeta Pages ni ficheros .cshtml.


2. Tecnologías

Esta línea en el fichero Surplus.cshtml está implementada en Razor (que es el motor de vistas que usamos)

cshtml

Precio de excedente: @Model.SurplusPrice.ToString("C") / kg

Si hubiéramos usado PHP o ASP clásico y asumimos que el precio ya estaba formateado como moneda sería:

php

Precio de excedente: <?= $surplusPrice ?> / kg

asp

Precio de excedente: <%= surplusPrice %> / kg

La principal diferencia es la sintaxis (usar <?= en PHP o <%= en ASP clásico). En el caso de Razor solo tenemos que usar @ y es capaz de saber donde acaba la expresión.


En la UT2 era necesario generar el HTML en el propio Program.cs. Se usaban strings para generar HTML. Además se usaba la función CreatePage y también se definían las rutas de tipo GET/HEAD con MapMethods. Por ejemplo en el Program.cs teníamos:


Ahora en la UT3 hemos usado AddRazorPages y MapRazorPages. El HTML se encuentra en los ficheros cshtml. Los elementos comunes se encuentran en _Layout.cshtml. Los cálculos se realizan en los PageModel. El entorno es igual (nginx+Kestrel).


Comparación de los archivos del proyecto

En el Program.cs de la UT2 (commit 603352f) teníamos algo similar a:

csharp

app.MapMethods("/", new[] { "GET", "HEAD" }, () =>

Results.Content(CreatePage("Inicio", homeContent), "text/html; charset=utf-8"));

En la UT3 en el Program.cs tenemos builder.Services.AddRazorPages(); y app.MapRazorPages(); (en lugar de las rutas con string html). Pero también mantenemos app.MapStaticAssets(); para los recursos estáticos. Podemos ver el resultado (precio del excedente) en la captura 2 (apartado 5 ).

3. Etiquetas de código



FormaEjemplo real en el proyectoHTML en el caso A

Implícita@Model.RemainingKg223

Explícita@(Model.Surplus.Note ?? "Sin observaciones")Recoger antes de las 14:00 (hay que ver el código fuente)



Bloque@{ ViewData["Title"] = "Excedente destacado"; }No hay texto directamente. El layout utiliza el título.

Escape@@novaharvest@novaharvest

Comentario Razor@ La nota del productor se muestra como texto. @No está en el HTML

En About.cshtml el email info@novaharvest.es es reconocido por Razor como un email. Es decir, dejamos la arroba como está. En @@novaharvest hay que poner doble arroba para que Razor entienda que queremos mostrar una arroba literalmente y no es que queramos acceder a una variable novaharvest.

Evidencias en el HTML generado

En About.cshtml tenemos esta línea:

cshtml

Escríbenos a info@novaharvest.es o síguenos en @@novaharvest

En el código fuente vemos:



html

Escríbenos a info@novaharvest.es o síguenos en @novaharvest

El bloque de título no genera HTML. El layout utiliza ViewData["Title"] para generar el NovaHarvest - Excedente destacado. La expresión implícita de los kilos es Kilos disponibles: 223

4. Sintaxis



Si escribo @Model.SurplusPrize en lugar de @Model.SurplusPrice estoy intentando acceder a una propiedad que no existe en el objeto SurplusModel. Por ello el compilador protesta.



Por otro lado, si escribo:

cshtml

@Model.Surplus.PricePerKg 0.7m



El motor Razor sólo procesará la parte que está antes del espacio. Es decir, @Model.Surplus.PricePerKg. El resto será texto plano. Para que realice la multiplicación completa tengo que utilizar paréntesis:

cshtml

@(Model.Surplus.PricePerKg 0.7m)

En la versión final, el cálculo de descuento se realiza en el propio SurplusModel. Y en la página sólo mostramos el resultado.

Evidencias de las dos pruebas.

Si cambiamos el nombre de la propiedad y realizamos un dotnet build nos sale el siguiente error (CS1061) en Surplus.cshtml(13,32): "SurplusModel" does not contain a definition for "SurplusPrize" and no accessible method of extension accepting a first argument of type "SurplusModel".

Por otro lado, en la segunda prueba no tenemos problemas de compilación. En la pantalla se muestra Prueba: 3,20 0.7m (el motor Razor no realiza la multiplicación). Si añadimos los paréntesis, se muestra Prueba: 2,240. Los párrafos son: Prueba: 3,20 0.7m

y Prueba: 2,240

. Posteriormente hemos eliminado estas líneas temporales.

5. Efectos en el documento

La nota de Surplus es Recoger antes de las 14:00

Por otro lado en Razor aparecen las etiquetas (no en formato html) porque el encodeado de los caracteres html.

El html es <b>Recoger antes de las 14:00</b>

Si utilizamos Html.Raw entonces se parsearía como etiqueta html. En este caso un malintencionado podría inyectar código malicioso (XSS). En nuestro proyecto no se utiliza.

Escenario A.

El html que se genera es Días para consumirlo: 5

y Reservar

Escenario C.

reservedKg = 343. El html generado es Días para consumirlo: 5

y Reservar

Escenario D.

Hay 120kg reservados y DateTime.Today.AddDays(-6). El html es Días para consumirlo: 1

y Reservar

Es decir, si no hay urgencia UrgentClass es null. Y si aún quedan kilos (SoldOut es false), por tanto en razor no se insertan esos atributos.

Evidencias de los siguientes escenarios

Escenario A

html

Días para consumirlo: 5

Reservar

html


Nota del productor: <b>Recoger antes de las 14:00</b>


Escenario C

html

Días para consumirlo: 5

Reservar

Escenario D

html

Días para consumirlo: 1

Reservar

En el escenario D se puede ver que en el texto de días está en rojo y negrita. En el escenario C el botón está deshabilitado.


6. Directivas

Directiva o fichero Qué cambia en este proyecto

@page Convierte los .cshtml en páginas con su propia URL. (Sobre About, Status y Surplus)

@page "/" Fija explícitamente el path de Index a /

@page "/ticket" Cambia el path de Reservations/Ticket.cshtml a /ticket

@model Asocia Status, Surplus y Ticket con su PageModel asociado.

@using Importa el espacio de nombres NovaHarvest.Web.Models (Para las páginas)

@namespace Coloca las clases Razor en el namespace de las páginas

@inject Permite acceder al IWebHostEnvironment Env en el layout (Para mostrar el entorno)

_ViewStart.cshtml Fija el layout por defecto común a todas

Layout = null El justificante no utiliza este layout.

Con la directiva @namespace se sitúa en el espacio de nombres de las páginas (NovaHarvest.Web.Pages) donde también están SurplusModel y StatusModel. Si esto se quita no funcionan las referencias por nombre simple.



El path de /reservations/ticket no es el del justificante. En esta página tenemos @page "/ticket" que marca otro path absoluto.



Evidencias de la prueba

Al quitar la directiva @namespace NovaHarvest.Web.Pages de _ViewImports.cshtml se producen 6 errores de compilación (CS0246). Por ejemplo, el compilador nos indica que no encuentra los tipos StatusModel y SurplusModel. Posteriormente se ha vuelto a poner la directiva.



El justificante está en /ticket. Es un HTML independiente (sin menu y footer del layout). Los paths de /reservations/ticket y /nada devuelven error 404.

7. Tipos y operadores

Dato (calculado en SurplusModel.OnGet()) Tipo Operadores/métodos (por qué)

ExpiryDate DateTime AddDays(ShelfLifeDays) => Calculamos la fecha de consumo.

DaysRemaining int Diferencia de fechas + .Days => Día entero.

SurplusPrice decimal y - => Calculamos el precio (con descuento).

RemainingKg int - => Kilos enteros restantes.

BoxCount int / => Cajas completas.

LooseKg int % => Kilos sobrantes.

BoxSize int = (de BoxKg) => Tamaño de la caja.

SurplusValue decimal => Valor total del excedente.

SoldOut bool == => Comprobamos si nos quedan 0 kilos.

State string ?: y < => Texto dependiendo de disponibilidad.

UrgentClass string? <=, >, &&, ?: => Decide si hay urgencia o no.

También tenemos alguna división de enteros (como RemainingKg / BoxKg). Por ejemplo 223 / 5 = 44 (con resto 3). Esto es porque estamos trabajando con kilos enteros. Si los kilos fueran decimales podría ser 44,6 (que no son cajas completas).



Si mostramos el valor de un bool directamente en Razor (por ejemplo) se mostrará True o False. Si es null no se muestra nada. En la nota usamos ?? para mostrar "Sin observaciones" en caso de que no haya texto.



Evidencias de los cálculos

En el escenario A ( captura 2 ) tenemos 223kg. Habrá 44 cajas completas y 3kg sueltos. En el escenario B (kilos reservados a 300) tenemos 43kg, 8 cajas, 3kg sueltos, 96,32€ y el estado "¡Últimos kilos!" ( ver captura ). En el escenario E (pasamos null como nota) se muestra "Sin observaciones" ( ver captura ). Los escenarios C y D ya los hemos visto en el apartado 5.

8. Cultura



En Program.cs tenemos la llamada app.UseRequestLocalization("es-ES"); que indica que las peticiones tendrán la cultura española de España.



En Surplus.cshtml se utiliza ToString("C") para mostrar los precios. En Index.cshtml se utiliza DateTime.Now.ToString("D") para mostrar la fecha larga.



Así tenemos que en el escenario inicial los precios aparecen en 3,20 € / 2,24 € / 499,52 €. Y la fecha en español.



Si no tenemos la instrucción de localización, el formato es dependiente de la cultura del entorno en el que estamos. Podemos tener docker y dotnet run con diferentes configuraciones. Con es-ES los tenemos uniformados en ambos lugares.



Resultados observados



Entorno Configuración Fecha larga Inicio Precio original Precio excedente Valor excedente



Docker Con es-ES martes, 24 de noviembre de 2026 3,20 € 2,24 € 499,52 €



dotnet run (Windows) Sin UseRequestLocalization martes, 24 de noviembre de 2026 3,20 € 2,24 € 499,52 €



Docker Sin UseRequestLocalization Tuesday, 24 November 2026 ¤3.20 ¤2.24 ¤499.52



En nuestro windows sin la instrucción sigue siendo en español. En el contenedor en inglés. El separador decimal es punto. Se utiliza el símbolo genérico de moneda. Con la instrucción app.UseRequestLocalization("es-ES"); nos aseguramos de que no dependa del entorno. Al final de la prueba hemos vuelto a ponerla. Con la configuración original y en dotnet run también tenemos los importes en español (capturas 14-19). En la captura 16 podemos ver Entorno: Development.

9. Ámbito

La variable es local (int visits = 0; visits++) y está envuelta en @{ }, por tanto cada vez que se ejecuta la página se genera en tiempo de ejecución.



Por otro lado en cada GET se recrea. Por tanto siempre vale 1.



En la versión final el campo SiteStats.Visits es static y es compartido por toda la aplicación. En StatusModel.OnGet() se incrementa y se copia a Visits.



Es compartido por todos los navegadores ya que es un proceso del servidor. Con el reinicio del backend (se reinicia el campo static) en la primera petición a /status pasa de 0 a 1.



Los archivos layout y Status.cshtml corresponden a diferentes clases. Por tanto no es posible acceder a variables locales de la página en el layout. Para compartir el título en esta petición se usa ViewData["Title"].



Evidencias de las pruebas del contador



El contador es static. Se muestra 1 en la primera captura. Tras varias recargas se muestra 19. Si entramos desde un incógnito se muestra 20. Es decir que no está ligado a la sesión del navegador.



Tras ejecutar docker compose restart backend en el primer acceso se muestra 1.



Para probar el ámbito local he modificado temporalmente Status.cshtml. He puesto en su interior int visits = 0; visits++. Además he sustituido @Model.Visitas por @visits. En las dos capturas (19:55:52 y 19:56:27) se sigue viendo 1 (aunque han pasado minutos). Posteriormente he vuelto a poner el código original.

10. PageModel

Los datos de SurplusModel son la propiedad Surplus. Además están los datos privados (las constantes). Los cálculos de OnGet() (por ejemplo, el cálculo del precio con descuento: SurplusPrice = Surplus.PricePerKg (1 - SurplusDiscount))



El fichero Surplus.cshtml contiene las etiquetas HTML. El título. Las expresiones de presentación (por ejemplo @Model.SurplusPrice.ToString("C")) y la nota (con ??).



De esta forma podemos separar los cálculos de HTML. Por ejemplo, si queremos modificar el dato o el cálculo en C# no hay que tocar el HTML.



Para /surplus tenemos tres peticiones GET. Se crean tres instancias de SurplusModel. Cada una de ellas sólo existe durante la petición correspondiente.



Por otro lado SiteStats.Visits es static. Existe mientras está en ejecución el proceso del servidor. Es compartido entre todos los navegadores. Sólo aumenta cuando se solicita /status.



Ejemplo del código del proyecto



En Pages/Surplus.cshtml.cs en OnGet():



csharp



SurplusPrice = Surplus.PricePerKg * (1 - SurplusDiscount);



RemainingKg = Surplus.AvailableKg - Surplus.ReservedKg;



En Pages/Surplus.cshtml:



cshtml



Precio de excedente: @Model.SurplusPrice.ToString("C") / kg



Kilos disponibles: @Model.RemainingKg



El modelo es el que prepara los datos y la vista es la que los muestra. En las capturas de los escenarios de los apartados 5 y 7 se puede ver como sólo modificando los datos de entrada varía el resultado.

Comprobación final tras restaurar el código


