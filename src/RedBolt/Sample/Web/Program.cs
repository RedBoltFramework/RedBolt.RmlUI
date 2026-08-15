namespace RedBolt.Sample.Web;

using RedBolt.Engine;

internal static class SampleCatalog
{
    internal static Application Create(string applicationTypeName) => applicationTypeName switch
    {
        "RedBolt.Sample.AdvancedUI.AdvancedUIApplication" => new global::RedBolt.Sample.AdvancedUI.AdvancedUIApplication(),
        "RedBolt.Sample.HelloRmlUI.HelloRmlUI" => new global::RedBolt.Sample.HelloRmlUI.HelloRmlUI(),
        _ => throw new System.InvalidOperationException($"Unknown RedBolt sample application: {applicationTypeName}"),
    };
}
