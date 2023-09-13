using System.Drawing;

namespace PictureFrame
{
    /// <summary>
    /// 生成一张预览所需的参数
    /// </summary>
    public class PreviewGeneClass
    {
        //定义成类而非构造体，是因为struct不允许定义无参数构造函数，有参数构造函数构造过于繁复

        //单参数构造函数，仅传入原始图像
        //初始化时，默认信息条在图像下方
        public PreviewGeneClass( Bitmap bm )
        {
            //原始图像
            bitmap = bm;

            //信息条参数
            stripLocation = InfoStripLocations.Beneath;
            stripSize = 0.04f;

            //logo参数，默认占strip1/2高，居左
            brandIndex = null;
            logoSize = 0.5f;
            logoX = 0.00f;
            logoY = 0.50f;

            //time参数，默认占strip1/3高，居右
            time = "";
            timeSize = 0.33f;
            timeFontStyle = FontStyle.Regular; 
            timeFont = new FontFamily("Arial");
            timeX = 1.00f;
            timeY = 0.50f;
            timeTranParent = 255;

            //info参数
            info = "";
            infoSize = 0.33f;
            infoFontStyle = FontStyle.Regular;
            infoFontFamily = new FontFamily("Arial");
            infoX = 0.50f;
            infoY = 0.50f;
            infoTransparent = 255;

            //外框参数
            marginSize = 0.00f;
        }


        /// <summary>
        /// 原始图像数据
        /// </summary>
        public Bitmap bitmap;

        /// <summary>
        /// 信息条位置
        /// </summary>
        public InfoStripLocations stripLocation;

        /// <summary>
        /// 信息条相对尺寸，如果在图像下方，则信息条实际尺寸为stripSize/原始图像高
        /// </summary>
        public float stripSize;

        /// <summary>
        /// 品牌编号，允许为null
        /// 图像加载后，无brandIndex信息，待选择Brand之后，为图像添加对应Logo
        /// </summary>
        public int? brandIndex;

        /// <summary>
        /// logo大小(Logo尺寸/信息条尺寸)
        /// </summary>
        public float logoSize;

        /// <summary>
        /// logo横坐标
        /// </summary>
        public float logoX;

        /// <summary>
        /// logo纵坐标
        /// </summary>
        public float logoY;

        /// <summary>
        /// 拍摄时间信息
        /// </summary>
        public string time;

        /// <summary>
        /// 拍摄时间字体样式
        /// </summary>
        public FontFamily timeFont;

        /// <summary>
        /// 拍摄时间文字透明度
        /// </summary>
        public int timeTranParent;

        /// <summary>
        /// 拍摄时间字体样式
        /// </summary>
        public FontStyle timeFontStyle;

        /// <summary>
        /// 拍摄时间横坐标
        /// </summary>
        public float timeX;

        /// <summary>
        /// 拍摄时间纵坐标
        /// </summary>
        public float timeY;

        /// <summary>
        /// 拍摄时间字号
        /// </summary>
        public float timeSize;

        /// <summary>
        /// 图像信息字体
        /// </summary>
        public FontFamily infoFontFamily;

        /// <summary>
        /// 图像信息字体样式
        /// </summary>
        public FontStyle infoFontStyle;

        /// <summary>
        /// 参数文字透明度
        /// </summary>
        public int infoTransparent;

        /// <summary>
        /// 图像信息文字
        /// </summary>
        public string info;

        /// <summary>
        /// 图像信息横坐标
        /// </summary>
        public float infoX;

        /// <summary>
        /// 图像信息纵坐标
        /// </summary>
        public float infoY;

        /// <summary>
        /// 图像信息字号
        /// </summary>
        public float infoSize;

        /// <summary>
        /// 外框大小
        /// </summary>
        public float marginSize;


    }
}
