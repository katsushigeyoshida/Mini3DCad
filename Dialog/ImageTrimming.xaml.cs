using CoreLib;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Mini3DCad
{
    /// <summary>
    /// ImageTrimming.xaml の相互作用ロジック
    /// </summary>
    public partial class ImageTrimming : Window
    {
        public int mBitmapWidth = 0;            //  画像の幅
        public int mBitmapHeight = 0;           //  画像の高さ
        public int mWidth = 0;                  //  画像の指定幅
        public int mHeight = 0;                 //  画像の指定高さ
        public BitmapSource mBitmapSource;      //  画像データ
        public bool mCancelEnable = true;

        private YLib ylib = new YLib();

        public ImageTrimming()
        {
            InitializeComponent();

            cbAspect.IsChecked = true;
            if (cbAspect.IsChecked == true)
                tbHeight.IsReadOnly = true;
            btCopy.IsEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!mCancelEnable)
                btCancel.Visibility = Visibility.Collapsed;

            if (System.Windows.Clipboard.ContainsImage()) {
                //  クリップボードに画像データがある
                mBitmapSource = System.Windows.Clipboard.GetImage();
                if (mBitmapSource != null) {
                    Bitmap bitmap = ylib.cnvBitmapSource2Bitmap(mBitmapSource);
                    mBitmapWidth = bitmap.Width;
                    mBitmapHeight = bitmap.Height;
                    imImageView.Stretch = Stretch.Fill;
                    lbImageSize.Content = $"{bitmap.Width} x {bitmap.Height}";      //  画像の大きさ
                    tbWidth.Text = bitmap.Width.ToString();
                    tbHeight.Text = bitmap.Height.ToString();
                    tbRateWidth.Text = 100.ToString();
                    tbRateHeight.Text = 100.ToString();
                    imImageView.Source = ylib.bitmap2BitmapSource(bitmap);
                }
            } else {
                Close();
            }
        }

        /// <summary>
        /// アスペクト比固定のチェックボックス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbAspect_Click(object sender, RoutedEventArgs e)
        {
            if (cbAspect.IsChecked == true) {
                tbHeight.IsReadOnly = true;
                int width = ylib.intParse(tbWidth.Text, 0);
                tbHeight.Text = (width * mBitmapHeight / mBitmapWidth).ToString();
                tbRateHeight.IsReadOnly = true;
                tbRateHeight.Text = (width * 100 / mBitmapWidth).ToString();
            } else {
                tbHeight.IsReadOnly = false;
                tbRateHeight.IsReadOnly = false;
            }
        }

        /// <summary>
        /// 画像幅を入力した時、高さを入れる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbWidth_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int width = ylib.intParse(tbWidth.Text, 0);
            if (cbAspect.IsChecked == true) {
                tbHeight.Text = (width * mBitmapHeight / mBitmapWidth).ToString();
                tbRateHeight.Text = (width * 100 / mBitmapWidth).ToString();
            }
            tbRateWidth.Text = (width * 100 / mBitmapWidth).ToString();
        }

        /// <summary>
        /// 画像の幅倍率の入力時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbRateWidth_KeyUp(object sender, KeyEventArgs e)
        {
            int ratewidth = ylib.intParse(tbRateWidth.Text, 0);
            if (cbAspect.IsChecked == true) {
                tbHeight.Text = (mBitmapHeight * ratewidth / 100).ToString();
                tbRateHeight.Text = ratewidth.ToString();
            }
            tbWidth.Text = (mBitmapWidth * ratewidth / 100).ToString();
        }

        /// <summary>
        /// 画像高さの入力時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbHeight_KeyUp(object sender, KeyEventArgs e)
        {
            int height = ylib.intParse(tbHeight.Text, 0);
            tbRateHeight.Text = (height * 100 / mBitmapHeight).ToString();
        }

        /// <summary>
        /// 画像高さ倍率入力時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbRateHeight_KeyUp(object sender, KeyEventArgs e)
        {
            int rateHeight = ylib.intParse(tbRateHeight.Text, 0);
            tbHeight.Text = (rateHeight * mBitmapHeight / 100).ToString();
        }

        /// <summary>
        /// [OK]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            mWidth = ylib.intParse(tbWidth.Text);
            mHeight = ylib.intParse(tbHeight.Text);
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// [Cancel]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// [トリミング]キャプチャしたイメージを表示しトリミングする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btTriming_Click(object sender, RoutedEventArgs e)
        {
            //  キャプチャしたイメージを表示し領域を切り取る
            FullView dlg = new FullView();
            dlg.mBitmapSource = mBitmapSource;
            dlg.mFullScreen = false;
            if (dlg.ShowDialog() == true) {
                Bitmap bitmap = ylib.cnvBitmapSource2Bitmap(mBitmapSource);
                bitmap = ylib.trimingBitmap(bitmap, dlg.mStartPoint, dlg.mEndPoint);
                if (bitmap != null) {
                    //  切り取った領域を貼り付ける
                    mBitmapSource = ylib.bitmap2BitmapSource(bitmap);
                    imImageView.Source = mBitmapSource;
                    //  画像の大きさ
                    mBitmapWidth = bitmap.Width;
                    mBitmapHeight = bitmap.Height;
                    tbWidth.Text = bitmap.Width.ToString();
                    tbHeight.Text = bitmap.Height.ToString();
                    lbImageSize.Content = $"{bitmap.Width} x {bitmap.Height}";
                    btCopy.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// [コピー]トリミングしたイメージをクリップボードに戻す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCopy_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetImage(mBitmapSource);
        }

        /// <summary>
        /// [保存]イメージをファイルに保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            List<string[]> mImageFilters = new List<string[]>() {
                    new string[] { "PNGファイル", "*.png" },
                    new string[] { "JPGファイル", "*.jpg" },
                    new string[] { "JPEGファイル", "*.jpeg" },
                    new string[] { "GIFファイル", "*.gif" },
                    new string[] { "BMPファイル", "*.bmp" },
                    new string[] { "すべてのファイル", "*.*"}
            };
            string path = ylib.fileSaveSelectDlg("イメージ保存", ".", mImageFilters);
            if (0 < path.Length) {
                if (Path.GetExtension(path).Length == 0)
                    path += ".png";
                ylib.saveBitmapImage(mBitmapSource, path);
            }
        }
    }
}
