using System.Collections.Immutable;

namespace Coplt.Miscellaneous.Analysis.Generators.Templates;

public class DirtyObserverTemplate(
    GenBase GenBase,
    string Name,
    ImmutableArray<string> Sources
) : ATemplate(GenBase)
{
    protected override void DoGen()
    {
        sb.AppendLine(GenBase.Target.Code);
        sb.AppendLine("{");

        #region Fields

        sb.AppendLine();
        sb.AppendLine($"     public ulong Version() => this._version;");
        sb.AppendLine($"     private ulong _version;");

        foreach (var source in Sources)
        {
            sb.AppendLine($"     private ulong _version__{source};");
        }

        #endregion

        #region SyncVersion

        sb.AppendLine();
        sb.AppendLine($"     /// <returns>Is changed</returns>");
        sb.AppendLine($"     private bool SyncVersion()");
        sb.AppendLine($"     {{");
        if (Sources.Length != 0)
        {
            foreach (var source in Sources)
            {
                sb.AppendLine($"         var _v__{source} = this.{source}.Version();");
            }
            foreach (var source in Sources)
            {
                sb.AppendLine($"         if (this._version__{source} != _v__{source}) goto changed;");
            }
            sb.AppendLine($"         goto not_changed;");
            sb.AppendLine($"         changed:");
            foreach (var source in Sources)
            {
                sb.AppendLine($"         this._version__{source} = _v__{source};");
            }
            sb.AppendLine($"         this._version++;");
            sb.AppendLine($"         return true;");
        }
        sb.AppendLine($"         not_changed:");
        sb.AppendLine($"         return false;");
        sb.AppendLine($"     }}");

        #endregion

        sb.AppendLine();
        sb.AppendLine("}");
    }
}
