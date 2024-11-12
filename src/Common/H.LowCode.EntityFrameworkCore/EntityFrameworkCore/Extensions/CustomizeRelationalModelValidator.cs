using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace H.LowCode.EntityFrameworkCore;

/// <summary>
/// 重写ModelValidator 确保表可以重复注册
/// </summary>
#pragma warning disable EF1001 // Internal EF Core API usage.
internal class CustomizeRelationalModelValidator : SqlServerModelValidator
#pragma warning restore EF1001 // Internal EF Core API usage.
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationalModelValidator" />.
    /// </summary>
    /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this service. </param>
    public CustomizeRelationalModelValidator(
        ModelValidatorDependencies dependencies, RelationalModelValidatorDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <summary>
    ///     Validates the mapping/configuration of shared tables in the model.
    /// </summary>
    /// <param name="model"> The model to validate. </param>
    /// <param name="logger"> The logger to use. </param>
    protected override void ValidateSharedTableCompatibility(
        IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {

    }

    protected override void ValidateClrInheritance(
        [NotNull] IModel model, [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {

    }
}