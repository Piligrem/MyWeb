namespace InSearch.Core.Domain.Media
{
    /// <summary>
    /// Represents a picture item type
    /// </summary>
    public enum PictureType : int
    {
        /// <summary>
        /// Entities (products, categories, manufacturers)
        /// </summary>
        entity = 1,
        /// <summary>
        /// Avatar
        /// </summary>
        avatar = 10,
        /// <summary>
        /// Photo client
        /// </summary>
        photo = 2,
        /// <summary>
        /// Marker device
        /// </summary>
        marker = 3
    }

    public enum ThumbnailScaleMode
    {
        Auto,
        UseWidth,
        UseHeight
    }
}
