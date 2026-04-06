using System.IO;
using UnityEngine;

/// <summary>
/// 슬롯 미리보기용 PNG로 <see cref="Sprite"/>를 기록합니다. 아틀라스(읽기 전용 텍스처)도 RT 블릿으로 잘라냅니다.
/// </summary>
public static class SaveThumbnailEncoder
{
    public const int ThumbnailMaxWidth = 480;

    public static bool TryWriteSpriteAsSlotPng(Sprite sprite, string absolutePath)
    {
        if (sprite == null || sprite.texture == null || string.IsNullOrEmpty(absolutePath))
            return false;

        Texture2D region = CopySpriteRegionToTexture2D(sprite);
        if (region == null)
            return false;

        try
        {
            int tw = ThumbnailMaxWidth;
            int th = Mathf.Max(1, Mathf.RoundToInt(region.height * (tw / (float)region.width)));
            Texture2D scaled = ScaleTextureBlit(region, tw, th);
            Object.Destroy(region);
            region = null;
            File.WriteAllBytes(absolutePath, scaled.EncodeToPNG());
            Object.Destroy(scaled);
            return true;
        }
        catch (IOException)
        {
            if (region != null)
                Object.Destroy(region);
            return false;
        }
    }

    static Texture2D CopySpriteRegionToTexture2D(Sprite sprite)
    {
        Texture src = sprite.texture;
        Rect tr = sprite.textureRect;
        int w = (int)tr.width;
        int h = (int)tr.height;
        if (w <= 0 || h <= 0)
            return null;

        RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        RenderTexture prev = RenderTexture.active;
        Graphics.Blit(src, rt);
        RenderTexture.active = rt;
        Texture2D slice = new Texture2D(w, h, TextureFormat.RGB24, false);
        slice.ReadPixels(new Rect(tr.x, tr.y, w, h), 0, 0);
        slice.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return slice;
    }

    static Texture2D ScaleTextureBlit(Texture2D src, int w, int h)
    {
        RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        RenderTexture prev = RenderTexture.active;
        Graphics.Blit(src, rt);
        RenderTexture.active = rt;
        Texture2D dst = new Texture2D(w, h, TextureFormat.RGB24, false);
        dst.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        dst.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return dst;
    }
}
