# PictureFrame

## 概要：  
> 照片自动加框程序，支持自定义样式及批量处理。其具体功能包括本地图片的打开、添加图框并保存与批量处理，以及图框样式的自定义与保存  

## 功能说明：  
### :art:打开图像并进行图框样式编辑：  
通过*Picture-Open Picture*可打开图像，选择本地jpg格式图像后，程序将自动提取图像的exif信息，包括拍摄数据(光圈、快门、感光度等)、拍摄时间以及相机品牌，并为图像添加带有上述信息的图框。其中，可以更改的参数有：  
1. 图框的尺寸以及信息显示的位置(位于图像下部或位于图像右侧)
2. 相机品牌Logo(目前仅支持Sony\Nikon\Canon)及其大小、位置
3. 日期信息的内容、字体(可选字体以本地字体库为准)与字体样式(如Bold、Italic、Underline等)、大小、位置、透明度
4. 拍摄参数的内容、字体(可选字体以本地字体库为准)与字体样式(如Bold、Italic、Underline等)、大小、位置、透明度

### :memo:保存图像、批量处理图像：
通过*Picture-Save Picture*可指定路径保存添加图框后的图像，无像素及画质损失；通过*Picture-Batch Apply*可选择多个jpg图像，依据当前图框样式在同路径下完成自动套框

### :bookmark:图框样式的保存与调用：
通过*Preset-Save Preset*可将当前样式保存为一个json文件；若要应用图框样式文件，可通过*Preset-Open Preset*调取；程序内嵌9个样式文件，可通过*Preset-Use Primary Preset*随时调用。

## :ambulance:注意事项：
1. 程序在启动时，仅提供Picture-Open Picture操作，待打开图像时启用全部功能
2. 若图像缺失exif信息，打开时将忽略
3. 批量处理图像时，程序会跳过缺失exif信息的图像


## Summary:   
> This is an automatic picture-framing program, supporting custom styles and batch processing. Its specific functions include opening an image, adding a frame on it, saving the framed picture, batch processing for multiple local images, as well as frame style customizing and saving.

## Function Description：
### :art:Open the image and edit the Frame style:
*Picture Open Picture* can open one image. After selecting a .jpg image, the program will automatically extract the exif information of it, including photography parameters(f_stop, exposure time, ISO, etc.), shooting time, and camera brand. Then a frame with the above information will be added to the image. The parameters that can be changed are:
1. The size of the frame and the position of the information display (located at the bottom or right of the image)
2. Camera brand logo (currently only Sony,Nikon and Canon are supported) and its size and location
3. Time information and its font(optional font based on local font library), font style(Bold, Italic, underline, etc.), size, position, and transparency
4. Photography parameter information, and its font(optional font based on local font library), font style(Bold, Italic, underline, etc.), size, position, and transparency

### :memo:Saving Picture/Pictures:
By using *Picture Save Picture*, you can specify a path to save the framed image, without any pixel or image quality loss; Multiple images can be selected through *Picture Batch Apply*, and automatic framing can be completed in the same path based on the current frame style

### :bookmark:Saving/Using Frame Style:
By using *Preset Save Preset*, the current frame style can be saved as a .json file; *Preset Open Preset* allows you to apply a frame style through a .json file; The program is embedded with 9 style files, which can be called at any time through *Preset Use Primary Preset*.

## :ambulance:notes:
1. When the program starts, only *Picture-Open Picture* function is provided, and all functions are enabled when the image is opened
2. If the opened image lacks exif information, the missing information will be ignored
3. When batching images, the program will skip images which lack exif information

![image](https://raw.githubusercontent.com/DevilHamster/PictureFrame/master/Demonstration.png)
