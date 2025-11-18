using Microsoft.Extensions.DependencyInjection;
using Demos.AttributeDemo;

var services = new ServiceCollection();

services.AddTransient<DeleteAccountService>();
services.AddTransient<Handler>();

var provider = services.BuildServiceProvider();

var handler = provider.GetRequiredService<Handler>();

handler.Run();
