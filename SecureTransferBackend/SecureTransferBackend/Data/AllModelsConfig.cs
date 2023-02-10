using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SecureTransferBackend.Data.Common;
using SecureTransferBackend.Services.Transfer.Models;

namespace SecureTransferBackend.Data;


public class AllModelsConfig
{
    public readonly List<object> allModels = new List<object>()
    {
        typeof(Bundle)
    };
}