using InSearch.Core.Configuration;

namespace InSearch.Core.Domain.Media
{
    public class MediaSettings : ISettings
    {
		public MediaSettings()
		{
			AvatarPictureSize = 85;
			ProductThumbPictureSize = 100;
			ProductDetailsPictureSize = 300;
			ProductThumbPictureSizeOnProductDetailsPage = 70;
			AssociatedProductPictureSize = 125;
			BundledProductPictureSize = 70;
			CategoryThumbPictureSize = 125;
			ManufacturerThumbPictureSize = 125;
			CartThumbPictureSize = 80;
			CartThumbBundleItemPictureSize = 32;
			MiniCartThumbPictureSize = 32;
			AutoCompleteSearchThumbPictureSize = 20;
			MaximumImageSize = 1280;
			DefaultPictureZoomEnabled = true;
			PictureZoomType = "window";
			DefaultImageQuality = 90;
			MultipleThumbDirectories = true;
            UserPhotoSize = 76;
            UserPhotoBigSize = 200;
            DeviceMarkerMaxSize = 64;
		}

        public int UserPhotoSize { get; set; }
        public int UserPhotoBigSize { get; set; }
        public int DeviceMarkerMaxSize { get; set; }
        public string MarkersPath { get; set; }
		public int AvatarPictureSize { get; set; }
        public int ProductThumbPictureSize { get; set; }
        public int ProductDetailsPictureSize { get; set; }
        public int ProductThumbPictureSizeOnProductDetailsPage { get; set; }
        public int AssociatedProductPictureSize { get; set; }
		public int BundledProductPictureSize { get; set; }
        public int CategoryThumbPictureSize { get; set; }
        public int ManufacturerThumbPictureSize { get; set; }
        public int CartThumbPictureSize { get; set; }
		public int CartThumbBundleItemPictureSize { get; set; }
        public int MiniCartThumbPictureSize { get; set; }
        public int AutoCompleteSearchThumbPictureSize { get; set; }

        public bool DefaultPictureZoomEnabled { get; set; }
        public string PictureZoomType { get; set; }

        public int MaximumImageSize { get; set; }

        /// <summary>
        /// Gets or sets a default quality used for image generation
        /// </summary>
        public int DefaultImageQuality { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether single (/media/thumbs/) or multiple (/media/thumbs/0001/ and /media/thumbs/0002/) directories will used for picture thumbs
        /// </summary>
        public bool MultipleThumbDirectories { get; set; }
    }
}