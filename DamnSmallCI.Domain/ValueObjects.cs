using Vogen;

namespace DamnSmallCI.Domain;

[ValueObject<string>]
public partial record struct Step;

[ValueObject<string>]
public partial record struct StepOutput;

[ValueObject<string>]
public partial record struct TaskOutput;

[ValueObject<string>]
public partial record struct PipelineOutput;

[ValueObject<string>]
public partial record struct ImageName;

[ValueObject<Seq<string>>]
public partial record struct ContainerEntrypoint;

[ValueObject<string>]
public partial record struct EnvironmentVariableName;

[ValueObject<string>]
public partial record struct EnvironmentVariableValue;

[ValueObject<Map<EnvironmentVariableName, EnvironmentVariableValue>>]
public partial record struct Environment
{
    public static Environment Empty { get; } = From(Map<EnvironmentVariableName, EnvironmentVariableValue>());
}
