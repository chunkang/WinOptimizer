namespace WinOptimizer.Data;

public record KnownSoftwareEntry(string FriendlyName, string[] MatchPatterns);

public static class KnownSoftwareDatabase
{
    public static IReadOnlyList<KnownSoftwareEntry> Entries { get; } = new List<KnownSoftwareEntry>
    {
        new("AhnLab Safe Transaction", new[] { "AhnLab Safe Transaction", "ASTx" }),
        new("nProtect Online Security", new[] { "nProtect Online Security", "nProtect Netizen", "nProtect KeyCrypt" }),
        new("INISAFE CrossWeb EX", new[] { "INISAFE CrossWeb", "INISAFE", "Initech" }),
        new("TouchEn nxKey", new[] { "TouchEn nxKey", "TouchEn" }),
        new("KSign", new[] { "KSign" }),
        new("XecureWeb", new[] { "XecureWeb", "XecureExpress" }),
        new("MagicLine4NX", new[] { "MagicLine4NX", "MagicLine" }),
        new("KTBNet", new[] { "KTBNet" }),
        new("AnySign4PC", new[] { "AnySign4PC" }),
        new("AnySign4PC EX", new[] { "AnySign4PC EX" }),
        new("CrossCert SecureTool", new[] { "CrossCert" }),
        new("Wizvera VeraPort", new[] { "Wizvera", "VeraPort", "Veraport" }),
        new("DreamSecurity SecureWeb", new[] { "DreamSecurity SecureWeb", "DreamSecurity" }),
        new("DreamCert", new[] { "DreamCert" }),
        new("SGA SGNAC", new[] { "SGNAC", "SGV3", "SGA SecureDoc" }),
        new("SoftCamp Secure KeyStroke", new[] { "SoftCamp", "Secure KeyStroke" }),
        new("UbiKey", new[] { "UbiKey", "Ubitech" }),
        new("SoftForum SafeGuard", new[] { "SoftForum SafeGuard", "SoftForum" }),
        new("eWiz", new[] { "eWiz" }),
        new("eSign", new[] { "eSign" }),
        new("AlphaShield", new[] { "AlphaShield", "AlphaSecure" }),
        new("EpageSAFER", new[] { "EpageSAFER" }),
        new("TrustZone", new[] { "TrustZone" }),
        new("MarkAny", new[] { "MarkAny" }),
        new("AnyBank", new[] { "AnyBank" }),
        new("Coscom SignKorea", new[] { "Coscom", "SignKorea" }),
        new("SCGuard", new[] { "SCGuard" }),
        new("SsenStone", new[] { "SsenStone" }),
        new("WESS SignEx", new[] { "WESS SignEx", "WESS" }),
        new("McAfee Secure Bank", new[] { "McAfee Secure Bank" }),
        new("TrendMicro PC Web Security", new[] { "TrendMicro PC Web Security" }),
        new("Nexess NexGuard", new[] { "Nexess", "NexGuard" }),
        new("INCA nProtect Netizen", new[] { "nProtect Netizen" }),
        new("INCA nProtect KeyCrypt", new[] { "nProtect KeyCrypt" }),
        new("Hancom GPKI Tool", new[] { "Hancom GPKI", "GPKI" }),
        new("RaonSecure", new[] { "RaonSecure" }),
    };
}
