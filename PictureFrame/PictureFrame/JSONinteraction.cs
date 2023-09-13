using Newtonsoft.Json;
using System.Data;
using System.Resources.Extensions;

namespace PictureFrame
{
    /// <summary>
    /// 存放json交互功能
    /// </summary>
    static class JSONinteractions
    {
        /// <summary>
        /// json文件的序列化
        /// </summary>
        /// <param name="previewGeneClass"></param>
        /// <param name="path">json文件路径</param>
        public static void Parameters2Json(PreviewGeneClass previewGeneClass, string path)
        {
            //创建字典
            DataTable dt = new DataTable();

            dt.Columns.Add("stripLocation", typeof(InfoStripLocations));
            dt.Columns.Add("stripSize", typeof(float));

            dt.Columns.Add("logoSize", typeof(float));
            dt.Columns.Add("logoX", typeof(float));
            dt.Columns.Add("logoY", typeof(float));

            dt.Columns.Add("timeSize", typeof(float));
            dt.Columns.Add("timeFontStyle", typeof(FontStyle));
            dt.Columns.Add("timeFont", typeof(String)); //字体参数用String格式存储
            dt.Columns.Add("timeX", typeof(float));
            dt.Columns.Add("timeY", typeof(float));
            dt.Columns.Add("timeTranParent", typeof(int));

            dt.Columns.Add("infoSize", typeof(float));
            dt.Columns.Add("infoFontStyle", typeof(FontStyle));
            dt.Columns.Add("infoFontFamily", typeof(String)); //字体参数用String格式存储
            dt.Columns.Add("infoX", typeof(float));
            dt.Columns.Add("infoY", typeof(float));
            dt.Columns.Add("infoTransparent", typeof(int));

            dt.Columns.Add("marginSize", typeof(float));

            DataRow dr = dt.NewRow();

            dr["stripLocation"] = previewGeneClass.stripLocation;
            dr["stripSize"] = previewGeneClass.stripSize;

            dr["logoSize"] = previewGeneClass.logoSize;
            dr["logoX"] = previewGeneClass.logoX;
            dr["logoY"] = previewGeneClass.logoY;

            dr["timeSize"] = previewGeneClass.timeSize;
            dr["timeFontStyle"] = previewGeneClass.timeFontStyle;
            dr["timeFont"] = previewGeneClass.timeFont.Name; //字体参数用String格式存储
            dr["timeX"] = previewGeneClass.timeX;
            dr["timeY"] = previewGeneClass.timeY;
            dr["timeTranParent"] = previewGeneClass.timeTranParent;

            dr["infoSize"] = previewGeneClass.infoSize;
            dr["infoFontStyle"] = previewGeneClass.infoFontStyle;
            dr["infoFontFamily"] = previewGeneClass.infoFontFamily.Name; //字体参数用String格式存储
            dr["infoX"] = previewGeneClass.infoX;
            dr["infoY"] = previewGeneClass.infoY;
            dr["infoTransparent"] = previewGeneClass.infoTransparent;

            dr["marginSize"] = previewGeneClass.marginSize;

            dt.Rows.Add(dr);

            string jsonStr = JsonConvert.SerializeObject(dt);

            File.WriteAllText(path, jsonStr);
        }

        /// <summary>
        /// json文件的反序列化(从文件)
        /// </summary>
        /// <param name="path">json文件路径</param>
        public static void Json2Parameters(PreviewGeneClass previewGeneClass, string path)
        {
            try
            {
                string jsonStr = File.ReadAllText(path);
                var dt = JsonConvert.DeserializeObject<DataTable>(jsonStr); //将json数据解析为DataTable
                DataRow dr = dt.Rows[0];

                if (dr["stripLocation"].ToString() == "0")
                { previewGeneClass.stripLocation = InfoStripLocations.Beneath; }
                else
                { previewGeneClass.stripLocation = InfoStripLocations.Right; }

                previewGeneClass.stripSize = float.Parse(dr["stripSize"].ToString());

                previewGeneClass.logoSize = float.Parse(dr["logoSize"].ToString());
                previewGeneClass.logoX = float.Parse(dr["logoX"].ToString());
                previewGeneClass.logoY = float.Parse(dr["logoY"].ToString());

                previewGeneClass.timeSize = float.Parse(dr["timeSize"].ToString());
                if (dr["timeFontStyle"].ToString() == "0")
                {
                    previewGeneClass.timeFontStyle = FontStyle.Regular;
                }
                else if (dr["timeFontStyle"].ToString() == "1")
                {
                    previewGeneClass.timeFontStyle = FontStyle.Bold;
                }
                else if (dr["timeFontStyle"].ToString() == "2")
                {
                    previewGeneClass.timeFontStyle = FontStyle.Italic;
                }
                else if (dr["timeFontStyle"].ToString() == "4")
                {
                    previewGeneClass.timeFontStyle = FontStyle.Underline;
                }
                else if (dr["timeFontStyle"].ToString() == "8")
                {
                    previewGeneClass.timeFontStyle = FontStyle.Strikeout;
                }
                previewGeneClass.timeFont = new FontFamily(dr["timeFont"].ToString());
                previewGeneClass.timeX = float.Parse(dr["timeX"].ToString());
                previewGeneClass.timeY = float.Parse(dr["timeY"].ToString());
                previewGeneClass.timeTranParent = int.Parse(dr["timeTranParent"].ToString());

                previewGeneClass.infoSize = float.Parse(dr["infoSize"].ToString());
                if (dr["infoFontStyle"].ToString() == "0")
                {
                    previewGeneClass.infoFontStyle = FontStyle.Regular;
                }
                else if (dr["infoFontStyle"].ToString() == "1")
                {
                    previewGeneClass.infoFontStyle = FontStyle.Bold;
                }
                else if (dr["infoFontStyle"].ToString() == "2")
                {
                    previewGeneClass.infoFontStyle = FontStyle.Italic;
                }
                else if (dr["infoFontStyle"].ToString() == "4")
                {
                    previewGeneClass.infoFontStyle = FontStyle.Underline;
                }
                else if (dr["infoFontStyle"].ToString() == "8")
                {
                    previewGeneClass.infoFontStyle = FontStyle.Strikeout;
                }
                previewGeneClass.infoFontFamily = new FontFamily(dr["infoFontFamily"].ToString());
                previewGeneClass.infoX = float.Parse(dr["infoX"].ToString());
                previewGeneClass.infoY = float.Parse(dr["infoY"].ToString());
                previewGeneClass.infoTransparent = int.Parse(dr["infoTransparent"].ToString());

                previewGeneClass.marginSize = float.Parse(dr["marginSize"].ToString());
            }
            catch(Exception e)
            {
                MessageBox.Show("Deserialization failed: " + e.Message);
            }
        }


        /// <summary>
        /// json文件的反序列化(从字节流)
        /// </summary>
        /// <param name="path">json文件路径</param>
        public static void Json2Parameters(PreviewGeneClass previewGeneClass, byte[] bytes)
        {
            //字节数组转字符串
            String jsonStr = System.Text.Encoding.UTF8.GetString(bytes);

            var dt = JsonConvert.DeserializeObject<DataTable>(jsonStr); //将json数据解析为DataTable
            DataRow dr = dt.Rows[0];

            if (dr["stripLocation"].ToString() == "0")
            { previewGeneClass.stripLocation = InfoStripLocations.Beneath; }
            else
            { previewGeneClass.stripLocation = InfoStripLocations.Right; }

            previewGeneClass.stripSize = float.Parse(dr["stripSize"].ToString());

            previewGeneClass.logoSize = float.Parse(dr["logoSize"].ToString());
            previewGeneClass.logoX = float.Parse(dr["logoX"].ToString());
            previewGeneClass.logoY = float.Parse(dr["logoY"].ToString());

            previewGeneClass.timeSize = float.Parse(dr["timeSize"].ToString());
            if (dr["timeFontStyle"].ToString() == "0")
            {
                previewGeneClass.timeFontStyle = FontStyle.Regular;
            }
            else if (dr["timeFontStyle"].ToString() == "1")
            {
                previewGeneClass.timeFontStyle = FontStyle.Bold;
            }
            else if (dr["timeFontStyle"].ToString() == "2")
            {
                previewGeneClass.timeFontStyle = FontStyle.Italic;
            }
            else if (dr["timeFontStyle"].ToString() == "4")
            {
                previewGeneClass.timeFontStyle = FontStyle.Underline;
            }
            else if (dr["timeFontStyle"].ToString() == "8")
            {
                previewGeneClass.timeFontStyle = FontStyle.Strikeout;
            }
            previewGeneClass.timeFont = new FontFamily(dr["timeFont"].ToString());
            previewGeneClass.timeX = float.Parse(dr["timeX"].ToString());
            previewGeneClass.timeY = float.Parse(dr["timeY"].ToString());
            previewGeneClass.timeTranParent = int.Parse(dr["timeTranParent"].ToString());

            previewGeneClass.infoSize = float.Parse(dr["infoSize"].ToString());
            if (dr["infoFontStyle"].ToString() == "0")
            {
                previewGeneClass.infoFontStyle = FontStyle.Regular;
            }
            else if (dr["infoFontStyle"].ToString() == "1")
            {
                previewGeneClass.infoFontStyle = FontStyle.Bold;
            }
            else if (dr["infoFontStyle"].ToString() == "2")
            {
                previewGeneClass.infoFontStyle = FontStyle.Italic;
            }
            else if (dr["infoFontStyle"].ToString() == "4")
            {
                previewGeneClass.infoFontStyle = FontStyle.Underline;
            }
            else if (dr["infoFontStyle"].ToString() == "8")
            {
                previewGeneClass.infoFontStyle = FontStyle.Strikeout;
            }
            previewGeneClass.infoFontFamily = new FontFamily(dr["infoFontFamily"].ToString());
            previewGeneClass.infoX = float.Parse(dr["infoX"].ToString());
            previewGeneClass.infoY = float.Parse(dr["infoY"].ToString());
            previewGeneClass.infoTransparent = int.Parse(dr["infoTransparent"].ToString());

            previewGeneClass.marginSize = float.Parse(dr["marginSize"].ToString());
        }
    }
}
