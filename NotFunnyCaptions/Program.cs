using System.CommandLine;
using NotFunnyCaptions;

var inputOption = new Option<FileInfo>(new[] { "--input", "-i" }, "The input file to process.");
var outputOption = new Option<FileInfo>(new[] { "--output", "-o" }, "The output file to write.");
var fontOption = new Option<FileInfo>(new[] { "--font", "-f" },
    () => new FileInfo(".\\Fonts\\Futura_Condensed_Extra_Bold.otf"), "The font to use for the captions.");

var captionOption = new Option<string>(new []{"--caption", "-c"}, "The caption to use.");

var rootCommand = new RootCommand("A command line tool to add a caption to image.");
rootCommand.AddOption(inputOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(fontOption);
rootCommand.AddOption(captionOption);

var generator = new Generator();

rootCommand.SetHandler((input, output, font, caption) => generator.Generate(input, output, font, caption), inputOption,
    outputOption, fontOption, captionOption);


return rootCommand.InvokeAsync(args).Result;
