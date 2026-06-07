namespace RepairSystem.Core.Interfaces;

public interface IQrCodeService
{
    string GetFormUrl();
    byte[] Generate(string url);
}
