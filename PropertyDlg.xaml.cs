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
        public bool mNameEnable = false;
        public Brush mLineColor = Brushes.Black;
        public string mLineColorName = "Black";
        public bool mLineColoeEnable = false;
        public Brush mFaceColor = Brushes.Blue;
        public string mFaceColorName = "Blue";
        public bool mFaceColorNull = false;
        public bool mFaceColorEnable = false;
        public int mLineFont = 0;
        public bool mLineFontOn = true;
        public bool mLineFontEnable = false;
        public bool mBothShading = true;
        public bool mBothShadingEnable = false;
        public bool mDisp3D = true;
        public bool mDisp3DEnable = false;
        public double mArcRadius = 1;
        public bool mArcRadiusEnable = false;
        public bool mArcRadiusOn = true;
        public double mArcStartAngle = 0;
        public bool mArcStartAngleEnable = false;
        public double mArcEndAngle = Math.PI * 2;
        public bool mArcOn = false;
        public bool mArcEndAngleEnable = false;
        public bool mReverseOn = false;
        public bool mReverse = false;
        public bool mReverseEnable = false;
        public bool mDivideAngOn = false;
        public double mDivideAng = 15;
        public bool mDivideAngEnable = false;
        public bool mPropertyAll = false;
        public List<CheckBoxListItem> mChkList;
        public bool mCkkListEnable = false;

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
            cbLineFont.ItemsSource   = mLineFontName;
            cbLineFont.SelectedIndex = mLineFont;
            cbLineFont.IsEnabled     = mLineFontOn;
            chFaceColor.IsChecked    = mFaceColorNull;
            chBothShading.IsChecked  = mBothShading;
            chDisp3D.IsChecked       = mDisp3D;
            tbArcRadius.Text         = mArcRadius.ToString();
            tbArcStartAngle.Text     = ylib.double2StrZeroSup(mArcStartAngle);
            tbArcEndAngle.Text       = ylib.double2StrZeroSup(mArcEndAngle);
            //lbReverseTitle.Visibility = mReverseOn ? Visibility.Visible : Visibility.Collapsed;
            //chReverse.Visibility = mReverseOn ? Visibility.Visible : Visibility.Collapsed;
            chReverse.IsEnabled = mReverseOn ? true : false;
            chReverse.IsChecked = mReverse;
            //lbDivideAngTitle.Visibility = mDivideAngOn ? Visibility.Visible : Visibility.Collapsed;
            //tbDivideAng.Visibility = mDivideAngOn ? Visibility.Visible : Visibility.Collapsed;
            tbDivideAng.IsEnabled = mDivideAngOn ? true : false;
            tbDivideAng.Text      = ylib.double2StrZeroSup(mDivideAng);
            cbLayerList.ItemsSource = mChkList;

            if (!mArcOn) {
                tbArcRadius.IsEnabled = false;
                tbArcStartAngle.IsEnabled = false;
                tbArcEndAngle.IsEnabled = false;
            }
            if (!mArcRadiusOn)
                tbArcRadius.IsEnabled = false;


            if (!mPropertyAll) {
                chNameEnable.Visibility = Visibility.Hidden;
                chLineColorEnable.Visibility = Visibility.Hidden;
                chFaceColorEnable.Visibility = Visibility.Hidden;
                chLineFontEnable.Visibility = Visibility.Hidden;
                chBothShadingEnable.Visibility = Visibility.Hidden;
                chDisp3DEnable.Visibility = Visibility.Hidden;
                chArcRadiusEnable.Visibility = Visibility.Hidden;
                chArcStartAngleEnable.Visibility = Visibility.Hidden;
                chArcEndAngleEnable.Visibility = Visibility.Hidden;
                chReverseEnable.Visibility = Visibility.Hidden;
                chDivideAngEnable.Visibility = Visibility.Hidden;
                chLayerListEnable.Visibility = Visibility.Hidden;
            }
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
            mNameEnable = chNameEnable.IsChecked == true;
            mLineFont = cbLineFont.SelectedIndex;
            mLineFontEnable = chLineFontEnable.IsChecked == true;
            if (0 <= cbFaceColor.SelectedIndex) {
                mFaceColor = ylib.mBrushList[cbFaceColor.SelectedIndex].brush;
                mFaceColorName = ylib.mBrushList[cbFaceColor.SelectedIndex].colorTitle;
            }
            mLineColoeEnable = chLineColorEnable.IsChecked == true;
            mFaceColorNull = chFaceColor.IsChecked == true;
            mFaceColorEnable = chFaceColor.IsChecked == true;
            mBothShading = (chBothShading.IsChecked == true);
            mBothShadingEnable = chBothShading.IsChecked == true;
            mDisp3D = chDisp3D.IsChecked == true;
            mDisp3DEnable = chDisp3DEnable.IsChecked == true;
            mArcRadius = ylib.doubleParse(tbArcRadius.Text, 1);
            mArcStartAngle = ylib.doubleParse(tbArcStartAngle.Text, 1);
            mArcEndAngle = ylib.doubleParse(tbArcEndAngle.Text, 1);
            mReverse = chReverse.IsChecked == true;
            mReverseEnable = chReverseEnable.IsChecked == true;
            mDivideAng = ylib.doubleParse(tbDivideAng.Text, 10);
            mDivideAngEnable = chDivideAngEnable.IsChecked == true;
            mCkkListEnable = chLayerListEnable.IsChecked == true;

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
