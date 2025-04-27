using AutoMapper;
using CometUserAPI.Container;
using CometUserAPI.Entities;
using CometUserAPI.Helper;
using CometUserAPI.Model;
using CometUserAPI.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Security;
using System.Text;

// Tutorial https://www.youtube.com/watch?v=zNRVz7dgfuE
// https://www.youtube.com/watch?v=SIdgC4bqNZ4&t=1659s
// https://www.youtube.com/watch?v=e0n_Ai-n_TI&ab_channel=NihiraTechiees
// https://www.youtube.com/watch?v=8J3nuUegtL4
// https://github.com/nihira2020/WEBAPI_CORE_7

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IRefreshHandler, RefreshHandler>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IUserRoleService, UserRoleService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddDbContext<CometUserDBContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("apicon")));

//builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("apicon2")));

//builder.Services.AddAuthorization();

//builder.Services.AddIdentityApiEndpoints<User>().AddEntityFrameworkStores<ApplicationDbContext>();

var _authkey = builder.Configuration.GetValue<string>("JwtSettings:securitykey");

builder.Services.AddAuthentication(item =>
{
    item.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    item.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(item =>
{
    item.RequireHttpsMetadata = true;
    item.SaveToken = true;
    item.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authkey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

var autoMapper = new MapperConfiguration(item => item.AddProfile(new AutoMapperHandler()));
IMapper mapper = autoMapper.CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddCors(p => p.AddPolicy("corspolicy", build =>
{
    build.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
}
));

builder.Services.AddCors(p => p.AddPolicy("corspolicy1", build =>
{
    build.WithOrigins("https://dimain3.com").AllowAnyHeader().AllowAnyMethod();
}
));

builder.Services.AddCors(p => p.AddDefaultPolicy(build =>
{
    build.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
}
));

//builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter(policyName: "fixedWindow", options =>
//{
//    options.Window = TimeSpan.FromSeconds(10);
//    options.PermitLimit = 1;
//    options.QueueLimit = 0;
//    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
//}).RejectionStatusCode = 401);

builder.Services.AddRateLimiter(o => o
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        // configuration
    }));

string logpath = builder.Configuration.GetSection("Logging:Logpath").Value ?? "";
var _logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(logpath)
    .CreateLogger();
builder.Logging.AddSerilog(_logger);

var _jwtsetting = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(_jwtsetting);

var app = builder.Build();

app.MapGet("/minimalapi", () => "Nihira Techiees");
app.MapGet("/getchannel", (string channelName) => "Welcome to "+channelName).WithOpenApi(opt =>
{
    var parameter = opt.Parameters[0];
    parameter.Description = "Enter channel name";
    return opt;
});
app.MapGet("/getcustomer", async (CometUserDBContext db) => {
    return await db.TblCustomers.ToListAsync();
});
app.MapGet("/getcustomerbycode/{code}", async (CometUserDBContext db, string code) => {
    return await db.TblCustomers.FindAsync(code);
});
app.MapPost("/createcustomer", async (CometUserDBContext db, TblCustomer costumer) => {
    await db.TblCustomers.AddAsync(costumer);
    await db.SaveChangesAsync();
});
app.MapPut("/updatecustomer/{code}", async (CometUserDBContext db, TblCustomer costumer, string code) => {
    var existdata = await db.TblCustomers.FindAsync(code);
    if (existdata != null)
    {
        existdata.Name = costumer.Name;
        existdata.Email = costumer.Email;
        existdata.Phone = costumer.Phone;
        //existdata.CreditLimit = costumer.CreditLimit;
    }
    await db.SaveChangesAsync();
});
app.MapDelete("/deletecustomer/{code}", async (CometUserDBContext db, string code) => {
    var existdata = await db.TblCustomers.FindAsync(code);
    if (existdata != null)
    {
        db.TblCustomers.Remove(existdata);
    }
    await db.SaveChangesAsync();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.MapIdentityApi<User>();

app.UseCors();

app.UseHttpsRedirection();

//app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
