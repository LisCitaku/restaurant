using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantPosEnterprise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class UploadsController(IWebHostEnvironment environment) : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    [HttpPost("product-image")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> UploadProductImage([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Choose an image file first." });
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Only .jpg, .jpeg, .png, and .webp files are allowed." });
        }

        var frontendUploadsPath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "Frontend", "uploads"));
        Directory.CreateDirectory(frontendUploadsPath);

        var fileName = $"menu-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(frontendUploadsPath, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream, cancellationToken);

        return Ok(new
        {
            imageUrl = $"/uploads/{fileName}",
            fileName
        });
    }
}
