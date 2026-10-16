using System.IO.Compression;
using System.Text.Json.Nodes;

namespace ScratchCompiler.Decoder;

public class Costume
{
    private readonly (ZipArchive source, string fileName) _source;

    internal Costume(JsonObject json)
    {
        Name = json["name"]?.GetValue<string>() ?? string.Empty;
        FileFormat = json["dataFormat"]?.GetValue<string>() switch
        {
            "svg" => CostumeFormat.SVG,
            "png" => CostumeFormat.PNG,
            _ => throw new ArgumentException($"Unknown data format: {json["dataFormat"]?.GetValue<string>()}"),
        };
        AssetID = json["assetId"]?.GetValue<string>() ?? string.Empty;
        Resolution = json["bitmapResolution"]?.GetValue<int>() ?? 1;
        _source = (Project.Current.archive, json["md5ext"]?.GetValue<string>() ?? string.Empty);
        RotationCenter = (json["rotationCenterX"]?.GetValue<int>() ?? 0, json["rotationCenterY"]?.GetValue<int>() ?? 0);
    }
    
    public string Name { get; }

    public (int x, int y) RotationCenter { get; }

    public CostumeFormat FileFormat { get; }

    // ReSharper disable once InconsistentNaming
    public string AssetID { get; }

    public int Resolution { get; }

    public Stream OpenStream() => _source.source.GetEntry(_source.fileName)!.Open();
}

// ReSharper disable InconsistentNaming
public enum CostumeFormat : byte { SVG, PNG }
// ReSharper restore InconsistentNaming
