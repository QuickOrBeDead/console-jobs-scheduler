using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;

namespace ConsoleJobScheduler.Core.Infra.Migration.Conventions;

public sealed class CustomConventionSet : IConventionSet
{
    public CustomConventionSet()
    {
        var innerConventionSet = new DefaultConventionSet();
        ForeignKeyConventions = innerConventionSet.ForeignKeyConventions;
        ColumnsConventions = new List<IColumnsConvention>()
        {
            new CustomPrimaryKeyNameConvention(),
        };
        ConstraintConventions = new List<IConstraintConvention>
        {
            new CustomConstraintNameConvention(),
            innerConventionSet.SchemaConvention,
        };
        IndexConventions = innerConventionSet.IndexConventions;
        SequenceConventions = innerConventionSet.SequenceConventions;
        AutoNameConventions = innerConventionSet.AutoNameConventions;
        SchemaConvention = innerConventionSet.SchemaConvention;
        RootPathConvention = innerConventionSet.RootPathConvention;
    }

    /// <inheritdoc />
    public IRootPathConvention RootPathConvention { get; }

    /// <inheritdoc />
    public DefaultSchemaConvention SchemaConvention { get; }

    /// <inheritdoc />
    public IList<IColumnsConvention> ColumnsConventions { get; }

    /// <inheritdoc />
    public IList<IConstraintConvention> ConstraintConventions { get; }

    /// <inheritdoc />
    public IList<IForeignKeyConvention> ForeignKeyConventions { get; }

    /// <inheritdoc />
    public IList<IIndexConvention> IndexConventions { get; }

    /// <inheritdoc />
    public IList<ISequenceConvention> SequenceConventions { get; }

    /// <inheritdoc />
    public IList<IAutoNameConvention> AutoNameConventions { get; }
}