using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

var dataProtectionBuilder = builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(
        new DirectoryInfo(
            Path.Combine(builder.Environment.ContentRootPath, "Keys")
        )
    )
    .SetApplicationName("DataProtectionDemo");
if (OperatingSystem.IsWindows())
{
    dataProtectionBuilder.ProtectKeysWithDpapi();
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapPost("/protect", (ProtectRequest request, IDataProtectionProvider provider) =>
{
    IDataProtector protector = provider.CreateProtector("DataProtectionDemo.UserData");

    var protectedText = protector.Protect(request.Text);

    return Results.Ok(new { OriginalText = request.Text, ProtectedText = protectedText });
});
app.MapPost("/unprotect", (UnprotectRequest request, IDataProtectionProvider provider) =>
{
    IDataProtector protector = provider.CreateProtector("DataProtectionDemo.UserData");

    try
    {
        var originalText = protector.Unprotect(request.ProtectedText);
        return Results.Ok(new { OriginalText = originalText });
    }
    catch (CryptographicException)
    {
        return Results.BadRequest("Invalid or tampered protected data!");
    }
});
app.Run();

record ProtectRequest(string Text);
record UnprotectRequest(string ProtectedText);