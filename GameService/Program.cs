using GameService;
using log4net.Config;

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
XmlConfigurator.Configure(new FileInfo("log4net.config"));

ConsoleStart.RunGameConsole();