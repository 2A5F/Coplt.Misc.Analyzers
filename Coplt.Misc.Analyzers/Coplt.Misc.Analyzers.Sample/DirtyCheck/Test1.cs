using Coplt.Miscellaneous;

namespace Coplt.Misc.Analyzers.Sample.DirtyCheck.Test1;

[DirtySource]
public partial class Class1 { }

[DirtyObserver]
public partial class Class2
{
    [DirtySource]
    public Class1 a = null!;
}
