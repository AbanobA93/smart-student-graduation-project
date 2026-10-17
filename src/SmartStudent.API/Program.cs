using System.Reflection;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using SmartStudent.API.Middlewares;
using SmartStudent.Application.Common;
using SmartStudent.Application.Interfaces;
using SmartStudent.Application.Mappings;
using SmartStudent.Application.Services;
using SmartStudent.Application.Validators;
using SmartStudent.Core.Entities;
using SmartStudent.Infrastructure.Data;
using SmartStudent.Infrastructure.Identity;
using SmartStudent.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Serilog (Console + Daily Rolling File)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/smartstudent-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Configure SQL Server DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Configure ASP.NET Core Identity with native Guid keys
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 4. Configure JWT Authentication (with exact TTL: ClockSkew = TimeSpan.Zero)
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? new JwtSettings();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 5. Register AutoMapper & FluentValidation
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// 6. Register Repositories & Application Services (Dependency Inversion Principle)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClassRoomService, ClassRoomService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

builder.Services.AddSingleton<IFileStorageService>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
    return new LocalFileStorageService(webRoot);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 7. Configure Swagger / OpenAPI with JWT Bearer Lock Button & XML Comments
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Smart Student Management System — RESTful API",
        Version = "v1",
        Description = "Clean Architecture ASP.NET Core Web API with Full UUIDv7 Schema, Role-Domain Isolated JWT Authentication (Admin, Teacher, Student), Encapsulated Grading (F, C, B, A, A+), and Daily Attendance Tracking."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT Access Token obtained from /api/auth/login/{domain}."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// 8. Global Exception Handling Middleware (First in pipeline)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Enable Swagger UI in all environments for Graduation Project demonstration
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Student Management System API v1");
    c.DocumentTitle = "Smart Student Management System — Swagger UI";
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 9. Auto-Apply Migrations & Seed Default Roles + Admin User on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        await DbSeeder.SeedRolesAndAdminAsync(roleManager, userManager);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();
