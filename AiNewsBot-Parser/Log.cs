using Microsoft.Extensions.Logging;

namespace AiNewsBot_Parser;

public static class Log  
{  
    private static readonly ILoggerFactory Factory = LoggerFactory.Create(builder =>  
    {  
        builder.AddSimpleConsole(options =>  
        {  
            options.SingleLine = true;  
            options.TimestampFormat = "HH:mm:ss ";  
        });    });  
    public static ILogger<T> CreateLogger<T>() => Factory.CreateLogger<T>();  
}