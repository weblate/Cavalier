using SkiaSharp;
using System;
using System.Linq;

namespace NickvisionCavalier.Shared.Models;

public class Renderer
{
    private delegate void DrawFunc(float[] sample, DrawingDirection direction, float x, float y, float width, float height, SKPaint paint);
    private DrawFunc? _drawFunc;

    public SKCanvas? Canvas { get; set; }
    
    public Renderer()
    {
        Canvas = null;
    }
    
    public void Draw(float[] sample, float width, float height)
    {
        if (Canvas == null)
        {
            return;
        }
        Canvas.Clear();
        var fgPaint = new SKPaint
        {
            Style = Configuration.Current.Filling ? SKPaintStyle.Fill : SKPaintStyle.Stroke,
            StrokeWidth = Configuration.Current.LinesThickness,
            Color = SKColors.Blue
        };
        _drawFunc = Configuration.Current.Mode switch
        {
            DrawingMode.LevelsBox => DrawLevelsBox,
            DrawingMode.ParticlesBox => DrawParticlesBox,
            DrawingMode.BarsBox => DrawBarsBox,
            DrawingMode.SpineBox => DrawSpineBox,
            _ => DrawWaveBox,
        };
        if (Configuration.Current.Mirror == Mirror.Full)
        {
            _drawFunc(sample, Configuration.Current.Direction, 0, 0, GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
            _drawFunc(sample, GetMirrorDirection(), GetMirrorX(width), GetMirrorY(height), GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
        }
        else if (Configuration.Current.Mirror == Mirror.SplitChannels)
        {
            _drawFunc(sample.Take(sample.Length / 2).ToArray(), Configuration.Current.Direction, 0, 0, GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
            _drawFunc(sample.Skip(sample.Length / 2).Reverse().ToArray(), GetMirrorDirection(), GetMirrorX(width), GetMirrorY(height), GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
        }
        else
        {
            _drawFunc(sample, Configuration.Current.Direction, 0, 0, width, height, fgPaint);
        }
        Canvas.Flush();
    }

    private DrawingDirection GetMirrorDirection()
    {
        return Configuration.Current.Direction switch
        {
            DrawingDirection.TopBottom => DrawingDirection.BottomTop,
            DrawingDirection.BottomTop => DrawingDirection.TopBottom,
            DrawingDirection.LeftRight => DrawingDirection.RightLeft,
            _ => DrawingDirection.LeftRight
        };
    }

    private float GetMirrorX(float width)
    {
        if (Configuration.Current.Direction == DrawingDirection.LeftRight || Configuration.Current.Direction == DrawingDirection.RightLeft)
        {
            return width / 2.0f;
        }
        return 0;
    }

    private float GetMirrorY(float height)
    {
        if (Configuration.Current.Direction == DrawingDirection.TopBottom || Configuration.Current.Direction == DrawingDirection.BottomTop)
        {
            return height / 2.0f;
        }
        return 0;
    }

    private float GetMirrorWidth(float width)
    {
        if (Configuration.Current.Direction == DrawingDirection.LeftRight || Configuration.Current.Direction == DrawingDirection.RightLeft)
        {
            return width / 2.0f;
        }
        return width;
    }

    private float GetMirrorHeight(float height)
    {
        if (Configuration.Current.Direction == DrawingDirection.TopBottom || Configuration.Current.Direction == DrawingDirection.BottomTop)
        {
            return height / 2.0f;
        }
        return height;
    }

    private void DrawWaveBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / (sample.Length - 1);
        using var path = new SKPath();
        switch (direction)
        {
            case DrawingDirection.TopBottom:
                path.MoveTo(x, y + height * sample[0] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2));
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        x + step * (i + 0.5f),
                        y + height * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        x + step * (i + 0.5f),
                        y + height * sample[i+1] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        x + step * (i + 1),
                        y + height * sample[i+1] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2));
                }
                if (Configuration.Current.Filling)
                {
                    path.LineTo(x + width, y);
                    path.LineTo(x, y);
                    path.Close();
                }
                break;
            case DrawingDirection.BottomTop:
                path.MoveTo(x, y + height * (1 - sample[0]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2));
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        x + step * (i + 0.5f),
                        y + height * (1 - sample[i]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        x + step * (i + 0.5f),
                        y + height * (1 - sample[i+1]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        x + step * (i + 1),
                        y + height * (1 - sample[i+1]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2));
                }
                if (Configuration.Current.Filling)
                {
                    path.LineTo(x + width, y + height);
                    path.LineTo(x, y + height);
                    path.Close();
                }
                break;
            case DrawingDirection.LeftRight:
                path.MoveTo(x + width * sample[0] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2), y);
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        x + width * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + 0.5f),
                        x + width * sample[i+1] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + 0.5f),
                        x + width * sample[i+1] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + 1));
                }
                if (Configuration.Current.Filling)
                {
                    path.LineTo(x, y + height);
                    path.LineTo(x, y);
                    path.Close();
                }
                break;
            case DrawingDirection.RightLeft:
                path.MoveTo(x + width * (1 - sample[0]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2), y);
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        x + width * (1 - sample[i]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + 0.5f),
                        x + width * (1 - sample[i+1]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + 0.5f),
                        x + width * (1 - sample[i+1]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + 1));
                }
                if (Configuration.Current.Filling)
                {
                    path.LineTo(x + width, y + height);
                    path.LineTo(x + width, y);
                    path.Close();
                }
                break;
        }
        Canvas.DrawPath(path, paint);
    }

    private void DrawLevelsBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        var itemWidth = (direction < DrawingDirection.LeftRight ? step : width / 10) * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2);
        var itemHeight = (direction < DrawingDirection.LeftRight ? height / 10 : step) * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2);
        for (var i = 0; i < sample.Length; i++)
        {
            for (var j = 0; j < Math.Floor(sample[i] * 10); j++)
            {
                switch (direction)
                {
                    case DrawingDirection.TopBottom:
                        Canvas.DrawRoundRect(
                            x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            y + height / 10 * j + height / 10 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            itemWidth, itemHeight,
                            itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                            paint);
                        break;
                    case DrawingDirection.BottomTop:
                        Canvas.DrawRoundRect(
                            x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            y + height / 10 * (9 - j) + height / 10 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            itemWidth, itemHeight,
                            itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                            paint);
                        break;
                    case DrawingDirection.LeftRight:
                        Canvas.DrawRoundRect(
                            x + width / 10 * j + width / 10 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            itemWidth, itemHeight,
                            itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                            paint);
                        break;
                    case DrawingDirection.RightLeft:
                        Canvas.DrawRoundRect(
                            x + width / 10 * (9 - j) + width / 10 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            itemWidth, itemHeight,
                            itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                            paint);
                        break;
                }
            }
        }
    }

    private void DrawParticlesBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        var itemWidth = (direction < DrawingDirection.LeftRight ? step : width / 11) * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2);
        var itemHeight = (direction < DrawingDirection.LeftRight ? height / 11 : step) * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2);
        for (var i = 0; i < sample.Length; i++)
        {
            switch (direction)
            {
                case DrawingDirection.TopBottom:
                    Canvas.DrawRoundRect(
                        x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + height / 11 * 10 * sample[i] + height / 11 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        itemWidth, itemHeight,
                        itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                        paint);
                    break;
                case DrawingDirection.BottomTop:
                    Canvas.DrawRoundRect(
                        x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + height / 11 * 10 * (1 - sample[i]) + height / 11 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        itemWidth, itemHeight,
                        itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                        paint);
                    break;
                case DrawingDirection.LeftRight:
                    Canvas.DrawRoundRect(
                        x + width / 11 * 10 * sample[i] + width / 11 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        itemWidth, itemHeight,
                        itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                        paint);
                    break;
                case DrawingDirection.RightLeft:
                    Canvas.DrawRoundRect(
                        x + width / 11 * 10 * (1 - sample[i]) + width / 11 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        itemWidth, itemHeight,
                        itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                        paint);
                    break;
            }
        }
    }

    private void DrawBarsBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        for (var i = 0; i < sample.Length; i++)
        {
            if (sample[i] == 0)
            {
                continue;
            }
            switch (direction)
            {
                case DrawingDirection.TopBottom:
                    Canvas.DrawRect(
                        x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        Configuration.Current.Filling ? y : y + Configuration.Current.LinesThickness / 2,
                        step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        height * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        paint);
                    break;
                case DrawingDirection.BottomTop:
                    Canvas.DrawRect(
                        x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + height * (1 - sample[i]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        height * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        paint);
                    break;
                case DrawingDirection.LeftRight:
                    Canvas.DrawRect(
                        Configuration.Current.Filling ? x : x + Configuration.Current.LinesThickness / 2,
                        y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        width * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        paint);
                    break;
                case DrawingDirection.RightLeft:
                    Canvas.DrawRect(
                        x + width * (1 - sample[i]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        width * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        paint);
                    break;
            };
        }
    }

    private void DrawSpineBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        var itemSize = step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness);
        for (var i = 0; i < sample.Length; i++)
        {
            if (sample[i] == 0)
            {
                continue;
            }
            switch (direction)
            {
                case DrawingDirection.TopBottom:
                case DrawingDirection.BottomTop:
                    Canvas.DrawRoundRect(
                        x + step * (i + 0.5f) + (1 - itemSize * sample[i]) / 2,
                        y + height / 2 - itemSize * sample[i] / 2,
                        itemSize * sample[i], itemSize * sample[i],
                        itemSize * sample[i] / 2 * Configuration.Current.ItemsRoundness, itemSize * sample[i] / 2 * Configuration.Current.ItemsRoundness,
                        paint);
                    break;
                case DrawingDirection.LeftRight:
                case DrawingDirection.RightLeft:
                    Canvas.DrawRoundRect(
                        x + width / 2 - itemSize * sample[i] / 2,
                        y + step * (i + 0.5f) + (1 - itemSize * sample[i]) / 2,
                        itemSize * sample[i], itemSize * sample[i],
                        itemSize * sample[i] / 2 * Configuration.Current.ItemsRoundness, itemSize * sample[i] / 2 * Configuration.Current.ItemsRoundness,
                        paint);
                    break;
            }
        }
    }
}
