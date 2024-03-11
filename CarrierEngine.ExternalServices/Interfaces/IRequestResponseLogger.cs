﻿using System.Threading.Tasks;

namespace CarrierEngine.ExternalServices.Interfaces;

/// <summary>
/// Interface IRequestResponseLogger
/// </summary>
public interface IRequestResponseLogger 
{
    public Task Log<T>(T response);
    public Task SubmitLogs();
}