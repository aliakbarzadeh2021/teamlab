﻿using System;
using System.Drawing.Imaging;
using System.Text;
using ASC.Web.UserControls.Bookmarking.Common.Util;
using System.Drawing;

namespace ASC.Web.UserControls.Bookmarking.Common
{
	public class BookmarkingSettings
	{
		public static Guid ModuleId { get; set; }

		public const string BookmarkingSctiptKey = "__boomarking_script_key";

		public const string BookmarkingTagsAutocompleteSctiptKey = "__boomarking_Tags_Autocomplete_script_key";		

		public static int BookmarksCountOnPage = 10;
		public static int VisiblePageCount = 3;

		public static ImageFormat CaptureImageFormat = ImageFormat.Jpeg;

		public static string ThumbnailAbsoluteFilePath { get; set; }

		public static string ThumbnailAbsolutePath { get; set; }

		public static string ThumbnailRelativePath = "Products/Community/Modules/Bookmarking/Data/images/";

		public const string ThumbnailVirtualPath = "~/Products/Community/Modules/Bookmarking/Data/images/";

		public static int Timeout = 180;

		public static readonly BookmarkingThumbnailSize ThumbSmallSize = new BookmarkingThumbnailSize(96, 72);

		public static readonly BookmarkingThumbnailSize ThumbMediumSize = new BookmarkingThumbnailSize(192, 152);

		public static readonly Size BrowserSize = new Size(1280, 1020);
				
		public static Encoding PageTitleEncoding = Encoding.GetEncoding("windows-1251");	

		public static int PingTimeout = 3000;

		public const int NameMaxLength = 255;
		
		public const int DescriptionMaxLength = 65535;

	}
}
