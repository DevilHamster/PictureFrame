using Force.DeepCloner;
using System.CodeDom;
using System.Drawing.Text;

namespace PictureFrame
{
    public partial class frm_Main : Form
    {
        #region Informations
        string? filePath; //当前打开的图像的文件路径
        Bitmap rawImage;
        #endregion

        #region Lists
        string[] brandList = new string[]
        {
            //数码相机
            "SONY  索尼", //0
            "Nikon  尼康", //1
            "Canon  佳能", //2
            //"FUJIFILM  富士", //3
            //"PENTAX  宾得", //4
            //"Ricoh  理光", //5
            //"Leica  徕卡", //6
            //"Panasonic  松下", //7
            //"Hasselblad  哈苏", //8
            //"Kodak  柯达", //9
            //"Olympus  奥林巴斯","Insta360  影石","SIGMA  适马",

            //手机
            //"Apple  苹果","HUAWEI  华为","VIVO","SUMSUNG  三星",
            //"OPPO","HONOR  荣耀","ONEPLUS  一加","MEIZU  魅族","XiaoMi  小米",
            //"LG","realme  真我",

            //无人机
            //"DJI  大疆",

            //运动相机
            //"GoPro","SJCAM  速影","AKASO"
        }; //设备品牌列表

        #endregion

        #region flags
        bool isEditing = false; //当前是否正在处理图像
        bool isSaving = false; //当前是否正在保存图像或预设
        #endregion

        #region classes
        PreviewGeneClass previewGenePack; //生成预览图像的数据包，待载入图像后实例化
        BarStruct barStruct;
        #endregion

        public frm_Main()
        {
            InitializeComponent();
            //this.DoubleBuffered = true; //开启双缓存

            #region 参数初始化
            timer1.Interval= 1000;
            //信息栏尺寸控制
            barStruct.strip_Size_Min = 0.02f; barStruct.strip_Size_Max = 0.40f;
            barStruct.logo_Size_Min = 0.00f; barStruct.logo_Size_Max = 1.00f;
            barStruct.logo_X_Min = 0.00f; barStruct.logo_X_Max = 1.00f;
            barStruct.logo_Y_Min = 0.00f; barStruct.logo_Y_Max = 1.00f;
            barStruct.time_X_Min = 0.00f; barStruct.time_X_Max = 1.00f;
            barStruct.time_Y_Min = 0.00f; barStruct.time_Y_Max = 1.00f;
            barStruct.time_Size_Min = 0.00f; barStruct.time_Size_Max = 1.00f;
            barStruct.info_X_Min = 0.00f; barStruct.info_X_Max = 1.00f;
            barStruct.info_Y_Min = 0.00f; barStruct.info_Y_Max = 1.00f;
            barStruct.info_Size_Min = 0.00f; barStruct.info_Size_Max = 1.00f;
            barStruct.margin_Size_Min = 0.00f; barStruct.margin_Size_Max = 0.10f;
            //logo栏载入数据，如果用cbox_LogoList.DataSource = brandList绑定，会报错
            foreach (string brand in brandList)
            {
                cbox_LogoList.Items.Add(brand);
            }

            //获取系统自带的英文字体，在cbox_date_Font摆出可用字体以及样式(表逻辑)
            //字体和字体样式，分为表和里两个层次
            //在程序没加载入图像时，仅做表逻辑，即初始化一个显示
            //在程序载入图像时，字体只有里逻辑，即根据用户设定，更新previewGenePack，字体样式有表逻辑也有里逻辑，表逻辑为根据字体改变选项，里逻辑为更新previewGenePack
            InstalledFontCollection MyFont = new InstalledFontCollection();
            foreach (FontFamily family in MyFont.Families)
            {
                cbox_date_Font.Items.Add(family.Name);
                cbox_picInfo_Font.Items.Add(family.Name);
            }
            //如果有Arial，使用Arial，否则选择列表第一个
            if (cbox_date_Font.Items.Contains("Arial"))
            {
                cbox_date_Font.SelectedItem = "Arial";
                cbox_picInfo_Font.SelectedItem = "Arial";
            }
            else
            {
                cbox_date_Font.SelectedIndex = 0;
                cbox_picInfo_Font.SelectedIndex = 0;
            }
            //摆出可用FontStyle
            FontStyleUpdate(new FontFamily(cbox_date_Font.SelectedItem.ToString()), cbox_date_Style);
            FontStyleUpdate(new FontFamily(cbox_picInfo_Font.SelectedItem.ToString()), cbox_picInfo_Style);

            //锁死全局
            ConditionCheck(isEditing);

            #endregion
        }

        private void frm_Main_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 点击Open Picture，打开本地图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmi_OpenPicture_Click(object sender, EventArgs e)  
        {
            //内存保险
            GC.Collect();
            rawImage = null;

            #region 打开对话框，获取文件路径
            //实例化一个打开文件的对话框
            OpenFileDialog of = new OpenFileDialog();
            //不允许多选
            of.Multiselect = false;
            //设置对话框标题
            of.Title = "Please choose the image to edit";
            //设置指定图像类型
            of.Filter = "Image Files|*.jpg";
            //显示对话框，如果选择确定
            if (of.ShowDialog() == DialogResult.OK)
            {
                //获取文件路径
                filePath = of.FileName;
            }
            else
            {
                return;
            }
            #endregion

            #region 根据文件路径，加载图像并完善一个previewGenePack出来，保证能全部依赖previewGenePack的数据绘制一张预览图出来

            //这里使用一次ResizeToStandard，打开文件时会消耗几十毫秒，但使界面上的参数调整更为快速
            //pictureBox控件宽高均为802
            rawImage = PictureFunctions.ReadFromStream(filePath);
            Bitmap img = PictureFunctions.ResizeToStandard(rawImage, 802);
            previewGenePack = new PreviewGeneClass(img);

            //部分previewGenePack参数依赖控件数据(如果控件有数据)
            //previewGenePack字体字样部分参数通过控件读取

            //cbox_date_Font一定有选中内容，不做条件判断
            previewGenePack.timeFont = new FontFamily(cbox_date_Font.SelectedItem.ToString()); //打开图片时读取当前选中字体
            previewGenePack.infoFontFamily = new FontFamily(cbox_picInfo_Font.SelectedItem.ToString());
            //如果有选中字体样式，previewGenePack对应参数更新
            if (cbox_date_Style.SelectedIndex != -1)
            {
                switch (cbox_date_Style.SelectedItem)
                {
                    case "Bold":
                        previewGenePack.timeFontStyle = FontStyle.Bold; break;
                    case "Italic":
                        previewGenePack.timeFontStyle = FontStyle.Italic; break;
                    case "Regular":
                        previewGenePack.timeFontStyle = FontStyle.Regular; break;
                    case "Strikeout":
                        previewGenePack.timeFontStyle = FontStyle.Strikeout; break;
                    case "Underline":
                        previewGenePack.timeFontStyle = FontStyle.Underline; break;
                }
            }
            if (cbox_picInfo_Style.SelectedIndex != -1)
            {
                switch (cbox_date_Style.SelectedItem)
                {
                    case "Bold":
                        previewGenePack.infoFontStyle = FontStyle.Bold; break;
                    case "Italic":
                        previewGenePack.infoFontStyle = FontStyle.Italic; break;
                    case "Regular":
                        previewGenePack.infoFontStyle = FontStyle.Regular; break;
                    case "Strikeout":
                        previewGenePack.infoFontStyle = FontStyle.Strikeout; break;
                    case "Underline":
                        previewGenePack.infoFontStyle = FontStyle.Underline; break;
                }
            }

            #endregion

            #region 设置开关
            //所有控件(除Open Picture外)在未打开图像的情况下保持锁定
            //一旦软件打开一次图像，此后isEditing为true
            isEditing = true; ConditionCheck(isEditing);
            //previewGenePack更新，两个rbtn归位
            rbtn_InfoStrip_Beneath.Checked = true; rbtn_InfoStrip_Right.Checked = false;
            #endregion

            #region 获取图像信息，完善一个JPGInfo出来，更新参数界面，以及previewGenePack部分参数

            JPGInfo jpgInfo = PictureFunctions.GetInfo(filePath);
            if (jpgInfo.date != "")
            {
                tbox_date.Text = jpgInfo.date;
                //tbox_date.ReadOnly = true;
                previewGenePack.time = jpgInfo.date;
            }
            if (jpgInfo.camera != "")
            {
                tbox_picInfo_Camera.Text = jpgInfo.camera;
                //tbox_picInfo_Camera.ReadOnly = true;
            }
            if (jpgInfo.f_stop != "")
            {
                tbox_picInfo_F.Text = jpgInfo.f_stop;
                //tbox_picInfo_F.ReadOnly = true;
            }
            if (jpgInfo.expoTime != "")
            {
                tbox_picInfo_Expo.Text = jpgInfo.expoTime;
                //tbox_picInfo_Expo.ReadOnly = true;
            }
            if (jpgInfo.iso != "" && jpgInfo.iso != null)
            {
                tbox_picInfo_ISO.Text = "ISO" + jpgInfo.iso;
                //tbox_picInfo_ISO.ReadOnly = true;
            }
            if (jpgInfo.focalLength != "")
            {
                tbox_picInfo_Focal.Text = jpgInfo.focalLength;
                //tbox_picInfo_Focal.ReadOnly = true;
            }
            if (jpgInfo.evBias != "")
            {
                tbox_picInfo_Bias.Text = jpgInfo.evBias;
                //tbox_picInfo_Bias.ReadOnly = true;
            }
            if (PictureFunctions.GetBrandIndex(jpgInfo.maker) != -1)
            {
                cbox_LogoList.SelectedIndex = PictureFunctions.GetBrandIndex(jpgInfo.maker);
                previewGenePack.brandIndex = PictureFunctions.GetBrandIndex(jpgInfo.maker);
            }
            InfoUpdate(previewGenePack); //更新Info信息 

            #endregion

            #region 更新控件

            lbl_info_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.stripSize * 100) + "% of the width of the image";
            bar_StripSize._Value = bar_StripSize._Min + (int)((previewGenePack.stripSize - barStruct.strip_Size_Min) * (bar_StripSize._Max - bar_StripSize._Min) / (barStruct.strip_Size_Max - barStruct.strip_Size_Min));

            lbl_logo_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.logoSize * 100) + "% of the height of the Info Strip";
            bar_logoSize._Value = bar_logoSize._Min + (int)((previewGenePack.logoSize - barStruct.logo_Size_Min) * (bar_logoSize._Max - bar_logoSize._Min) / (barStruct.logo_Size_Max - barStruct.logo_Size_Min));

            bar_logoX._Value = bar_logoX._Min + (int)((previewGenePack.logoX - barStruct.logo_X_Min) * (bar_logoX._Max - bar_logoX._Min) / (barStruct.logo_X_Max - barStruct.logo_X_Min));
            bar_logoY._Value = bar_logoY._Min + (int)((previewGenePack.logoY - barStruct.logo_Y_Min) * (bar_logoY._Max - bar_logoY._Min) / (barStruct.logo_Y_Max - barStruct.logo_Y_Min));


            lbl_date_FontSize.Text = "Font Size :" + string.Format("{0:F2}", previewGenePack.timeSize * 100) + "% of the height of the Info Strip";
            bar_timeSize._Value = bar_timeSize._Min + (int)((previewGenePack.timeSize - barStruct.time_Size_Min) * (bar_timeSize._Max - bar_timeSize._Min) / (barStruct.time_Size_Max - barStruct.time_Size_Min));

            bar_timeX._Value = bar_timeX._Min + (int)((previewGenePack.timeX - barStruct.time_X_Min) * (bar_timeX._Max - bar_timeX._Min) / (barStruct.time_X_Max - barStruct.time_X_Min));
            bar_timeY._Value = bar_timeY._Min + (int)((previewGenePack.timeY - barStruct.time_Y_Min) * (bar_timeY._Max - bar_timeY._Min) / (barStruct.time_Y_Max - barStruct.time_Y_Min));

            bar_timeTrans._Value = previewGenePack.timeTranParent;


            lbl_picInfo_FontSize.Text = "Font Size :" + string.Format("{0:F2}", previewGenePack.infoSize * 100) + "% of the height of the Info Strip";
            bar_infoSize._Value = bar_infoSize._Min + (int)((previewGenePack.infoSize - barStruct.info_Size_Min) * (bar_infoSize._Max - bar_infoSize._Min) / (barStruct.info_Size_Max - barStruct.info_Size_Min));

            bar_infoX._Value = bar_infoX._Min + (int)((previewGenePack.infoX - barStruct.info_X_Min) * (bar_infoX._Max - bar_infoX._Min) / (barStruct.info_X_Max - barStruct.info_X_Min));
            bar_infoY._Value = bar_infoY._Min + (int)((previewGenePack.infoY - barStruct.info_Y_Min) * (bar_infoY._Max - bar_infoY._Min) / (barStruct.info_Y_Max - barStruct.info_Y_Min));

            bar_infoTrans._Value = previewGenePack.infoTransparent;

            bar_marginSize._Value = bar_marginSize._Min + (int)((previewGenePack.marginSize - barStruct.margin_Size_Min) * (bar_marginSize._Max - bar_marginSize._Min) / (barStruct.margin_Size_Max - barStruct.margin_Size_Min));

            #endregion

            #region 根据previewGenePack，绘制预览图在pictureBox上

            //绘制图像
            PreviewUpdate();

            #endregion

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //stopwatch.Stop();
            //TimeSpan timespan = stopwatch.Elapsed;
            //tbox_testMessage.Text = timespan.TotalMilliseconds.ToString();
        }

        /// <summary>
        /// 选择Beneath，调整previewGenePack，重新绘图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtn_InfoStrip_Beneath_CheckedChanged(object sender, EventArgs e)
        {
            previewGenePack.stripLocation = InfoStripLocations.Beneath;
            //绘制图像
            PreviewUpdate();

            //控件变动
            lbl_info_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.stripSize * 100) + "% of the height of the image";
            lbl_logo_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.logoSize * 100) + "% of the height of the Info Strip";
            lbl_date_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.timeSize * 100) + "% of the height of the Info Strip";
            lbl_picInfo_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.infoSize * 100) + "% of the height of the Info Strip";
            lbl_margin_size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.marginSize * 100) + "% of the width of raw image";
        }

        /// <summary>
        /// 选择Right，调整previewGenePack，重新绘图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtn_InfoStrip_Right_CheckedChanged(object sender, EventArgs e)
        {
            previewGenePack.stripLocation = InfoStripLocations.Right;
            //绘制图像
            PreviewUpdate();

            //控件变动
            lbl_info_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.stripSize * 100) + "% of the width of the image";
            lbl_logo_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.logoSize * 100) + "% of the width of the Info Strip";
            lbl_date_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.timeSize * 100) + "% of the width of the Info Strip";
            lbl_picInfo_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.infoSize * 100) + "% of the width of the Info Strip";
            lbl_margin_size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.marginSize * 100) + "% of the height of raw image";
        }

        /// <summary>
        /// 信息栏尺寸控制条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_StripSize_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //先更新previewgenePack，控制previewGenePack.stripSize最高为barStruct.strip_Size_Max，最低为barStruct.strip_Size_Min
            previewGenePack.stripSize = barStruct.strip_Size_Min +
                (barStruct.strip_Size_Max - barStruct.strip_Size_Min) * ((float)bar_StripSize._Value / (float)bar_StripSize._Max);

            //更新相关控件
            if (previewGenePack.stripLocation == InfoStripLocations.Beneath)
            {
                lbl_info_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.stripSize * 100) + "% of the width of the image";
            }
            else
            {
                lbl_info_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.stripSize * 100) + "% of the height of the image";
            }

            //后根据previewgenePack重绘图像预览
            PreviewUpdate();
        }

        /// <summary>
        /// Logo尺寸控制条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_logoSize_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //先更新previewgenePack
            previewGenePack.logoSize = barStruct.logo_Size_Min + 
                (barStruct.logo_Size_Max - barStruct.logo_Size_Min) * ((float)bar_logoSize._Value / (float)bar_logoSize._Max);
            //更新相关控件
            if (previewGenePack.stripLocation == InfoStripLocations.Beneath)
            {
                lbl_logo_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.logoSize * 100) + "% of the height of the Info Strip";
            }
            else
            {
                lbl_logo_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.logoSize * 100) + "% of the width of the Info Strip";
            }

            //后根据previewgenePack重绘图像预览
            PreviewUpdate();
        }

        /// <summary>
        /// LogoX控制条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_logoX_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //先更新previewgenePack
            previewGenePack.logoX = barStruct.logo_X_Min +
                (barStruct.logo_X_Max - barStruct.logo_X_Min) * ((float)bar_logoX._Value / (float)bar_logoX._Max);
            //不更新label显示
            //根据previewgenePack重绘图像预览
            PreviewUpdate();
        }

        /// <summary>
        /// LogoY控制条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_logoY_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //先更新previewgenePack
            previewGenePack.logoY = barStruct.logo_Y_Min +
                (barStruct.logo_Y_Max - barStruct.logo_Y_Min) * ((float)bar_logoY._Value / (float)bar_logoY._Max);
            //不更新label显示
            //根据previewgenePack重绘图像预览
            PreviewUpdate();
        }

        /// <summary>
        /// logo列表选中项发生变动，更新previewgenePack，如果现在有logo显示，重绘，如果没有logo显示，无动作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbox_LogoList_SelectedIndexChanged(object sender, EventArgs e)
        {
            //当前未选中值，返回
            if (cbox_LogoList.SelectedIndex == -1) { return; }

            else
            {
                //更新previewgenePack，控制previewGenePack.brandIndex参数
                previewGenePack.brandIndex = cbox_LogoList.SelectedIndex;
                //重绘
                PreviewUpdate();
            }
        }

        /// <summary>
        /// 点击后logoX回归中值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_logo_midAlignX_Click(object sender, EventArgs e)
        {
            bar_logoX._Value = (bar_logoX._Max - bar_logoX._Min) / 2;
        }

        /// <summary>
        /// 点击后logoY回归中值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_logo_midAlignY_Click(object sender, EventArgs e)
        {
            bar_logoY._Value = (bar_logoY._Max - bar_logoY._Min) / 2;
        }

        /// <summary>
        /// 字体选择更新，做字体的里逻辑，字体样式的表逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbox_date_Font_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (previewGenePack != null)
            {
                FontFamily temp = new FontFamily(cbox_date_Font.SelectedItem.ToString());
                //更新previewGenePack参数
                previewGenePack.timeFont = temp;
                //更新cbox_date_Style显示
                FontStyleUpdate(temp, cbox_date_Style);
                //绘制图像
                PreviewUpdate();
            }
        }

        /// <summary>
        /// 字体选择更新，做字体的里逻辑，字体样式的表逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbox_picInfo_Font_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (previewGenePack != null)
            {
                FontFamily temp = new FontFamily(cbox_picInfo_Font.SelectedItem.ToString());
                //更新previewGenePack参数
                previewGenePack.infoFontFamily = temp;
                //更新cbox_date_Style显示
                FontStyleUpdate(temp, cbox_picInfo_Style);
                //绘制图像
                PreviewUpdate();
            }
        }

        /// <summary>
        /// 字体样式选择更新，更新previewGenePack
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbox_date_Style_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (previewGenePack != null)
            {
                switch (cbox_date_Style.SelectedItem)
                {
                    case "Bold":
                        previewGenePack.timeFontStyle = FontStyle.Bold; break;
                    case "Italic":
                        previewGenePack.timeFontStyle = FontStyle.Italic; break;
                    case "Regular":
                        previewGenePack.timeFontStyle = FontStyle.Regular; break;
                    case "Strikeout":
                        previewGenePack.timeFontStyle = FontStyle.Strikeout; break;
                    case "Underline":
                        previewGenePack.timeFontStyle = FontStyle.Underline; break;
                }
                //绘制图像
                PreviewUpdate();
            }
        }

        /// <summary>
        /// 字体样式选择更新，更新previewGenePack
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbox_picInfo_Style_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (previewGenePack != null)
            {
                switch (cbox_picInfo_Style.SelectedItem)
                {
                    case "Bold":
                        previewGenePack.infoFontStyle = FontStyle.Bold; break;
                    case "Italic":
                        previewGenePack.infoFontStyle = FontStyle.Italic; break;
                    case "Regular":
                        previewGenePack.infoFontStyle = FontStyle.Regular; break;
                    case "Strikeout":
                        previewGenePack.infoFontStyle = FontStyle.Strikeout; break;
                    case "Underline":
                        previewGenePack.infoFontStyle = FontStyle.Underline; break;
                }
                //绘制图像
                PreviewUpdate();
            }
        }

        /// <summary>
        /// timeX控制条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_timeX_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //先更新previewgenePack
            previewGenePack.timeX = barStruct.time_X_Min +
                (barStruct.time_X_Max - barStruct.time_X_Min) * ((float)bar_timeX._Value / (float)bar_timeX._Max);
            //根据previewgenePack重绘图像预览
            PreviewUpdate();
        }

        /// <summary>
        /// timeY控制条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_timeY_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //先更新previewgenePack
            previewGenePack.timeY = barStruct.time_Y_Min +
                (barStruct.time_Y_Max - barStruct.time_Y_Min) * ((float)bar_timeY._Value / (float)bar_timeY._Max);
            //根据previewgenePack重绘图像预览
            PreviewUpdate();
        }

        /// <summary>
        /// timeSize控制条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_timeSize_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //先更新previewgenePack
            previewGenePack.timeSize = barStruct.time_Size_Min +
                (barStruct.time_Size_Max - barStruct.time_Size_Min) * ((float)bar_timeSize._Value / (float)bar_timeSize._Max);
            //更新相关控件
            if (previewGenePack.stripLocation == InfoStripLocations.Beneath)
            {
                lbl_date_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.timeSize * 100) + "% of the height of the Info Strip";
            }
            else
            {
                lbl_date_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.timeSize * 100) + "% of the width of the Info Strip";
            }
            //根据previewgenePack重绘图像预览
            PreviewUpdate();
        }

        /// <summary>
        /// timeTrans控制条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_timeTrans_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //更新previewGenePack
            previewGenePack.timeTranParent = bar_timeTrans._Value;
            //根据previewgenePack重绘图像预览
            PreviewUpdate();
        }

        /// <summary>
        /// infoX控制条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_infoX_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //先更新previewgenePack
            previewGenePack.infoX = barStruct.info_X_Min +
                (barStruct.info_X_Max - barStruct.info_X_Min) * ((float)bar_infoX._Value / (float)bar_infoX._Max);
            //根据previewgenePack重绘图像预览
            PreviewUpdate();
        }

        /// <summary>
        /// infoY控制条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_infoY_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //先更新previewgenePack
            previewGenePack.infoY = barStruct.info_Y_Min +
                (barStruct.info_Y_Max - barStruct.info_Y_Min) * ((float)bar_infoY._Value / (float)bar_infoY._Max);
            //根据previewgenePack重绘图像预览
            PreviewUpdate();
        }

        /// <summary>
        /// infoSize控制条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_infoSize_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //先更新previewgenePack
            previewGenePack.infoSize = barStruct.info_Size_Min +
                (barStruct.info_Size_Max - barStruct.info_Size_Min) * ((float)bar_infoSize._Value / (float)bar_infoSize._Max);
            //更新相关控件
            if (previewGenePack.stripLocation == InfoStripLocations.Beneath)
            {
                lbl_picInfo_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.infoSize * 100) + "% of the height of the Info Strip";
            }
            else
            {
                lbl_picInfo_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.infoSize * 100) + "% of the width of the Info Strip";
            }
            //根据previewgenePack重绘图像预览
            PreviewUpdate();
        }

        /// <summary>
        /// infoTrans控制条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_infoTrans_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //更新previewGenePack
            previewGenePack.infoTransparent = bar_infoTrans._Value;
            //根据previewgenePack重绘图像预览
            PreviewUpdate();
        }

        /// <summary>
        /// 时间显示居中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_date_midAlign_Click(object sender, EventArgs e)
        {
            bar_timeX._Value = (bar_timeX._Max - bar_timeX._Min) / 2;
        }

        /// <summary>
        /// 时间显示居中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_date_midAlign2_Click(object sender, EventArgs e)
        {
            bar_timeY._Value = (bar_timeY._Max - bar_timeY._Min) / 2;
        }

        /// <summary>
        /// info显示居中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_picInfo_midAlign_Click(object sender, EventArgs e)
        {
            bar_infoX._Value = (bar_infoX._Max - bar_infoX._Min) / 2;
        }

        /// <summary>
        /// info显示居中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_picInfo_midAlign2_Click(object sender, EventArgs e)
        {
            bar_infoY._Value = (bar_infoY._Max - bar_infoY._Min) / 2;
        }

        /// <summary>
        /// 边框大小控制条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_marginSize_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //先更新previewgenePack
            previewGenePack.marginSize = barStruct.margin_Size_Min +
                (barStruct.margin_Size_Max - barStruct.margin_Size_Min) * ((float)bar_marginSize._Value / (float)bar_marginSize._Max);

            //更新控件
            if (previewGenePack.stripLocation == InfoStripLocations.Beneath)
            {
                lbl_margin_size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.marginSize * 100) + "% of the width of raw image";
            }
            else 
            {
                lbl_margin_size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.marginSize * 100) + "% of the height of raw image";
            }

            //后根据previewgenePack重绘图像预览
            PreviewUpdate();
        }

        /// <summary>
        /// 日期更改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbox_date_TextChanged(object sender, EventArgs e)
        {
            previewGenePack.time = tbox_date.Text;
            //重绘图像预览
            PreviewUpdate();
        }

        /// <summary>
        /// 拍摄参数更改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbox_picInfo_Camera_TextChanged(object sender, EventArgs e)
        {
            InfoUpdate(previewGenePack);
            //重绘图像预览
            PreviewUpdate();
        }
        private void tbox_picInfo_F_TextChanged(object sender, EventArgs e)
        {
            InfoUpdate(previewGenePack);
            //重绘图像预览
            PreviewUpdate();
        }
        private void tbox_picInfo_Expo_TextChanged(object sender, EventArgs e)
        {
            InfoUpdate(previewGenePack);
            //重绘图像预览
            PreviewUpdate();
        }
        private void tbox_picInfo_ISO_TextChanged(object sender, EventArgs e)
        {
            InfoUpdate(previewGenePack);
            //重绘图像预览
            PreviewUpdate();
        }
        private void tbox_picInfo_Focal_TextChanged(object sender, EventArgs e)
        {
            InfoUpdate(previewGenePack);
            //重绘图像预览
            PreviewUpdate();
        }
        private void tbox_picInfo_Bias_TextChanged(object sender, EventArgs e)
        {
            InfoUpdate(previewGenePack);
            //重绘图像预览
            PreviewUpdate();
        }


        #region functions

        /// <summary>
        /// 更新字体样式选项(表逻辑)
        /// </summary>
        /// <param name="fontFamily"></param>
        public void FontStyleUpdate(FontFamily fontFamily, ComboBox cbox)
        {
            cbox.Items.Clear();
            if (fontFamily != null)
            {
                foreach (FontStyle fs in FontFunctions.FontStyleAvailable(fontFamily))
                {
                    cbox.Items.Add(fs.ToString());
                }
            }
        }

        /// <summary>
        /// 状态检查，锁死全局
        /// </summary>
        public void ConditionCheck(bool isEditing)
        {
            //如果当前没打开图像，锁死
            if (!isEditing)
            {
                foreach (Control control in this.Controls)
                {
                    control.Enabled = false;
                }
                //只保留部分功能
                menuStrip1.Enabled = true;
                tsmi_SavePicture.Enabled = false;
                tsmi_SavePreset.Enabled = false;
                tsmi_OpenPreset.Enabled = false;
                tsmi_BatchApply.Enabled = false;
                useToolStripMenuItem.Enabled = false;
                return;
            }
            //如果当前已经打开图像，检查第一个控件，如果第一个控件Enable==true，则不继续遍历，反之说明当前是第一次打开图像，全部解锁
            else
            {
                if (this.Controls[0].Enabled == true)
                {
                    return;
                }
                else
                {
                    foreach (Control control in this.Controls)
                    {
                        control.Enabled = true;
                    }
                    menuStrip1.Enabled = true;
                    tsmi_SavePicture.Enabled = true;
                    tsmi_SavePreset.Enabled = true;
                    tsmi_OpenPreset.Enabled = true;
                    tsmi_BatchApply.Enabled = true;
                    useToolStripMenuItem.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 更新拍摄参数信息
        /// </summary>
        /// <param name="previewGeneClass"></param>
        public void InfoUpdate(PreviewGeneClass previewGeneClass)
        {
            previewGeneClass.info = 
                tbox_picInfo_Camera.Text + " "
                + tbox_picInfo_F.Text + " "
                + tbox_picInfo_Expo.Text + " "
                + tbox_picInfo_ISO.Text + " "
                + tbox_picInfo_Focal.Text + " "
                + tbox_picInfo_Bias.Text;
        }

        /// <summary>
        /// 更新预览
        /// </summary>
        public void PreviewUpdate()
        {
            if (pb_Preview.Image != null)
            {
                pb_Preview.Image.Dispose();
            }

            pb_Preview.Image = PictureFunctions.DrawPreview(previewGenePack); //更新控件显示
        }

        /// <summary>
        /// 控件更新
        /// </summary>
        public void ControlsUpdate()
        {
            if (previewGenePack.stripLocation == InfoStripLocations.Beneath)
            {
                rbtn_InfoStrip_Beneath.Checked = true;
                rbtn_InfoStrip_Right.Checked = false;
            }
            else
            {
                rbtn_InfoStrip_Beneath.Checked = false;
                rbtn_InfoStrip_Right.Checked = true;
            }

            lbl_info_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.stripSize * 100) + "% of the width of the image";
            bar_StripSize._Value = bar_StripSize._Min + (int)((previewGenePack.stripSize - barStruct.strip_Size_Min) * (bar_StripSize._Max - bar_StripSize._Min) / (barStruct.strip_Size_Max - barStruct.strip_Size_Min));

            lbl_logo_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.logoSize * 100) + "% of the height of the Info Strip";
            bar_logoSize._Value = bar_logoSize._Min + (int)((previewGenePack.logoSize - barStruct.logo_Size_Min) * (bar_logoSize._Max - bar_logoSize._Min) / (barStruct.logo_Size_Max - barStruct.logo_Size_Min));

            bar_logoX._Value = bar_logoX._Min + (int)((previewGenePack.logoX - barStruct.logo_X_Min) * (bar_logoX._Max - bar_logoX._Min) / (barStruct.logo_X_Max - barStruct.logo_X_Min));
            bar_logoY._Value = bar_logoY._Min + (int)((previewGenePack.logoY - barStruct.logo_Y_Min) * (bar_logoY._Max - bar_logoY._Min) / (barStruct.logo_Y_Max - barStruct.logo_Y_Min));


            lbl_date_FontSize.Text = "Font Size :" + string.Format("{0:F2}", previewGenePack.timeSize * 100) + "% of the height of the Info Strip";
            bar_timeSize._Value = bar_timeSize._Min + (int)((previewGenePack.timeSize - barStruct.time_Size_Min) * (bar_timeSize._Max - bar_timeSize._Min) / (barStruct.time_Size_Max - barStruct.time_Size_Min));

            bar_timeX._Value = bar_timeX._Min + (int)((previewGenePack.timeX - barStruct.time_X_Min) * (bar_timeX._Max - bar_timeX._Min) / (barStruct.time_X_Max - barStruct.time_X_Min));
            bar_timeY._Value = bar_timeY._Min + (int)((previewGenePack.timeY - barStruct.time_Y_Min) * (bar_timeY._Max - bar_timeY._Min) / (barStruct.time_Y_Max - barStruct.time_Y_Min));

            //更新时间信息字体
            //如果读取出的字体不在系统中，切换至Arial或第一位
            if (!cbox_date_Font.Items.Contains(previewGenePack.timeFont.Name))
            {
                MessageBox.Show("Font not installed, will switch to default font.");
                if (cbox_date_Font.Items.Contains("Arial"))
                {
                    cbox_date_Font.SelectedItem = "Arial";
                    previewGenePack.timeFont = new FontFamily("Arial"); //修正
                }
                else
                {
                    cbox_date_Font.SelectedIndex = 0;
                    previewGenePack.timeFont = new FontFamily(cbox_date_Font.Items[0].ToString()); //修正
                }
            }
            else
            {
                cbox_date_Font.SelectedItem = previewGenePack.timeFont.ToString();
            }

            //更新拍摄参数信息字体
            if (!cbox_picInfo_Font.Items.Contains(previewGenePack.infoFontFamily.Name))
            {
                MessageBox.Show("Font not installed, will switch to default font.");
                if (cbox_picInfo_Font.Items.Contains("Arial"))
                {
                    cbox_picInfo_Font.SelectedItem = "Arial";
                    previewGenePack.infoFontFamily = new FontFamily("Arial");
                }
                else
                {
                    cbox_picInfo_Font.SelectedIndex = 0;
                    previewGenePack.infoFontFamily = new FontFamily(cbox_picInfo_Font.Items[0].ToString());
                }
            }
            else
            {
                cbox_picInfo_Font.SelectedItem = previewGenePack.infoFontFamily.ToString();
            }

            cbox_date_Style.SelectedItem = previewGenePack.timeFontStyle.ToString();
            cbox_picInfo_Style.SelectedItem = previewGenePack.infoFontStyle.ToString();

            bar_timeTrans._Value = previewGenePack.timeTranParent;


            lbl_picInfo_FontSize.Text = "Font Size :" + string.Format("{0:F2}", previewGenePack.infoSize * 100) + "% of the height of the Info Strip";
            bar_infoSize._Value = bar_infoSize._Min + (int)((previewGenePack.infoSize - barStruct.info_Size_Min) * (bar_infoSize._Max - bar_infoSize._Min) / (barStruct.info_Size_Max - barStruct.info_Size_Min));

            bar_infoX._Value = bar_infoX._Min + (int)((previewGenePack.infoX - barStruct.info_X_Min) * (bar_infoX._Max - bar_infoX._Min) / (barStruct.info_X_Max - barStruct.info_X_Min));
            bar_infoY._Value = bar_infoY._Min + (int)((previewGenePack.infoY - barStruct.info_Y_Min) * (bar_infoY._Max - bar_infoY._Min) / (barStruct.info_Y_Max - barStruct.info_Y_Min));

            bar_infoTrans._Value = previewGenePack.infoTransparent;

            bar_marginSize._Value = bar_marginSize._Min + (int)((previewGenePack.marginSize - barStruct.margin_Size_Min) * (bar_marginSize._Max - bar_marginSize._Min) / (barStruct.margin_Size_Max - barStruct.margin_Size_Min));
        }

        #endregion

        /// <summary>
        /// 打开文件路径，保存图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmi_SavePicture_Click(object sender, EventArgs e)
        {
            //保存文件窗口
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = "Save Picture as:",
                RestoreDirectory = true,
                Filter = "Image Files|*.jpg",
                FileName = Path.GetFileNameWithoutExtension(filePath) + "(framed)" + ".jpg"
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                isSaving = true;

                Bitmap temp = previewGenePack.bitmap; //暂存预览图像

                previewGenePack.bitmap = rawImage;
                Bitmap final = PictureFunctions.DrawPreview(previewGenePack);
                PictureFunctions.Save2Path(final, saveFileDialog.FileName);

                previewGenePack.bitmap = temp; //改回预览图像

                isSaving = false;

                MessageBox.Show("Saved Successfully.");
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// 打开文件路径，保存预设
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmi_SavePreset_Click(object sender, EventArgs e)
        {
            //保存文件窗口
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = "Save Preset as:",
                RestoreDirectory = true,
                Filter = "Preset Files|*.json"
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                isSaving = true;

                //做预设保存操作
                JSONinteractions.Parameters2Json(previewGenePack, saveFileDialog.FileName.ToString());

                isSaving = false;
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// 读取预设
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmi_OpenPreset_Click(object sender, EventArgs e)
        {
            //实例化一个打开文件的对话框
            OpenFileDialog of = new OpenFileDialog();
            //不允许多选
            of.Multiselect = false;
            //设置对话框标题
            of.Title = "Please choose the preset to apply";
            //设置指定图像类型
            of.Filter = "Preset Files|*.json";
            //显示对话框，如果选择确定
            if (of.ShowDialog() == DialogResult.OK)
            {
                //获取文件路径
                filePath = of.FileName;
                //变更previewGenePack
                JSONinteractions.Json2Parameters(previewGenePack, filePath);

                //根据previewgenePack重绘图像预览
                PreviewUpdate();

                //控件更新
                ControlsUpdate();
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// 使用内嵌预设beneathMidlign
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void beneathMidlignToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //变更previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Beneath_MidAlign);
            //根据previewgenePack重绘图像预览
            PreviewUpdate();

            //控件更新
            ControlsUpdate();
        }

        /// <summary>
        /// 每隔1s回收一次内存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            System.GC.Collect();
            timer1.Enabled = true;
        }

        /// <summary>
        /// 使用内嵌预设beneathMidlign2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void beneathMidlignToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            //变更previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Beneath_MidAlign2);
            //根据previewgenePack重绘图像预览
            PreviewUpdate();

            //控件更新
            ControlsUpdate();
        }

        /// <summary>
        /// 使用内嵌预设beneathMidlign3
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void beneathMidlignToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            //变更previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Beneath_MidAlign3);
            //根据previewgenePack重绘图像预览
            PreviewUpdate();

            //控件更新
            ControlsUpdate();
        }

        private void beneathCorner横向构图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //变更previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Beneath_Corner);
            //根据previewgenePack重绘图像预览
            PreviewUpdate();

            //控件更新
            ControlsUpdate();
        }

        private void rightCorner方形构图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //变更previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Beneath_Corner2);
            //根据previewgenePack重绘图像预览
            PreviewUpdate();

            //控件更新
            ControlsUpdate();
        }

        private void rightCorner竖向构图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //变更previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Beneath_Corner3);
            //根据previewgenePack重绘图像预览
            PreviewUpdate();

            //控件更新
            ControlsUpdate();
        }

        private void rightCorner横向构图ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //变更previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Right_Corner);
            //根据previewgenePack重绘图像预览
            PreviewUpdate();

            //控件更新
            ControlsUpdate();
        }

        private void rightCorner方形构图ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            //变更previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Right_Corner2);
            //根据previewgenePack重绘图像预览
            PreviewUpdate();

            //控件更新
            ControlsUpdate();
        }

        private void rightCorner竖向1构图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //变更previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Right_Corner3);
            //根据previewgenePack重绘图像预览
            PreviewUpdate();

            //控件更新
            ControlsUpdate();
        }

        /// <summary>
        /// 批处理操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmi_BatchApply_Click(object sender, EventArgs e)
        {
            //实例化一个打开文件的对话框
            OpenFileDialog of = new OpenFileDialog();
            //不允许多选
            of.Multiselect = true;
            //设置对话框标题
            of.Title = "Please choose the images to edit";
            //设置指定图像类型
            of.Filter = "Image Files|*.jpg";
            //显示对话框，如果选择确定
            if (of.ShowDialog() == DialogResult.OK)
            {
                isSaving = true;

                int failCount = 0; //失败计数器

                //获取文件路径
                string[] nameList = of.FileNames;

                //显示进度条窗口
                ProgressShower progress = new ProgressShower();
                progress.Location = this.Location; 
                progress.progressBar1.Maximum = nameList.Count();

                progress.Show();

                //previewGenePack备份
                PreviewGeneClass temp = previewGenePack.DeepClone();

                foreach (string path in nameList)
                {

                    //生成完整保存路径
                    string fullpath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + "(framed)" + ".jpg";
                    //生成pack
                    PreviewGeneClass newPack = PictureFunctions.Path2Pack(previewGenePack, path);
                    //如果无法生成pack，计数器加1，跳过本次遍历
                    if (newPack == null)
                    {
                        failCount += 1;
                        continue;
                    }

                    //根据pack保存图像
                    PictureFunctions.Save2Path(PictureFunctions.DrawPreview(newPack), fullpath);
                    progress.progressBar1.Value += 1;
                }
                progress.Close(); //关闭进度条显示
                MessageBox.Show("Process done. Failed :" + failCount.ToString());

                previewGenePack = temp; //回归备份
                previewGenePack.bitmap = PictureFunctions.ResizeToStandard(rawImage, 802);
                PreviewUpdate();

                isSaving = false;
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// 窗体关闭事件n
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frm_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isSaving)
            {
                MessageBox.Show("Save operation is in progress, please do not close the window.");
            }
            else
            {
                System.Environment.Exit(0); //彻底关闭程序
            }
        }
    }
}