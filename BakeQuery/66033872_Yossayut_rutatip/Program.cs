using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using _66033872_Yossayut_rutatip.Models.db;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtIssuer = jwtSection["Issuer"] ?? "BakeQuery";
var jwtAudience = jwtSection["Audience"] ?? "BakeQueryClient";
var jwtSecret = jwtSection["Secret"] ?? "BakeQuery_Default_Secret_Key_Change_Me_123456";

builder.Services
    .AddAuthentication(options =>
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
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrWhiteSpace(context.Token) &&
                    context.Request.Cookies.TryGetValue("bakequery_access_token", out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/Account/Login");
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                context.Response.Redirect("/Account/AccessDenied");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<Csi402BakequeryContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.Parse("9.6.0-mysql")
     ));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Csi402BakequeryContext>();
    EnsureOrderRepliesTable(db);
    EnsurePaymentProofsTable(db);
    SeedRoles(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=HomePage}/{id?}");


app.Run();

static void SeedRoles(Csi402BakequeryContext db)
{
    var requiredRoles = new Dictionary<string, string>
    {
        ["MANAGER"] = "Full access to Back Office",
        ["ADMIN"] = "Can manage orders, menus, and promotions",
        ["STAFF"] = "Can process incoming orders and reply to customer orders",
        ["CUSTOMER"] = "Can place orders and track order status"
    };

    var now = DateTime.Now;
    var existingRoles = db.Roles.ToList();

    foreach (var roleName in requiredRoles.Keys)
    {
        var existingRole = existingRoles
            .FirstOrDefault(r => r.RoleName.ToUpper() == roleName);

        if (existingRole == null)
        {
            db.Roles.Add(new Role
            {
                RoleName = roleName,
                Description = requiredRoles[roleName],
                CreatedAt = now,
                UpdatedAt = now
            });

            continue;
        }

        var hasChanges = false;
        if (existingRole.RoleName != roleName)
        {
            existingRole.RoleName = roleName;
            hasChanges = true;
        }

        if (string.IsNullOrWhiteSpace(existingRole.Description))
        {
            existingRole.Description = requiredRoles[roleName];
            hasChanges = true;
        }

        if (hasChanges)
        {
            existingRole.UpdatedAt = now;
        }
    }

    db.SaveChanges();
}

static void EnsureOrderRepliesTable(Csi402BakequeryContext db)
{
    db.Database.ExecuteSqlRaw(
        @"CREATE TABLE IF NOT EXISTS `order_replies` (
            `reply_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
            `order_id` BIGINT UNSIGNED NOT NULL,
            `replied_by` BIGINT UNSIGNED NULL,
            `reply_message` VARCHAR(1000) NOT NULL,
            `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (`reply_id`),
            INDEX `idx_order_replies_order_id` (`order_id`),
            INDEX `idx_order_replies_replied_by` (`replied_by`),
            CONSTRAINT `fk_order_replies_order`
                FOREIGN KEY (`order_id`) REFERENCES `orders`(`order_id`) ON DELETE CASCADE,
            CONSTRAINT `fk_order_replies_replied_by`
                FOREIGN KEY (`replied_by`) REFERENCES `users`(`user_id`) ON DELETE SET NULL
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;");
}

static void EnsurePaymentProofsTable(Csi402BakequeryContext db)
{
    db.Database.ExecuteSqlRaw(
        @"CREATE TABLE IF NOT EXISTS `payment_proofs` (
            `proof_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
            `order_id` BIGINT UNSIGNED NOT NULL,
            `uploaded_by` BIGINT UNSIGNED NOT NULL,
            `file_path` VARCHAR(500) NOT NULL,
            `original_file_name` VARCHAR(255) NOT NULL,
            `verification_status` ENUM('PENDING','APPROVED','REJECTED') NOT NULL DEFAULT 'PENDING',
            `upload_note` VARCHAR(500) NULL,
            `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (`proof_id`),
            INDEX `idx_payment_proofs_order_id` (`order_id`),
            INDEX `idx_payment_proofs_uploaded_by` (`uploaded_by`),
            CONSTRAINT `fk_payment_proofs_order`
                FOREIGN KEY (`order_id`) REFERENCES `orders`(`order_id`) ON DELETE CASCADE,
            CONSTRAINT `fk_payment_proofs_uploaded_by`
                FOREIGN KEY (`uploaded_by`) REFERENCES `users`(`user_id`) ON DELETE RESTRICT
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;");
}
