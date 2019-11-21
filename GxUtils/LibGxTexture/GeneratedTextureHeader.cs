namespace LibGxTexture
{
    public struct GeneratedTextureHeader
    {
        public GxTextureFormat textureFormat;
        public int textureCount;
        public int textureWidth;
        public int textureHeight;
        public int textureMipmapCount;

        public GeneratedTextureHeader(GxTextureFormat textureFormat, int textureCount, int textureWidth, int textureHeight, int textureMipmapCount)
        {
            this.textureFormat = textureFormat;
            this.textureCount = textureCount;
            this.textureWidth = textureWidth;
            this.textureHeight = textureHeight;
            this.textureMipmapCount = textureMipmapCount;
        }
    }
}
