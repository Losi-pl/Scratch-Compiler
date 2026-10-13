using System.IO.Compression;
using System.Text.Json.Nodes;

namespace ScratchCompiler.Decoder;

public class Costume
{
    private string _name;
    private (int x, int y) _rotationCenter;
    private CostumeFormat _format;
    private string _assetID;
    private int _resolution;
    private (ZipArchive source, string fileName) _source;

    internal Costume(JsonObject json)
    {
        _name = json["name"]?.GetValue<string>() ?? string.Empty;
        _format = json["dataFormat"]?.GetValue<string>() switch
        {
            "svg" => CostumeFormat.SVG,
            "png" => CostumeFormat.PNG,
            _ => throw new ArgumentException($"Unknown data format: {json["dataFormat"]?.GetValue<string>()}"),
        };
        _assetID = json["assetId"]?.GetValue<string>() ?? string.Empty;
        _resolution = json["bitmapResolution"]?.GetValue<int>() ?? 1;
        _source = (Project._currentProject.archive, json["md5ext"]?.GetValue<string>() ?? string.Empty);
    }
}

// ReSharper disable InconsistentNaming
public enum CostumeFormat : byte { SVG, PNG }
// ReSharper restore InconsistentNaming
