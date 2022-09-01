using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NotFunnyCaptions;

public class Generator
{
    private readonly FontCollection _fonts = new();

    public void Generate(FileInfo input, FileInfo output, FileInfo font, string caption)
    {
        caption = caption
            .Replace("\\n", "\n")
            .Replace("\\\\", "\\");

        var fontFamily = _fonts.Add(font.FullName);
        using var image = Image.Load(input.FullName);
        using var result = GenerateTheSequel(fontFamily, image, caption);
        result.Save(output.FullName);
    }

    private static Image GenerateTheSequel(FontFamily fontFamily, Image inputImage, string caption)
    {
        var fontSize = inputImage.Height * 0.15f;

        var lines = caption.Split('\n');

        Font font;
        IPathCollection[] glyphs;

        do
        {
            font = fontFamily.CreateFont(fontSize);

            glyphs = lines.Select(line => TextBuilder.GenerateGlyphs(line, new TextOptions(font)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }) ?? throw new NullReferenceException()).ToArray();

            fontSize *= 0.95f;
        } while (glyphs.Any(g => g.Bounds.Width > inputImage.Width * 0.7));

        var padding = glyphs.First().Bounds.Height * 0.5f;

        var captionBarHeight =
            (int)(glyphs.Aggregate(0.0f, (v, g) => v + g.Bounds.Height) + padding * 2);

        var height = inputImage.Height + captionBarHeight;

        var outputImage = new Image<Rgba32>(inputImage.Width, height);
        var count = inputImage.Frames.Count;

        var inputImageMetadata = inputImage.Metadata.GetGifMetadata();
        var outputImageMetadata = outputImage.Metadata.GetGifMetadata();

        outputImageMetadata.Comments = inputImageMetadata.Comments;
        outputImageMetadata.RepeatCount = inputImageMetadata.RepeatCount;
        outputImageMetadata.ColorTableMode = inputImageMetadata.ColorTableMode;
        outputImageMetadata.GlobalColorTableLength = inputImageMetadata.GlobalColorTableLength;

        for (var i = 0; i < count; i++)
        {
            var tempImage = new Image<Rgba32>(inputImage.Width, height);
            tempImage.Mutate(ctx =>
            {
                ctx
                    .Fill(Color.White)
                    .DrawImage(inputImage, new Point(0, captionBarHeight), 1);

                var y = padding;

                for (var j = 0; j < lines.Length; j++)
                {
                    ctx.DrawText(lines[j], font, Color.Black,
                        new PointF(inputImage.Width / 2.0f - glyphs[j].Bounds.Width / 2,
                            y));

                    y += glyphs[j].Bounds.Height;
                }
            });

            outputImage.Frames.AddFrame(tempImage.Frames[0]);

            var inputMetadata = inputImage.Frames[0].Metadata.GetGifMetadata();
            var outputMetadata = outputImage.Frames[^1].Metadata.GetGifMetadata();

            outputMetadata.DisposalMethod = inputMetadata.DisposalMethod;
            outputMetadata.FrameDelay = inputMetadata.FrameDelay;
            outputMetadata.ColorTableLength = inputMetadata.ColorTableLength;

            if (i < count - 1)
                inputImage.Frames.RemoveFrame(0);
        }

        outputImage.Frames.RemoveFrame(0);
        return outputImage;
    }
}