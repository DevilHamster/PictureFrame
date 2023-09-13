using Force.DeepCloner;
using System.CodeDom;
using System.Drawing.Text;

namespace PictureFrame
{
    public partial class frm_Main : Form
    {
        #region Informations
        string? filePath; //��ǰ�򿪵�ͼ����ļ�·��
        Bitmap rawImage;
        #endregion

        #region Lists
        string[] brandList = new string[]
        {
            //�������
            "SONY  ����", //0
            "Nikon  �῵", //1
            "Canon  ����", //2
            //"FUJIFILM  ��ʿ", //3
            //"PENTAX  ����", //4
            //"Ricoh  ���", //5
            //"Leica  �⿨", //6
            //"Panasonic  ����", //7
            //"Hasselblad  ����", //8
            //"Kodak  �´�", //9
            //"Olympus  ���ְ�˹","Insta360  Ӱʯ","SIGMA  ����",

            //�ֻ�
            //"Apple  ƻ��","HUAWEI  ��Ϊ","VIVO","SUMSUNG  ����",
            //"OPPO","HONOR  ��ҫ","ONEPLUS  һ��","MEIZU  ����","XiaoMi  С��",
            //"LG","realme  ����",

            //���˻�
            //"DJI  ��",

            //�˶����
            //"GoPro","SJCAM  ��Ӱ","AKASO"
        }; //�豸Ʒ���б�

        #endregion

        #region flags
        bool isEditing = false; //��ǰ�Ƿ����ڴ���ͼ��
        bool isSaving = false; //��ǰ�Ƿ����ڱ���ͼ���Ԥ��
        #endregion

        #region classes
        PreviewGeneClass previewGenePack; //����Ԥ��ͼ������ݰ���������ͼ���ʵ����
        BarStruct barStruct;
        #endregion

        public frm_Main()
        {
            InitializeComponent();
            //this.DoubleBuffered = true; //����˫����

            #region ������ʼ��
            timer1.Interval= 1000;
            //��Ϣ���ߴ����
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
            //logo���������ݣ������cbox_LogoList.DataSource = brandList�󶨣��ᱨ��
            foreach (string brand in brandList)
            {
                cbox_LogoList.Items.Add(brand);
            }

            //��ȡϵͳ�Դ���Ӣ�����壬��cbox_date_Font�ڳ����������Լ���ʽ(���߼�)
            //�����������ʽ����Ϊ������������
            //�ڳ���û������ͼ��ʱ���������߼�������ʼ��һ����ʾ
            //�ڳ�������ͼ��ʱ������ֻ�����߼����������û��趨������previewGenePack��������ʽ�б��߼�Ҳ�����߼������߼�Ϊ��������ı�ѡ����߼�Ϊ����previewGenePack
            InstalledFontCollection MyFont = new InstalledFontCollection();
            foreach (FontFamily family in MyFont.Families)
            {
                cbox_date_Font.Items.Add(family.Name);
                cbox_picInfo_Font.Items.Add(family.Name);
            }
            //�����Arial��ʹ��Arial������ѡ���б��һ��
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
            //�ڳ�����FontStyle
            FontStyleUpdate(new FontFamily(cbox_date_Font.SelectedItem.ToString()), cbox_date_Style);
            FontStyleUpdate(new FontFamily(cbox_picInfo_Font.SelectedItem.ToString()), cbox_picInfo_Style);

            //����ȫ��
            ConditionCheck(isEditing);

            #endregion
        }

        private void frm_Main_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// ���Open Picture���򿪱���ͼ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmi_OpenPicture_Click(object sender, EventArgs e)  
        {
            //�ڴ汣��
            GC.Collect();
            rawImage = null;

            #region �򿪶Ի��򣬻�ȡ�ļ�·��
            //ʵ����һ�����ļ��ĶԻ���
            OpenFileDialog of = new OpenFileDialog();
            //�������ѡ
            of.Multiselect = false;
            //���öԻ������
            of.Title = "Please choose the image to edit";
            //����ָ��ͼ������
            of.Filter = "Image Files|*.jpg";
            //��ʾ�Ի������ѡ��ȷ��
            if (of.ShowDialog() == DialogResult.OK)
            {
                //��ȡ�ļ�·��
                filePath = of.FileName;
            }
            else
            {
                return;
            }
            #endregion

            #region �����ļ�·��������ͼ������һ��previewGenePack��������֤��ȫ������previewGenePack�����ݻ���һ��Ԥ��ͼ����

            //����ʹ��һ��ResizeToStandard�����ļ�ʱ�����ļ�ʮ���룬��ʹ�����ϵĲ���������Ϊ����
            //pictureBox�ؼ���߾�Ϊ802
            rawImage = PictureFunctions.ReadFromStream(filePath);
            Bitmap img = PictureFunctions.ResizeToStandard(rawImage, 802);
            previewGenePack = new PreviewGeneClass(img);

            //����previewGenePack���������ؼ�����(����ؼ�������)
            //previewGenePack�����������ֲ���ͨ���ؼ���ȡ

            //cbox_date_Fontһ����ѡ�����ݣ����������ж�
            previewGenePack.timeFont = new FontFamily(cbox_date_Font.SelectedItem.ToString()); //��ͼƬʱ��ȡ��ǰѡ������
            previewGenePack.infoFontFamily = new FontFamily(cbox_picInfo_Font.SelectedItem.ToString());
            //�����ѡ��������ʽ��previewGenePack��Ӧ��������
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

            #region ���ÿ���
            //���пؼ�(��Open Picture��)��δ��ͼ�������±�������
            //һ�������һ��ͼ�񣬴˺�isEditingΪtrue
            isEditing = true; ConditionCheck(isEditing);
            //previewGenePack���£�����rbtn��λ
            rbtn_InfoStrip_Beneath.Checked = true; rbtn_InfoStrip_Right.Checked = false;
            #endregion

            #region ��ȡͼ����Ϣ������һ��JPGInfo���������²������棬�Լ�previewGenePack���ֲ���

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
            InfoUpdate(previewGenePack); //����Info��Ϣ 

            #endregion

            #region ���¿ؼ�

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

            #region ����previewGenePack������Ԥ��ͼ��pictureBox��

            //����ͼ��
            PreviewUpdate();

            #endregion

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //stopwatch.Stop();
            //TimeSpan timespan = stopwatch.Elapsed;
            //tbox_testMessage.Text = timespan.TotalMilliseconds.ToString();
        }

        /// <summary>
        /// ѡ��Beneath������previewGenePack�����»�ͼ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtn_InfoStrip_Beneath_CheckedChanged(object sender, EventArgs e)
        {
            previewGenePack.stripLocation = InfoStripLocations.Beneath;
            //����ͼ��
            PreviewUpdate();

            //�ؼ��䶯
            lbl_info_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.stripSize * 100) + "% of the height of the image";
            lbl_logo_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.logoSize * 100) + "% of the height of the Info Strip";
            lbl_date_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.timeSize * 100) + "% of the height of the Info Strip";
            lbl_picInfo_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.infoSize * 100) + "% of the height of the Info Strip";
            lbl_margin_size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.marginSize * 100) + "% of the width of raw image";
        }

        /// <summary>
        /// ѡ��Right������previewGenePack�����»�ͼ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtn_InfoStrip_Right_CheckedChanged(object sender, EventArgs e)
        {
            previewGenePack.stripLocation = InfoStripLocations.Right;
            //����ͼ��
            PreviewUpdate();

            //�ؼ��䶯
            lbl_info_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.stripSize * 100) + "% of the width of the image";
            lbl_logo_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.logoSize * 100) + "% of the width of the Info Strip";
            lbl_date_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.timeSize * 100) + "% of the width of the Info Strip";
            lbl_picInfo_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.infoSize * 100) + "% of the width of the Info Strip";
            lbl_margin_size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.marginSize * 100) + "% of the height of raw image";
        }

        /// <summary>
        /// ��Ϣ���ߴ������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_StripSize_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //�ȸ���previewgenePack������previewGenePack.stripSize���ΪbarStruct.strip_Size_Max�����ΪbarStruct.strip_Size_Min
            previewGenePack.stripSize = barStruct.strip_Size_Min +
                (barStruct.strip_Size_Max - barStruct.strip_Size_Min) * ((float)bar_StripSize._Value / (float)bar_StripSize._Max);

            //������ؿؼ�
            if (previewGenePack.stripLocation == InfoStripLocations.Beneath)
            {
                lbl_info_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.stripSize * 100) + "% of the width of the image";
            }
            else
            {
                lbl_info_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.stripSize * 100) + "% of the height of the image";
            }

            //�����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }

        /// <summary>
        /// Logo�ߴ������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_logoSize_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //�ȸ���previewgenePack
            previewGenePack.logoSize = barStruct.logo_Size_Min + 
                (barStruct.logo_Size_Max - barStruct.logo_Size_Min) * ((float)bar_logoSize._Value / (float)bar_logoSize._Max);
            //������ؿؼ�
            if (previewGenePack.stripLocation == InfoStripLocations.Beneath)
            {
                lbl_logo_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.logoSize * 100) + "% of the height of the Info Strip";
            }
            else
            {
                lbl_logo_Size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.logoSize * 100) + "% of the width of the Info Strip";
            }

            //�����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }

        /// <summary>
        /// LogoX������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_logoX_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //�ȸ���previewgenePack
            previewGenePack.logoX = barStruct.logo_X_Min +
                (barStruct.logo_X_Max - barStruct.logo_X_Min) * ((float)bar_logoX._Value / (float)bar_logoX._Max);
            //������label��ʾ
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }

        /// <summary>
        /// LogoY������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_logoY_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //�ȸ���previewgenePack
            previewGenePack.logoY = barStruct.logo_Y_Min +
                (barStruct.logo_Y_Max - barStruct.logo_Y_Min) * ((float)bar_logoY._Value / (float)bar_logoY._Max);
            //������label��ʾ
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }

        /// <summary>
        /// logo�б�ѡ������䶯������previewgenePack�����������logo��ʾ���ػ棬���û��logo��ʾ���޶���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbox_LogoList_SelectedIndexChanged(object sender, EventArgs e)
        {
            //��ǰδѡ��ֵ������
            if (cbox_LogoList.SelectedIndex == -1) { return; }

            else
            {
                //����previewgenePack������previewGenePack.brandIndex����
                previewGenePack.brandIndex = cbox_LogoList.SelectedIndex;
                //�ػ�
                PreviewUpdate();
            }
        }

        /// <summary>
        /// �����logoX�ع���ֵ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_logo_midAlignX_Click(object sender, EventArgs e)
        {
            bar_logoX._Value = (bar_logoX._Max - bar_logoX._Min) / 2;
        }

        /// <summary>
        /// �����logoY�ع���ֵ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_logo_midAlignY_Click(object sender, EventArgs e)
        {
            bar_logoY._Value = (bar_logoY._Max - bar_logoY._Min) / 2;
        }

        /// <summary>
        /// ����ѡ����£�����������߼���������ʽ�ı��߼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbox_date_Font_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (previewGenePack != null)
            {
                FontFamily temp = new FontFamily(cbox_date_Font.SelectedItem.ToString());
                //����previewGenePack����
                previewGenePack.timeFont = temp;
                //����cbox_date_Style��ʾ
                FontStyleUpdate(temp, cbox_date_Style);
                //����ͼ��
                PreviewUpdate();
            }
        }

        /// <summary>
        /// ����ѡ����£�����������߼���������ʽ�ı��߼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbox_picInfo_Font_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (previewGenePack != null)
            {
                FontFamily temp = new FontFamily(cbox_picInfo_Font.SelectedItem.ToString());
                //����previewGenePack����
                previewGenePack.infoFontFamily = temp;
                //����cbox_date_Style��ʾ
                FontStyleUpdate(temp, cbox_picInfo_Style);
                //����ͼ��
                PreviewUpdate();
            }
        }

        /// <summary>
        /// ������ʽѡ����£�����previewGenePack
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
                //����ͼ��
                PreviewUpdate();
            }
        }

        /// <summary>
        /// ������ʽѡ����£�����previewGenePack
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
                //����ͼ��
                PreviewUpdate();
            }
        }

        /// <summary>
        /// timeX������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_timeX_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //�ȸ���previewgenePack
            previewGenePack.timeX = barStruct.time_X_Min +
                (barStruct.time_X_Max - barStruct.time_X_Min) * ((float)bar_timeX._Value / (float)bar_timeX._Max);
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }

        /// <summary>
        /// timeY������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_timeY_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //�ȸ���previewgenePack
            previewGenePack.timeY = barStruct.time_Y_Min +
                (barStruct.time_Y_Max - barStruct.time_Y_Min) * ((float)bar_timeY._Value / (float)bar_timeY._Max);
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }

        /// <summary>
        /// timeSize������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_timeSize_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //�ȸ���previewgenePack
            previewGenePack.timeSize = barStruct.time_Size_Min +
                (barStruct.time_Size_Max - barStruct.time_Size_Min) * ((float)bar_timeSize._Value / (float)bar_timeSize._Max);
            //������ؿؼ�
            if (previewGenePack.stripLocation == InfoStripLocations.Beneath)
            {
                lbl_date_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.timeSize * 100) + "% of the height of the Info Strip";
            }
            else
            {
                lbl_date_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.timeSize * 100) + "% of the width of the Info Strip";
            }
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }

        /// <summary>
        /// timeTrans������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_timeTrans_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //����previewGenePack
            previewGenePack.timeTranParent = bar_timeTrans._Value;
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }

        /// <summary>
        /// infoX������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_infoX_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //�ȸ���previewgenePack
            previewGenePack.infoX = barStruct.info_X_Min +
                (barStruct.info_X_Max - barStruct.info_X_Min) * ((float)bar_infoX._Value / (float)bar_infoX._Max);
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }

        /// <summary>
        /// infoY������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_infoY_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //�ȸ���previewgenePack
            previewGenePack.infoY = barStruct.info_Y_Min +
                (barStruct.info_Y_Max - barStruct.info_Y_Min) * ((float)bar_infoY._Value / (float)bar_infoY._Max);
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }

        /// <summary>
        /// infoSize������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_infoSize_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //�ȸ���previewgenePack
            previewGenePack.infoSize = barStruct.info_Size_Min +
                (barStruct.info_Size_Max - barStruct.info_Size_Min) * ((float)bar_infoSize._Value / (float)bar_infoSize._Max);
            //������ؿؼ�
            if (previewGenePack.stripLocation == InfoStripLocations.Beneath)
            {
                lbl_picInfo_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.infoSize * 100) + "% of the height of the Info Strip";
            }
            else
            {
                lbl_picInfo_FontSize.Text = "Size :" + string.Format("{0:F2}", previewGenePack.infoSize * 100) + "% of the width of the Info Strip";
            }
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }

        /// <summary>
        /// infoTrans������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_infoTrans_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //����previewGenePack
            previewGenePack.infoTransparent = bar_infoTrans._Value;
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }

        /// <summary>
        /// ʱ����ʾ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_date_midAlign_Click(object sender, EventArgs e)
        {
            bar_timeX._Value = (bar_timeX._Max - bar_timeX._Min) / 2;
        }

        /// <summary>
        /// ʱ����ʾ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_date_midAlign2_Click(object sender, EventArgs e)
        {
            bar_timeY._Value = (bar_timeY._Max - bar_timeY._Min) / 2;
        }

        /// <summary>
        /// info��ʾ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_picInfo_midAlign_Click(object sender, EventArgs e)
        {
            bar_infoX._Value = (bar_infoX._Max - bar_infoX._Min) / 2;
        }

        /// <summary>
        /// info��ʾ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_picInfo_midAlign2_Click(object sender, EventArgs e)
        {
            bar_infoY._Value = (bar_infoY._Max - bar_infoY._Min) / 2;
        }

        /// <summary>
        /// �߿��С������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bar_marginSize_ValueChanged(object sender, ControlBarEventArgs e)
        {
            //�ȸ���previewgenePack
            previewGenePack.marginSize = barStruct.margin_Size_Min +
                (barStruct.margin_Size_Max - barStruct.margin_Size_Min) * ((float)bar_marginSize._Value / (float)bar_marginSize._Max);

            //���¿ؼ�
            if (previewGenePack.stripLocation == InfoStripLocations.Beneath)
            {
                lbl_margin_size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.marginSize * 100) + "% of the width of raw image";
            }
            else 
            {
                lbl_margin_size.Text = "Size :" + string.Format("{0:F2}", previewGenePack.marginSize * 100) + "% of the height of raw image";
            }

            //�����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }

        /// <summary>
        /// ���ڸ����¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbox_date_TextChanged(object sender, EventArgs e)
        {
            previewGenePack.time = tbox_date.Text;
            //�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }

        /// <summary>
        /// ������������¼�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbox_picInfo_Camera_TextChanged(object sender, EventArgs e)
        {
            InfoUpdate(previewGenePack);
            //�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }
        private void tbox_picInfo_F_TextChanged(object sender, EventArgs e)
        {
            InfoUpdate(previewGenePack);
            //�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }
        private void tbox_picInfo_Expo_TextChanged(object sender, EventArgs e)
        {
            InfoUpdate(previewGenePack);
            //�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }
        private void tbox_picInfo_ISO_TextChanged(object sender, EventArgs e)
        {
            InfoUpdate(previewGenePack);
            //�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }
        private void tbox_picInfo_Focal_TextChanged(object sender, EventArgs e)
        {
            InfoUpdate(previewGenePack);
            //�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }
        private void tbox_picInfo_Bias_TextChanged(object sender, EventArgs e)
        {
            InfoUpdate(previewGenePack);
            //�ػ�ͼ��Ԥ��
            PreviewUpdate();
        }


        #region functions

        /// <summary>
        /// ����������ʽѡ��(���߼�)
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
        /// ״̬��飬����ȫ��
        /// </summary>
        public void ConditionCheck(bool isEditing)
        {
            //�����ǰû��ͼ������
            if (!isEditing)
            {
                foreach (Control control in this.Controls)
                {
                    control.Enabled = false;
                }
                //ֻ�������ֹ���
                menuStrip1.Enabled = true;
                tsmi_SavePicture.Enabled = false;
                tsmi_SavePreset.Enabled = false;
                tsmi_OpenPreset.Enabled = false;
                tsmi_BatchApply.Enabled = false;
                useToolStripMenuItem.Enabled = false;
                return;
            }
            //�����ǰ�Ѿ���ͼ�񣬼���һ���ؼ��������һ���ؼ�Enable==true���򲻼�����������֮˵����ǰ�ǵ�һ�δ�ͼ��ȫ������
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
        /// �������������Ϣ
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
        /// ����Ԥ��
        /// </summary>
        public void PreviewUpdate()
        {
            if (pb_Preview.Image != null)
            {
                pb_Preview.Image.Dispose();
            }

            pb_Preview.Image = PictureFunctions.DrawPreview(previewGenePack); //���¿ؼ���ʾ
        }

        /// <summary>
        /// �ؼ�����
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

            //����ʱ����Ϣ����
            //�����ȡ�������岻��ϵͳ�У��л���Arial���һλ
            if (!cbox_date_Font.Items.Contains(previewGenePack.timeFont.Name))
            {
                MessageBox.Show("Font not installed, will switch to default font.");
                if (cbox_date_Font.Items.Contains("Arial"))
                {
                    cbox_date_Font.SelectedItem = "Arial";
                    previewGenePack.timeFont = new FontFamily("Arial"); //����
                }
                else
                {
                    cbox_date_Font.SelectedIndex = 0;
                    previewGenePack.timeFont = new FontFamily(cbox_date_Font.Items[0].ToString()); //����
                }
            }
            else
            {
                cbox_date_Font.SelectedItem = previewGenePack.timeFont.ToString();
            }

            //�������������Ϣ����
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
        /// ���ļ�·��������ͼƬ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmi_SavePicture_Click(object sender, EventArgs e)
        {
            //�����ļ�����
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

                Bitmap temp = previewGenePack.bitmap; //�ݴ�Ԥ��ͼ��

                previewGenePack.bitmap = rawImage;
                Bitmap final = PictureFunctions.DrawPreview(previewGenePack);
                PictureFunctions.Save2Path(final, saveFileDialog.FileName);

                previewGenePack.bitmap = temp; //�Ļ�Ԥ��ͼ��

                isSaving = false;

                MessageBox.Show("Saved Successfully.");
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// ���ļ�·��������Ԥ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmi_SavePreset_Click(object sender, EventArgs e)
        {
            //�����ļ�����
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = "Save Preset as:",
                RestoreDirectory = true,
                Filter = "Preset Files|*.json"
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                isSaving = true;

                //��Ԥ�豣�����
                JSONinteractions.Parameters2Json(previewGenePack, saveFileDialog.FileName.ToString());

                isSaving = false;
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// ��ȡԤ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmi_OpenPreset_Click(object sender, EventArgs e)
        {
            //ʵ����һ�����ļ��ĶԻ���
            OpenFileDialog of = new OpenFileDialog();
            //�������ѡ
            of.Multiselect = false;
            //���öԻ������
            of.Title = "Please choose the preset to apply";
            //����ָ��ͼ������
            of.Filter = "Preset Files|*.json";
            //��ʾ�Ի������ѡ��ȷ��
            if (of.ShowDialog() == DialogResult.OK)
            {
                //��ȡ�ļ�·��
                filePath = of.FileName;
                //���previewGenePack
                JSONinteractions.Json2Parameters(previewGenePack, filePath);

                //����previewgenePack�ػ�ͼ��Ԥ��
                PreviewUpdate();

                //�ؼ�����
                ControlsUpdate();
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// ʹ����ǶԤ��beneathMidlign
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void beneathMidlignToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //���previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Beneath_MidAlign);
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();

            //�ؼ�����
            ControlsUpdate();
        }

        /// <summary>
        /// ÿ��1s����һ���ڴ�
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
        /// ʹ����ǶԤ��beneathMidlign2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void beneathMidlignToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            //���previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Beneath_MidAlign2);
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();

            //�ؼ�����
            ControlsUpdate();
        }

        /// <summary>
        /// ʹ����ǶԤ��beneathMidlign3
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void beneathMidlignToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            //���previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Beneath_MidAlign3);
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();

            //�ؼ�����
            ControlsUpdate();
        }

        private void beneathCorner����ͼToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //���previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Beneath_Corner);
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();

            //�ؼ�����
            ControlsUpdate();
        }

        private void rightCorner���ι�ͼToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //���previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Beneath_Corner2);
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();

            //�ؼ�����
            ControlsUpdate();
        }

        private void rightCorner����ͼToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //���previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Beneath_Corner3);
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();

            //�ؼ�����
            ControlsUpdate();
        }

        private void rightCorner����ͼToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //���previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Right_Corner);
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();

            //�ؼ�����
            ControlsUpdate();
        }

        private void rightCorner���ι�ͼToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            //���previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Right_Corner2);
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();

            //�ؼ�����
            ControlsUpdate();
        }

        private void rightCorner����1��ͼToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //���previewGenePack
            JSONinteractions.Json2Parameters(previewGenePack, Properties.Resources.Right_Corner3);
            //����previewgenePack�ػ�ͼ��Ԥ��
            PreviewUpdate();

            //�ؼ�����
            ControlsUpdate();
        }

        /// <summary>
        /// ���������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmi_BatchApply_Click(object sender, EventArgs e)
        {
            //ʵ����һ�����ļ��ĶԻ���
            OpenFileDialog of = new OpenFileDialog();
            //�������ѡ
            of.Multiselect = true;
            //���öԻ������
            of.Title = "Please choose the images to edit";
            //����ָ��ͼ������
            of.Filter = "Image Files|*.jpg";
            //��ʾ�Ի������ѡ��ȷ��
            if (of.ShowDialog() == DialogResult.OK)
            {
                isSaving = true;

                int failCount = 0; //ʧ�ܼ�����

                //��ȡ�ļ�·��
                string[] nameList = of.FileNames;

                //��ʾ����������
                ProgressShower progress = new ProgressShower();
                progress.Location = this.Location; 
                progress.progressBar1.Maximum = nameList.Count();

                progress.Show();

                //previewGenePack����
                PreviewGeneClass temp = previewGenePack.DeepClone();

                foreach (string path in nameList)
                {

                    //������������·��
                    string fullpath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + "(framed)" + ".jpg";
                    //����pack
                    PreviewGeneClass newPack = PictureFunctions.Path2Pack(previewGenePack, path);
                    //����޷�����pack����������1���������α���
                    if (newPack == null)
                    {
                        failCount += 1;
                        continue;
                    }

                    //����pack����ͼ��
                    PictureFunctions.Save2Path(PictureFunctions.DrawPreview(newPack), fullpath);
                    progress.progressBar1.Value += 1;
                }
                progress.Close(); //�رս�������ʾ
                MessageBox.Show("Process done. Failed :" + failCount.ToString());

                previewGenePack = temp; //�ع鱸��
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
        /// ����ر��¼�n
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
                System.Environment.Exit(0); //���׹رճ���
            }
        }
    }
}