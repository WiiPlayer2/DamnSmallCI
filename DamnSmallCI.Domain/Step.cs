﻿using Vogen;

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
