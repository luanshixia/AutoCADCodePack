# AutoCAD Code Pack

Previously hosted on [CodePlex](https://acadcodepack.codeplex.com/).

`AutoCAD Code Pack` is a powerful library that helps you to develop AutoCAD plugins using the AutoCAD .NET API. It re-encapsulates the over-designed and old-fashioned classes and methods into easy-to-use static modules and functions. It also brings modern C# syntax like LINQ and lambdas (functional programming) to AutoCAD development. With all the features it provides, you can save over half the lines of your code.

The library was originally developed for AutoCAD R18 (2010, 2011, 2012) and .NET 3.5. We recently updated it to target AutoCAD R23 (2019) and .NET 4.7.1, thanks to the contribution from [@lavantgarde](https://github.com/lavantgarde). You can target other versions with the source files and build yourself.

View [Test.cs](https://github.com/luanshixia/AutoCADCodePack/blob/master/AutoCADCommands/Test.cs) for API usage examples.

## Modules

The library consists of the following modules:

* `Draw` to directly draw entities (with AutoCAD-command-like functions)
* `NoDraw` to create in-memory entities
* `Modify` to edit entities (with AutoCAD-command-like functions)
* `Annotation` to draw annotations
* `DbHelper` to manipulate the DWG database
* `QuickSelection` to simplify entity manipulation with LINQ style coding experience (like jQuery, if you know some Web)
* `Interaction` to handle user interactions
* `Algorithms` to offer some mathematical helpers
* `MultiDoc` to process cross-document scenarios
* `CustomDictionary` to help you attach data to entities
* `SymbolPack` to help you draw symbols like arrows, etc.
* `IronPython` to allow you use IronPython in AutoCAD

## A Quick Look

You may write this elegant code with the code pack:

```csharp
/// <summary>
/// Eliminate zero-length polylines
/// </summary>
[CommandMethod("PolyClean0", CommandFlags.UsePickSet)]
public static void PolyClean0()
{
    var ids = Interaction.GetSelection("\nSelect polyline", "LWPOLYLINE");
    int n = 0;
    ids.QForEach<Polyline>(poly =>
    {
        if (poly.Length == 0)
        {
            poly.Erase();
            n++;
        }
    });
    Interaction.WriteLine("{0} eliminated.", n);
}
```

instead of the verbose version using original API:

```csharp
[CommandMethod("PolyClean0_Old", CommandFlags.UsePickSet)]
public static void PolyClean0_Old()
{
    string message = "\nSelect polyline";
    string allowedType = "LWPOLYLINE";
    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
    PromptSelectionOptions opt = new PromptSelectionOptions { MessageForAdding = message };
    ed.WriteMessage(message);
    SelectionFilter filter = new SelectionFilter(new TypedValue[] { new TypedValue(0, allowedType) });
    PromptSelectionResult res = ed.GetSelection(opt, filter);
    if (res.Status != PromptStatus.OK)
    {
        return;
    }            
    ObjectId[] ids = res.Value.GetObjectIds();
    int n = 0;
    Database db = HostApplicationServices.WorkingDatabase;
    using (Transaction trans = db.TransactionManager.StartTransaction())
    {
        foreach (ObjectId id in ids)
        {
            Polyline poly = trans.GetObject(id, OpenMode.ForWrite) as Polyline;
            if (poly.Length == 0)
            {
                poly.Erase();
                n++;
            }
        }
        trans.Commit();
    }
    ed.WriteMessage("{0} eliminated.", n);
}
```

## Examples

View [Test.cs](https://github.com/luanshixia/AutoCADCodePack/blob/master/AutoCADCommands/Test.cs) for detailed API usage examples.
