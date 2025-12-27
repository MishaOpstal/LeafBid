using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LeafBidAPI.Filters;

// ReSharper disable once ClassNeverInstantiated.Global
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum) return;
        
        string[] names = Enum.GetNames(context.Type);
        IEnumerable<int> values = Enum.GetValues(context.Type).Cast<int>();

        schema.Description += "<p>Possible values:</p><ul>";
        foreach ((string name, int value) in names.Zip(values, (n, v) => (n, v)))
        {
            schema.Description += $"<li><b>{value}</b> = {name}</li>";
        }
        schema.Description += "</ul>";
    }
}