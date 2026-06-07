using Microsoft.EntityFrameworkCore;
using QRCoder;
using RepairSystem.Core.Data;
using RepairSystem.Core.Interfaces;

namespace RepairSystem.Core.Services;

public class QrCodeService : IQrCodeService
{
    private readonly AppDbContext _db;

    public QrCodeService(AppDbContext db) => _db = db;

    public string GetFormUrl() =>
        _db.SystemSettings.FirstOrDefault(s => s.Key == "QrCodeFormUrl")?.Value ?? "";

    public byte[] Generate(string url)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using var code = new PngByteQRCode(data);
        return code.GetGraphic(10);
    }
}
