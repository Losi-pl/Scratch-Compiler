using System.IO.Compression;
using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser;

public class Sound
{
    private readonly (ZipArchive source, string fileName) _source;
    
    public Sound(JsonObject json)
    {
        Name = json["name"]?.GetValue<string>() ?? string.Empty;
        AssetID = json["assetId"]?.GetValue<string>() ?? string.Empty;
        
        Rate = json["rate"]?.GetValue<int>() ?? throw new InvalidCastException();
        SampleCount = json["sampleCount"]?.GetValue<int>() ?? throw new InvalidCastException();
        
        Format = json["dataFormat"]?.GetValue<string>() switch
        {
            "wav" => SoundFormat.WAV,
            _ => throw new InvalidDataException()
        };
        
        _source = (Project.Current.archive, json["md5ext"]?.GetValue<string>() ?? string.Empty);
    }


    public string Name { get; }

    // ReSharper disable once InconsistentNaming
    public string AssetID { get; }
    public int Rate { get; }
    public int SampleCount { get; }
    public SoundFormat Format { get; }

    public Stream OpenStream() => _source.source.GetEntry(_source.fileName)!.Open();
}

// ReSharper disable InconsistentNaming
public enum SoundFormat : byte { WAV }
// ReSharper restore InconsistentNaming