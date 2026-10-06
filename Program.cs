using SkyNet;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();   // 1. HttpContext service
var app = builder.Build();
app.UseMiddleware<IHandler>();               // 2. SkyNet handler as middleware
app.UseStaticHttpCurrent();                  // 3. static http class service
app.Run();