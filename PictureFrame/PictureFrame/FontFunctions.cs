
namespace PictureFrame
{
    /// <summary>
    /// 与字体有关的功能存放于此
    /// </summary>
    public static class FontFunctions
    {
        /// <summary>
        /// 根据FontFamily获取可用的FontStyle列表
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <returns></returns>
        public static List<FontStyle> FontStyleAvailable(FontFamily fontFamily)
        {
            List<FontStyle> fontStyleList = new List<FontStyle> {};
            foreach (FontStyle fs in Enum.GetValues(typeof(FontStyle)))
            {
                if (fontFamily.IsStyleAvailable(fs))
                {
                    fontStyleList.Add(fs);
                }
            }
            return fontStyleList;
        }
    }
}
