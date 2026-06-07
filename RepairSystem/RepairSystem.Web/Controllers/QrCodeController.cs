using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Core.Interfaces;

namespace RepairSystem.Web.Controllers;

[Authorize]
public class QrCodeController : Controller
{
    private readonly IQrCodeService _qr;

    public QrCodeController(IQrCodeService qr) => _qr = qr;

    [HttpGet]
    public IActionResult Generate()
    {
        var url = _qr.GetFormUrl();
        if (string.IsNullOrWhiteSpace(url))
            return NotFound();

        var image = _qr.Generate(url);
        return File(image, "image/png");
    }
}
