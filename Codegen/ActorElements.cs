namespace ScratchCompiler.Codegen;

internal class ActorElements(ScriptData[] scripts, TargetEvents events, EmbeddedFile[] costumes)
{
    public ScriptData[] Scripts = scripts;
    public TargetEvents Events = events;
    public EmbeddedFile[] Costumes = costumes;
}