using Fish_Manage;
using Fish_Manage.Models;
using Fish_Manage.Models.Momo;
using Fish_Manage.Repository;
using Fish_Manage.Repository.DTO;
using Fish_Manage.Repository.IRepository;
using Fish_Manage.Service.IService;
using Fish_Manage.Service.Momo;
using Fish_Manage.Service.Payment;
using Fish_Manage.Service.Vosk;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;


//Momo
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.AddHttpContextAccessor();

// Configure DbContext
builder.Services.AddDbContext<FishManageContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<FishManageContext>()
    .AddDefaultTokenProviders();
builder.Services.AddResponseCaching();
async Task CreateRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roleNames = { "admin", "customer" };

    foreach (var roleName in roleNames)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}


// Configure AutoMapper, Repositories, and Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentCODService, PaymentCODService>();
builder.Services.AddScoped<ICouponModelRepository, CouponModelRepository>();
builder.Services.AddScoped<EmailSender>();
builder.Services.AddScoped<APIResponse>();

//httpClient
builder.Services.AddHttpClient();
//Mail
builder.Services.AddTransient<IEmailSender, EmailSender>();
//Vosk
builder.Services.AddSingleton<VoskModelService>();
//Cloudinary
builder.Services.AddSingleton<CloudinaryService>();
//JWT
builder.Services.AddScoped<JwtService>();

builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });
// Add Controllers with support for JSON and XML
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
})
.AddNewtonsoftJson()
.AddXmlDataContractSerializerFormatters();

var key = builder.Configuration["ApiSettings:Secret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Set JWT as the default scheme
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
    };
})
.AddFacebook(o =>
{
    o.ClientId = builder.Configuration["FaceBook:AppId"];
    o.ClientSecret = builder.Configuration["FaceBook:AppSecret"];
})
.AddCookie()
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["GoogleKeys:ClientId"];
    options.ClientSecret = builder.Configuration["GoogleKeys:ClientSecret"];
    options.SignInScheme = IdentityConstants.ExternalScheme;
});


//okta
//.AddOpenIdConnect(options =>
//{
//    options.Authority = builder.Configuration["Okta:Authority"];
//    options.ClientId = builder.Configuration["Okta:ClientId"];
//    options.ClientSecret = builder.Configuration["Okta:ClientSecret"];
//    options.ResponseType = "code";
//    options.SaveTokens = true;
//    options.CallbackPath = new PathString("/");
//    options.RequireHttpsMetadata = false;
//});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                      "Enter 'Bearer' [space] and then your token in the text input below. \r\n\r\n" +
                      "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

//System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls13;


HttpClientHandler handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};

HttpClient client = new HttpClient(handler);


//ServicePointManager.ServerCertificateValidationCallback +=   //allow all certificate
//    (sender, certificate, chain, errors) =>
//    {
//        return true;
//    };


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CreateRoles(services);
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
