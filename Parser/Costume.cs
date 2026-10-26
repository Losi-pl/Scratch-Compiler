using System.IO.Compression;
using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser;

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
            "jpg" => CostumeFormat.JPG,
            _ => throw new ArgumentException($"Unknown data format: {json["dataFormat"]?.GetValue<string>()}"),
        };
        AssetID = json["assetId"]?.GetValue<string>() ?? string.Empty;
        Resolution = json["bitmapResolution"]?.GetValue<int>() ?? 1;
        _source = (Project.Current.archive, json["md5ext"]?.GetValue<string>() ?? string.Empty);
        RotationCenter = (json["rotationCenterX"]?.GetValue<float>() ?? 0, json["rotationCenterY"]?.GetValue<float>() ?? 0);
    }
    
    public string Name { get; }

    public (float x, float y) RotationCenter { get; }

    public CostumeFormat FileFormat { get; }

    // ReSharper disable once InconsistentNaming
    public string AssetID { get; }

    public int Resolution { get; }

    public Stream OpenStream() => _source.source.GetEntry(_source.fileName)!.Open();
}

// ReSharper disable InconsistentNaming
public enum CostumeFormat : byte { SVG, PNG, JPG }
// ReSharper restore InconsistentNaming
