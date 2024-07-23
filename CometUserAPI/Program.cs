using AutoMapper;
using CometUserAPI.Container;
using CometUserAPI.Entities;
using CometUserAPI.Helper;
using CometUserAPI.Model;
using CometUserAPI.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IRefreshHandler, RefreshHandler>();
builder.Services.AddDbContext<CometUserDBContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("apicon")));

//builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

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

builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter(policyName: "fixedWindow", options =>
{
    options.Window = TimeSpan.FromSeconds(1);
    options.PermitLimit = 1;
    options.QueueLimit = 0;
    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
}).RejectionStatusCode = 401);

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
    if(existdata != null)
    {
        existdata.Name = costumer.Name;
        existdata.Email = costumer.Email;
        existdata.Phone = costumer.Phone;
        existdata.CreditLimit = costumer.CreditLimit;
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
app.UseRateLimiter();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}
app.UseCors();

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
