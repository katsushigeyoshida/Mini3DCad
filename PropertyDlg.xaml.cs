using CoreLib;
using System.Windows;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Mini3DCad
{
    /// <summary>
    /// PropertyDlg.xaml の相互作用ロジック
    /// </summary>
    public partial class PropertyDlg : Window
    {
        public string mName = "";
        public Brush mLineColor = Brushes.Black;
        public Brush mFaceColor = Brushes.Blue;
        public string mLineColorName = "Black";
        public string mFaceColorName = "Blue";
        public int mLineFont = 0;
        public bool mLineFontOn = true;
        public bool mFaceColorNull = false;
        public bool mBothShading = true;
        public bool mDisp3D = true;
        public bool mReverseOn = false;
        public bool mReverse = false;
        public bool mDivideAngOn = false;
        public double mDivideAng = 15;

        private string[] mLineFontName = new string[] {
            "実線", "破線", "一点鎖線", "二点鎖線"};
        private YLib ylib = new YLib();

        public PropertyDlg()
        {
            InitializeComponent();

            //lbNameTitle.Visibility = Visibility.Hidden;
            //tbName.Visibility = Visibility.Hidden;
            cbLineColor.DataContext = ylib.mBrushList;
            cbFaceColor.DataContext = ylib.mBrushList;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbName.Text = mName;
            int colorindex = ylib.getBrushNo(mLineColor);
            if (0 <= colorindex)
                cbLineColor.SelectedIndex = colorindex;
            colorindex = ylib.getBrushNo(mFaceColor);
            if (!mFaceColorNull && 0 <= colorindex)
                cbFaceColor.SelectedIndex = ylib.getBrushNo(mFaceColor);
            cbLineFont.ItemsSource = mLineFontName;
            cbLineFont.SelectedIndex = mLineFont;
            cbLineFont.IsEnabled = mLineFontOn;
            chFaceColor.IsChecked = mFaceColorNull;
            chShading.IsChecked = mBothShading;
            chDisp3D.IsChecked = mDisp3D;
            //lbReverseTitle.Visibility = mReverseOn ? Visibility.Visible : Visibility.Collapsed;
            //chReverse.Visibility = mReverseOn ? Visibility.Visible : Visibility.Collapsed;
            chReverse.IsEnabled = mReverseOn ? true : false;
            chReverse.IsChecked = mReverse;
            //lbDivideAngTitle.Visibility = mDivideAngOn ? Visibility.Visible : Visibility.Collapsed;
            //tbDivideAng.Visibility = mDivideAngOn ? Visibility.Visible : Visibility.Collapsed;
            tbDivideAng.IsEnabled = mDivideAngOn ? true : false;
            tbDivideAng.Text = ylib.double2StrZeroSup(mDivideAng);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            mName = tbName.Text;
            if (0 <= cbLineColor.SelectedIndex) {
                mLineColor = ylib.mBrushList[cbLineColor.SelectedIndex].brush;
                mLineColorName = ylib.mBrushList[cbLineColor.SelectedIndex].colorTitle;
            }
            mLineFont = cbLineFont.SelectedIndex;
            if (0 <= cbFaceColor.SelectedIndex) {
                mFaceColor = ylib.mBrushList[cbFaceColor.SelectedIndex].brush;
                mFaceColorName = ylib.mBrushList[cbFaceColor.SelectedIndex].colorTitle;
            }
            mFaceColorNull = chFaceColor.IsChecked == true;
            mBothShading = (chShading.IsChecked == true);
            mDisp3D = chDisp3D.IsChecked == true;
            mReverse = chReverse.IsChecked == true;
            mDivideAng = ylib.doubleParse(tbDivideAng.Text, 10);

            DialogResult = true;
            Close();
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
