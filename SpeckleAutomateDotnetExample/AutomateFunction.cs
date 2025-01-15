using Objects;
using Objects.Geometry;
using Objects.Primitive;
using Speckle.Automate.Sdk;
using Speckle.Core.Models;
using Speckle.Core.Models.Extensions;

public static class AutomateFunction
{
  public static async Task Run(
    AutomationContext automationContext,
    FunctionInputs functionInputs
  )
  {
    _ = typeof(ObjectsKit).Assembly; // INFO: Force objects kit to initialize

    // Initialize base object for model that we're creating
    var model = new Base();
    var boxes = new List<Box>();

    // Get model that triggered function
    var commitObject = await automationContext.ReceiveVersion();

    // Generate some arbitrary geometry to show (could be anything!)
    foreach (var obj in commitObject.Flatten())
    {
      switch (obj)
      {
        case Mesh mesh:
        {
          // Create a box at the center of each mesh
          var bbox = mesh.bbox;
          var origin = bbox.basePlane.origin;

          var newBox = new Box(
            basePlane: bbox.basePlane,
            xSize: new Interval(origin.x - 1, origin.x + 1),
            ySize: new Interval(origin.y - 1, origin.y + 1),
            zSize: new Interval(origin.z - 1, origin.z + 1)
          );

          boxes.Add(newBox);
          break;
        }
        default:
        {
          Console.WriteLine(obj);
          break;
        }
      }
    }

    // Store generated geometry in model
    model["boxes"] = boxes;

    // Send the geometry to the target model
    var commitId = automationContext
      .CreateNewVersionInProject(
        model,
        functionInputs.DestinationModel,
        "Data from Speckle Automate"
      )
      .Result;

    Console.WriteLine(
      $"Commit {commitId} created in project model {functionInputs.DestinationModel}"
    );

    automationContext.MarkRunSuccess($"Added {boxes.Count} boxes!");
  }
}
