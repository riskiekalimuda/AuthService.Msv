// Pengaturan ini memaksa .NET dan Npgsql menyelaraskan format DateTime lama/lokal menjadi kompatibel dengan pemformatan database
using AuthService.Msv.Models;
using AuthService.Msv.Profiles;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthMsvDbContext>(options => 
options.UseNpgsql(builder.Configuration.GetConnectionString("AuthMsvDBConnection")));

builder.Services.AddControllers();

builder.Services.AddAutoMapper(x => { }, typeof(MappingProfile));

builder.Services.AddScoped<AuthService.Msv.Services.AuthService>();

var app = builder.Build();
app.MapControllers();


app.Run();


