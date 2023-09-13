using Force.DeepCloner;
using MetadataExtractor;

namespace PictureFrame
{
    /// <summary>
    /// 与jpg图像处理的功能存放于此
    /// </summary>
    static class PictureFunctions
    {
        /// <summary>
        /// 根据文件路径，获取JPGInfo
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static JPGInfo GetInfo(string path)
        {
            JPGInfo jpgInfo = new JPGInfo(); //创建结构体
            if (path == "" || path == null) { return jpgInfo; }
            var rmd = ImageMetadataReader.ReadMetadata(path);
            foreach (var data in rmd)
            {
                foreach (var tag in data.Tags)
                {
                    //tag.Name查找表：https://github.com/drewnoakes/metadata-extractor-dotnet/blob/master/MetadataExtractor/Formats/Exif/ExifDirectoryBase.cs
                    switch (tag.Name)
                    {
                        case "Model":
                            jpgInfo.camera = tag.Description;
                            break;
                        case "Date/Time Original":
                            jpgInfo.date = tag.Description;
                            break;
                        case "F-Number":
                            jpgInfo.f_stop = tag.Description;
                            break;
                        case "Shutter Speed Value":
                            jpgInfo.expoTime = tag.Description;
                            break;
                        case "ISO Speed Ratings":
                            jpgInfo.iso = tag.Description;
                            break;
                        case "Focal Length":
                            jpgInfo.focalLength = tag.Description;
                            break;
                        case "Exposure Bias Value":
                            jpgInfo.evBias = tag.Description;
                            break;
                        case "Make":
                            jpgInfo.maker = tag.Description;
                            break;
                    }
                }
            }
            return jpgInfo;
        }

        /// <summary>
        /// 保存图像至路径
        /// </summary>
        /// <param name="path"></param>
        public static void Save2Path(Bitmap bitmap, string path)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                //图片转字节流
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    //字节流写入文件
                    fs.Write(stream.ToArray(), 0 ,(int)stream.ToArray().Length);
                }
            }
            System.GC.Collect();
        }

        /// <summary>
        /// 根据JPGInfo的信息，返回品牌对应的编号
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static int GetBrandIndex(string tag)
        {
            int index = 0;
            switch (tag)
            {
                case "SONY":
                    index = 0; break;
                case "NIKON CORPORATION":
                    index = 1; break;
                case "Canon":
                    index = 2; break;
            }
            if (index == 0 && tag != "SONY")
            {
                return -1;
            }
            else
            {
                return index;
            }
        }

        /// <summary>
        /// 为图像添加空白信息条
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Bitmap AddStrip(Bitmap img, InfoStripLocations infoStripLocation, float stripSize)
        {
            int w = img.Width; int h = img.Height; //两边像素数
            if (infoStripLocation == InfoStripLocations.Beneath)
            {
                //创建空白画布
                Bitmap bitmap = new Bitmap(w, h + (int)(w*stripSize));
                Graphics g = Graphics.FromImage(bitmap);

                //先在图像下方填充白色
                g.FillRectangle(Brushes.White, new Rectangle(0,h,w, h + (int)(w * stripSize)));
                //再在x=0,y=0处画上图像
                g.DrawImage(img, 0, 0, w, h);

                return bitmap;
            }
            else
            {
                //创建空白画布
                Bitmap bitmap = new Bitmap(w+(int)(h*stripSize), h);
                Graphics g = Graphics.FromImage(bitmap);
                //先在图像右侧填充白色
                g.FillRectangle(Brushes.White, new Rectangle(w, 0, w + (int)(h * stripSize), h));
                //再在图像左侧画上图像
                g.DrawImage(img, 0, 0, w, h);

                return bitmap;
            }

        }

        /// <summary>
        /// 获取图像长边像素
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static int GetLongEdge(Bitmap img)
        {
            return Math.Max(img.Width, img.Height);
        }

        /// <summary>
        /// 将图像压缩，控长边像素
        /// </summary>
        /// <param name="img">原图</param>
        /// <param name="standard">压缩后图像长边像素数</param>
        public static Bitmap ResizeToStandard(Bitmap img, int standard)
        { 
            Size size = new Size();
            if (img.Width == GetLongEdge(img))
            {
                //如果图像长边为宽边
                size.Width = standard;
                size.Height = (int)Math.Round((float)(standard * img.Height)/(float)img.Width);
                return new Bitmap(img, size);
            }
            else
            {
                //如果图像长边为高边
                size.Height = standard;
                size.Width = (int)Math.Round((float)(standard * img.Width)/(float)img.Height);
                return new Bitmap(img, size);
            }
        }

        /// <summary>
        /// 将图像根据指定高度等比缩放
        /// </summary>
        /// <param name="img">原图</param>
        /// <param name="height">指定高度</param>
        /// <returns></returns>
        public static Bitmap ResizeToDefHeight(Bitmap img, int height)
        {
            if (height == 0)
            {
                return null;
            }
            Size size = new Size();
            size.Height = height;
            size.Width = (int)Math.Round((float)(height * img.Width) / (float)img.Height);
            return new Bitmap(img, size);
        }

        /// <summary>
        /// 给出文件路径，返回Bitmap
        /// 用此函数，实现打开文件时实现不锁定文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Bitmap ReadFromStream(string path)
        {
            //创建文件流
            FileStream fs = new FileStream(path,FileMode.Open,FileAccess.Read);

            int filelength = (int)fs.Length; //文件长度

            Byte[] image = new Byte[filelength]; //建立字节数组

            //从当前流异步读取字节序列
            fs.ReadAsync(image, 0, filelength);
            Bitmap result = new Bitmap(fs, true); //通过字节流读取文件，并选择是否启用颜色校正
            fs.Close();

            return result;
        }  

        /// <summary>
        /// 在图像上添加Logo图案(void)
        /// </summary>
        /// <param name="img">(带有信息条的)原始图像</param>
        /// <param name="stripSize">信息条相对尺寸值</param>
        /// <param name="stripLocation">信息条位置</param>
        /// <param name="brandIndex">品牌编号</param>
        /// <param name="size">Logo相对尺寸值</param>
        /// <param name="x">Logo相对x坐标</param>
        /// <param name="y">Logo相对y坐标</param>
        /// <returns>添加Logo图案后的图像</returns>
        public static void DrawLogo(Bitmap img, float stripSize, InfoStripLocations stripLocation, int? brandIndex, float size, float x, float y)
        {
            //如果信息栏在下方
            if (stripLocation == InfoStripLocations.Beneath)
            {
                //获取logo图片并根据指定参数压缩
                //logo图像高度 = img.Width * stripSize * size
                Bitmap logo = ResizeToDefHeight(GetLogo((int)brandIndex), (int)Math.Round(img.Width * stripSize * size));

                //如果没有logo图像信息，直接返回
                if (logo == null) { return ; }

                //获取绘制坐标，需要先将logo图像resize，再根据x和y推算坐标
                Point coor = GetCoorLogo(x, y, img.Width, (int)Math.Round(img.Width * stripSize), logo.Width, logo.Height);
                //coor纵坐标需要加上...
                coor.Y += img.Height-(int)Math.Round(img.Width * stripSize);

                //在原图上创建图层
                Graphics g = Graphics.FromImage(img);
                //在对应坐标绘制图像
                g.DrawImage(logo, coor);
                return ;
            }
            //如果信息栏在右侧
            else
            {
                //获取logo图片并根据指定参数压缩
                //logo图像高度 = img.Height * stripSize * size
                Bitmap logo = ResizeToDefHeight(GetLogo((int)brandIndex), (int)Math.Round(img.Height * stripSize * size));

                //如果没有logo图像信息，直接返回
                if (logo == null) { return ; }

                //旋转图像，顺时针旋转270度
                logo.RotateFlip(RotateFlipType.Rotate270FlipNone);

                //获取绘制坐标
                //信息条宽为(int)Math.Round(img.Height * stripSize),高度为img.Height
                Point coor = GetCoorLogo(x, y, (int)Math.Round(img.Height * stripSize), img.Height, logo.Width, logo.Height);
                //coor横坐标需要加上...
                coor.X += img.Width - (int)Math.Round(img.Height * stripSize);

                //在原图上创建图层
                Graphics g = Graphics.FromImage(img);
                //在对应坐标绘制图像
                g.DrawImage(logo, coor);
                return ;
            }
        }

        /// <summary>
        /// 在图像上添加文字图案 (void)
        /// </summary>
        /// <param name="img"></param>
        /// <param name="textImage"></param>
        /// <param name="stripLocation"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="stripSize"></param>
        /// <returns></returns>
        public static void DrawTextImage(Bitmap img, Bitmap textImage, InfoStripLocations stripLocation, float x, float y, float stripSize)
        {
            //如果信息栏在下方
            if (stripLocation == InfoStripLocations.Beneath)
            {
                //无图像压缩过程

                //如果没有图案传入，直接返回
                if (textImage == null) { return ; }

                //获取绘制坐标，绘制图案的x坐标：(img.Width-textImage)*x
                //y坐标：rawHeight + (int)((img.Height-rawHeight-textImage.Height)*y)
                int rawHeight = img.Height - (int)(img.Width * stripSize);
                Point coor = new Point( (int)Math.Round((img.Width - textImage.Width) * x), rawHeight + (int)Math.Round((img.Height - rawHeight - textImage.Height) * y));

                //在原图上创建图层
                Graphics g = Graphics.FromImage(img);
                //在对应坐标绘制图像
                g.DrawImage(textImage, coor);
                //return img;
            }
            //如果信息栏在右侧
            else
            {
                //如果没有图案传入，直接返回
                if (textImage == null) { return ; }

                //旋转图像，顺时针旋转270度
                textImage.RotateFlip(RotateFlipType.Rotate270FlipNone);


                int rawWidth = img.Width - (int)(img.Height * stripSize);

                //(int)Math.Round((img.Height - textImage.Height) * y) 
                Point coor = new Point(rawWidth + (int)Math.Round((img.Width - rawWidth - textImage.Width) * x), (int)Math.Round((img.Height - textImage.Height) * y));

                //在原图上创建图层
                Graphics g = Graphics.FromImage(img);
                //在对应坐标绘制图像
                g.DrawImage(textImage, coor);
                //return img;
            }
        }

        /// <summary>
        /// 根据参数，获取在图像上绘制logo的坐标
        /// </summary>
        /// <param name="x">logo相对x坐标</param>
        /// <param name="y">logo相对y坐标</param>
        /// <param name="W">信息条宽度</param>
        /// <param name="H">信息条高度</param>
        /// <param name="w">logo宽度</param>
        /// <param name="h">logo高度</param>
        /// <returns></returns>
        public static Point GetCoorLogo(float x, float y, int W, int H, int w, int h)
        {
            Point point = new Point();
            point.X = (int)(x * (W - w));
            point.Y = (int)(y * (H - h));
            return point;
        }

        /// <summary>
        /// 根据索引值获取对应logo图片文件
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Bitmap GetLogo(int index)
        {
            Bitmap bmp = null;
            switch (index)
            {
                case 0:
                    bmp = Properties.Resources.logo_Sony; break;
                case 1:
                    bmp = Properties.Resources.logo_Nikon; break;
                case 2:
                    bmp = Properties.Resources.logo_Canon; break;
            }
            return bmp;
        }

        /// <summary>
        /// 根据文字生成文字图像
        /// </summary>
        /// <param name="text">文字内容</param>
        /// <param name="fontFamily">文字字体:FontFamily</param>
        /// <param name="fontStyle">文字字体样式:FontStyle</param>
        /// <param name="transparent">透明度, 0-255</param>
        /// <param name="height">文字高度(像素数)</param>
        /// <returns></returns>
        public static Bitmap GeneTextImage(string text, FontFamily fontFamily, FontStyle fontStyle, int transparent, int height)
        {
            if (text == "" || text == null){ return null; }
            else
            {
                //创建空白bitmap
                Bitmap bmp = new Bitmap(1,1);
                Graphics g = Graphics.FromImage(bmp);

                StringFormat format = new StringFormat(StringFormatFlags.NoWrap); 
                Font font = new Font(fontFamily, 80, fontStyle); //固定字号80磅
                //计算绘制文字所需区域大小，创建矩形区域绘图
                SizeF sizef = g.MeasureString(text, font, PointF.Empty, format);

                int Width = (int)(sizef.Width + 1);
                int Height = (int)(sizef.Height + 1);
                bmp.Dispose();

                bmp = new Bitmap(Width, Height);
                bmp.MakeTransparent();
                Rectangle rec = new Rectangle(0, 0, Width, Height);

                //绘图
                g = Graphics.FromImage(bmp);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                Brush brush = new SolidBrush(Color.FromArgb(transparent, 0, 0, 0));
                //g.FillRectangle(Brushes.White, rec); //填充背景色
                g.DrawString(text, font, brush, rec, format);

                //根据高度重置图像大小
                return ResizeToDefHeight(bmp, height);
                //return bmp;
            }
        }

        /// <summary>
        /// 获取文字高度像素
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="infoStripLocation"></param>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public static int GetTextHeight(Bitmap bmp, InfoStripLocations infoStripLocation, float stripSize, float textSize)
        {
            if (infoStripLocation == InfoStripLocations.Beneath)
            {
                return (int) (bmp.Width * stripSize * textSize);
            }
            else
            {
                return (int) (bmp.Height * stripSize * textSize);
            }
        }

        /// <summary>
        /// 为图像绘制边框，如Strip在下，左+上+右，如Strip在右，下+左+上
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Bitmap MakeMarigin(Bitmap bmp, float size, InfoStripLocations infoStripLocations)
        {
            if (bmp == null) { return null; }
            if(infoStripLocations == InfoStripLocations.Beneath)
            {
                //创建空白bitmap
                Bitmap board = new Bitmap((int)(bmp.Width * (1+2*size)), (int)(bmp.Height + bmp.Width*size));
                Graphics g = Graphics.FromImage(board);

                //画板填充白色
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, board.Width, board.Height));
                //绘制图像
                g.DrawImage(bmp, (Single)(bmp.Width*size), (Single)(bmp.Width * size), (Single)bmp.Width, (Single)bmp.Height);

                return board;
            }
            else
            {
                //创建空白bitmap
                Bitmap board = new Bitmap((int)(bmp.Width + bmp.Height*size), (int)(bmp.Height * (1 + 2*size)));
                Graphics g = Graphics.FromImage(board);

                //画板填充白色
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, board.Width, board.Height));
                //绘制图像
                g.DrawImage(bmp, (Single)(bmp.Height * size), (Single)(bmp.Height * size), (Single)bmp.Width, (Single)bmp.Height);

                return board;
            }
        }

        /// <summary>
        /// 根据所需数据，绘制预览图
        /// </summary>
        /// <param name="previewGenePack"></param>
        /// <returns></returns>
        public static Bitmap DrawPreview(PreviewGeneClass previewGenePack)
        {
            Bitmap result = null;

            //添加空白信息显示条
            result = AddStrip(previewGenePack.bitmap, previewGenePack.stripLocation, previewGenePack.stripSize); 

            //添加logo，更新result 
            if (previewGenePack.brandIndex != null)
            {
                DrawLogo(result, previewGenePack.stripSize, previewGenePack.stripLocation, previewGenePack.brandIndex, previewGenePack.logoSize, previewGenePack.logoX, previewGenePack.logoY);
            }

            //生成文字图案
            Bitmap timeTextImage = GeneTextImage(
                previewGenePack.time, 
                previewGenePack.timeFont,
                previewGenePack.timeFontStyle, 
                previewGenePack.timeTranParent, 
                GetTextHeight(previewGenePack.bitmap, previewGenePack.stripLocation, previewGenePack.stripSize, previewGenePack.timeSize)
                );
            //绘制文字图案
            if (timeTextImage != null)
            {
                DrawTextImage(
                    result,
                    timeTextImage,
                    previewGenePack.stripLocation,
                    previewGenePack.timeX,
                    previewGenePack.timeY,
                    previewGenePack.stripSize
                    );
            }

            //生成参数文字图案
            Bitmap infoTextImage = GeneTextImage(
                previewGenePack.info,
                previewGenePack.infoFontFamily,
                previewGenePack.infoFontStyle,
                previewGenePack.infoTransparent,
                GetTextHeight(previewGenePack.bitmap, previewGenePack.stripLocation, previewGenePack.stripSize, previewGenePack.infoSize)
                );
            //绘制文字图案
            if (infoTextImage != null)
            {
                DrawTextImage(
                    result,
                    infoTextImage,
                    previewGenePack.stripLocation,
                    previewGenePack.infoX,
                    previewGenePack.infoY,
                    previewGenePack.stripSize
                    );
            }

            if (previewGenePack.marginSize != 0)
            {
                result = MakeMarigin(result, previewGenePack.marginSize, previewGenePack.stripLocation);
            }
            else
            {
                return result;
            }
            return result;
        }

        /// <summary>
        /// 根据现有模板，根据新的图片文件更新出新的模板
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static PreviewGeneClass Path2Pack(PreviewGeneClass previewGenePack, string path) 
        {
            //获取图像信息，如果有信息缺失，放弃执行
            JPGInfo jpginfo = GetInfo(path);
            if (
                jpginfo.date == null ||
                jpginfo.camera == null ||
                jpginfo.f_stop == null ||
                jpginfo.expoTime == null ||
                jpginfo.iso == null ||
                jpginfo.focalLength == null ||
                jpginfo.evBias == null ||
                jpginfo.maker == null
                )
            {
                return null;
            }

            //创建现有模板的深表复制
            //有多深有待考究
            PreviewGeneClass newPack = previewGenePack.DeepClone();

            //newPack更新，包括bitmap字段更新，time字段和info字段更新，brandIndex更新
            newPack.bitmap = ReadFromStream(path);
            newPack.brandIndex = GetBrandIndex(jpginfo.maker);
            newPack.info = jpginfo.camera + " " +
                jpginfo.f_stop + " " +
                jpginfo.expoTime + " " + "ISO" + 
                jpginfo.iso + " " +
                jpginfo.focalLength + " " +  
                jpginfo.evBias;
            newPack.time = jpginfo.date;

            return newPack;
        }
    }
}
