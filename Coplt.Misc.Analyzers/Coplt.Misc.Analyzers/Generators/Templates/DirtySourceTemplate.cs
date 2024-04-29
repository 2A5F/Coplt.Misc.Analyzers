using System.Collections.Immutable;

namespace Coplt.Miscellaneous.Analysis.Generators.Templates;

public class DirtySourceTemplate(
    GenBase GenBase,
    string Name) : ATemplate(GenBase)
{
    protected override void DoGen()
    {
        sb.AppendLine(GenBase.Target.Code);
        sb.AppendLine("{");

        sb.AppendLine();
        sb.AppendLine($"     public ulong Version() => this._version;");
        sb.AppendLine($"     public void Changed() => this._version++;");
        sb.AppendLine($"     private ulong _version;");

        sb.AppendLine();
        sb.AppendLine("}");
    }
}
