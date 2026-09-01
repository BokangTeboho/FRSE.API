using System.ComponentModel;
using System.Text;
using NJsonSchema.Generation;

namespace FE.API.Swagger;

public class EnumSchemaFilter : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        var type = context.ContextualType.Type;

        if (!type.IsEnum)
            return;

        var sb = new StringBuilder();

        var descAttr = type
            .GetCustomAttributes(typeof(DescriptionAttribute), false)
            .OfType<DescriptionAttribute>()
            .FirstOrDefault();

        if (descAttr is not null)
            sb.Append(descAttr.Description);

        var names = Enum.GetNames(type);
        if (names.Length > 0)
        {
            if (sb.Length > 0)
                sb.Append("\n\n");

            sb.Append("Available values: ");
            sb.Append(string.Join(", ", names));
        }

        if (sb.Length > 0)
            context.Schema.Description = sb.ToString();
    }
}
