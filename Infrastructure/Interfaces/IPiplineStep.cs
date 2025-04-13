using System;

namespace Infrastructure.Interfaces;

public interface IPipelineStep<T>
{
    T Process(T context);
}
